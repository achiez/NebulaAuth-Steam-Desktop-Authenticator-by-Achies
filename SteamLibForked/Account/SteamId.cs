using SteamLib.Utility;

namespace SteamLib.Account;

public struct SteamId //TODO: validation in parse methods
{
    public SteamId64 Steam64 { get; }
    public SteamId2 Steam2 { get; }
    public SteamId3 Steam3 { get; }
    public SteamId(SteamId64 steam64, char type = 'U', short universe = 0)
    {
        Steam64 = steam64;
        Steam2 = steam64.ToSteam2(universe);
        Steam3 = steam64.ToSteam3(type);
    }

    public SteamId(SteamId2 steam2, char type = 'U')
    {
        Steam2 = steam2;
        Steam64 = steam2.ToSteam64();
        Steam3 = steam2.ToSteam3(type);
    }

    public SteamId(SteamId3 steam3, byte universe = 0)
    {
        Steam3 = steam3;
        Steam64 = steam3.ToSteam64();
        Steam2 = steam3.ToSteam2(universe);
    }

    public static SteamId FromSteam64(long steam64, char type = 'U', short universe = 0) => new(new SteamId64(steam64), type, universe);
    public override string ToString() => Steam64.ToString();
    public static bool TryParse(string input, out SteamId result) => SteamIdParser.TryParse(input, out result);

    /// <exception cref="FormatException"></exception>
    public static SteamId Parse(string input) => SteamIdParser.Parse(input);
    public static bool operator ==(SteamId left, SteamId right) => left.Equals(right);
    public static bool operator !=(SteamId left, SteamId right) => !left.Equals(right);
    public bool Equals(SteamId other) => Steam64.Equals(other.Steam64);
    public override bool Equals(object? obj) => obj is SteamId other && Equals(other);
    public override int GetHashCode() => Steam64.GetHashCode();
}

public struct SteamId64
{
    public const long SEED = 76561197960265728L;
    public long Id { get; }
    public SteamId64(long id)
    {
        if (id < SEED)
            throw new ArgumentOutOfRangeException(nameof(id),$"Invalid SteamID provided {id}");
        Id = id;
    }

    public SteamId2 ToSteam2(short universe = 0)
    {
        var accountIdLowBit = (byte)(Id & 1);
        var accountIdHighBits = (int)(Id >> 1) & 0x7FFFFFF;
        return new SteamId2((byte)universe, accountIdLowBit, accountIdHighBits);
    }

    public SteamId3 ToSteam3(char type = 'U')
    {
        var accountIdLowBit = (byte)(Id & 1);
        var accountIdHighBits = (int)(Id >> 1) & 0x7FFFFFF;
        return new SteamId3(accountIdLowBit + accountIdHighBits * 2, type);
    }


    public override string ToString()
    {
        return Id.ToString();
    }

    public ulong ToUlong() => (ulong)Id;
    public long ToLong() => Id;

    public bool Equals(SteamId64 other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is SteamId64 other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(SteamId64 left, SteamId64 right) => left.Equals(right);
    public static bool operator !=(SteamId64 left, SteamId64 right) => !left.Equals(right);
    public static bool TryParse(string input, out SteamId64 result) => SteamIdParser.TryParse64(input, out result);
    /// <exception cref="FormatException"></exception>
    public static SteamId64 Parse(string input) => SteamIdParser.Parse64(input);

    public static implicit operator long(SteamId64 steamId64) => steamId64.ToLong();
}

public struct SteamId2
{

    public byte Universe { get; }
    public byte LowestBit { get; }
    public int HighestBit { get; }


    public SteamId2(byte lowestBit, int highestBit)
    {
        if(lowestBit > 1)
            throw new ArgumentOutOfRangeException(nameof(lowestBit), $"Invalid SteamID2 lowestBit provided {lowestBit}. Max value is 1");

        if(highestBit < 0)
            throw new ArgumentOutOfRangeException(nameof(highestBit), $"Invalid SteamID2 highestBit provided {highestBit}");

        LowestBit = lowestBit;
        HighestBit = highestBit;
        Universe = 0;
    }

    public SteamId2(byte universe, byte lowestBit, int highestBit)
    {
        Universe = universe;
        LowestBit = lowestBit;
        HighestBit = highestBit;
    }

    public SteamId64 ToSteam64() => new SteamId64(SteamId64.SEED + LowestBit + HighestBit * 2);

    public SteamId3 ToSteam3(char type = 'U') => new SteamId3(HighestBit * 2 + LowestBit);

    public override string ToString() => $"STEAM_{Universe}:{LowestBit}:{HighestBit}";

    public bool Equals(SteamId2 other) => Universe == other.Universe && LowestBit == other.LowestBit && HighestBit == other.HighestBit;
    public override bool Equals(object? obj) => obj is SteamId2 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Universe, LowestBit, HighestBit);
    public static bool operator ==(SteamId2 left, SteamId2 right) => left.Equals(right);
    public static bool operator !=(SteamId2 left, SteamId2 right) => !left.Equals(right);
    public static bool TryParse(string input, out SteamId2 result) => SteamIdParser.TryParse2(input, out result);
    /// <exception cref="FormatException"></exception>
    public static SteamId2 Parse(string input) => SteamIdParser.Parse2(input);
}

public struct SteamId3
{
    public char Type { get; }
    public int Id { get; }

    public SteamId3(int id, char type = 'U')
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), $"Invalid SteamID provided {id}");
        Type = type;
        Id = id;
    }

    public SteamId2 ToSteam2(byte universe = 0)
    {
        var bit = Id % 2;
        var highestBits = Id / 2;
        return new SteamId2(universe, (byte)bit, highestBits);
    }


    public SteamId64 ToSteam64() => new SteamId64(SteamId64.SEED + Id);
    public override string ToString() => $"[{Type}:1:{Id}]";

    public string ToString(bool withBrackets)
    {
        if (withBrackets)
        {
            return ToString();
        }
        else
        {
            return $"{Type}:1:{Id}";
        }
    }

    public int ToInt() => Id;
    public bool Equals(SteamId3 other) => Type == other.Type && Id == other.Id;
    public override bool Equals(object? obj) => obj is SteamId3 other && Equals(other);
    public readonly override int GetHashCode() => HashCode.Combine(Type, Id);
    public static bool operator ==(SteamId3 left, SteamId3 right) => left.Equals(right);
    public static bool operator !=(SteamId3 left, SteamId3 right) => !left.Equals(right);
    public static bool TryParse(string input, out SteamId3 result) => SteamIdParser.TryParse3(input, out result);

    /// <exception cref="FormatException"></exception>
    public static SteamId3 Parse(string input) => SteamIdParser.Parse3(input);
}