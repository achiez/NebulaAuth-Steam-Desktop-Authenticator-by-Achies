using SteamLib.Utility;
using SteamLibForked.Models.SteamIds;

public readonly struct SteamId : IEquatable<SteamId> //TODO: validation in parse methods (in siblings also)
{
    public SteamId64 Steam64 { get; }
    public SteamId2 Steam2 { get; }
    public SteamId3 Steam3 { get; }


    public SteamId(SteamId64 steam64, SteamId2 steam2, SteamId3 steam3)
    {
        Steam64 = steam64;
        Steam2 = steam2;
        Steam3 = steam3;
    }

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

    public static SteamId FromSteam64(long steam64, char type = 'U', short universe = 0)
    {
        return new SteamId(new SteamId64(steam64), type, universe);
    }

    public static SteamId FromSteam64(ulong steam64, char type = 'U', short universe = 0)
    {
        return new SteamId(new SteamId64((long) steam64), type, universe);
    }

    public static SteamId FromSteam2(byte lowestBit, int highestBit, byte universe = 0, char type = 'U')
    {
        return new SteamId(new SteamId2(lowestBit, highestBit), type);
    }

    public static SteamId FromSteam3(int steam3, char type = 'U')
    {
        return new SteamId(new SteamId3(steam3, type));
    }

    public override string ToString()
    {
        return Steam64.ToString();
    }

    public static bool TryParse(string input, out SteamId result)
    {
        return SteamIdParser.TryParse(input, out result);
    }

    /// <exception cref="FormatException"></exception>
    public static SteamId Parse(string input)
    {
        return SteamIdParser.Parse(input);
    }

    public static bool operator ==(SteamId left, SteamId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SteamId left, SteamId right)
    {
        return !left.Equals(right);
    }

    public bool Equals(SteamId other)
    {
        return Steam64.Equals(other.Steam64);
    }

    public override bool Equals(object? obj)
    {
        return obj is SteamId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Steam64.GetHashCode();
    }

    public static implicit operator SteamId(SteamId64 steamId64)
    {
        return new SteamId(steamId64);
    }

    public static implicit operator SteamId(SteamId2 steamId2)
    {
        return new SteamId(steamId2);
    }

    public static implicit operator SteamId(SteamId3 steamId3)
    {
        return new SteamId(steamId3);
    }

    public static implicit operator SteamId64(SteamId steamId)
    {
        return steamId.Steam64;
    }

    public static implicit operator SteamId2(SteamId steamId)
    {
        return steamId.Steam2;
    }

    public static implicit operator SteamId3(SteamId steamId)
    {
        return steamId.Steam3;
    }
}