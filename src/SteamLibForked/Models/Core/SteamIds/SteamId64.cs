using SteamLib.Utility;

namespace SteamLibForked.Models.SteamIds;

public readonly struct SteamId64 : IEquatable<SteamId64>
{
    public const long SEED = 76561197960265728L;
    public long Id { get; }

    public SteamId64(long id)
    {
        if (id < SEED)
            throw new ArgumentOutOfRangeException(nameof(id), $"Invalid SteamID provided {id}");
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

    public ulong ToUlong()
    {
        return (ulong)Id;
    }

    public long ToLong()
    {
        return Id;
    }

    public bool Equals(SteamId64 other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is SteamId64 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(SteamId64 left, SteamId64 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SteamId64 left, SteamId64 right)
    {
        return !left.Equals(right);
    }

    public static bool TryParse(string? input, out SteamId64 result)
    {
        return SteamIdParser.TryParse64(input, out result);
    }

    public static bool TryParse(long input, out SteamId64 result)
    {
        return SteamIdParser.TryParse64(input, out result);
    }

    /// <exception cref="FormatException"></exception>
    public static SteamId64 Parse(string input)
    {
        return SteamIdParser.Parse64(input);
    }

    public static implicit operator long(SteamId64 steamId64)
    {
        return steamId64.ToLong();
    }
}