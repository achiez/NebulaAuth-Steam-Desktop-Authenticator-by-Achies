using SteamLib.Utility;

namespace SteamLib.Core.Models;

public readonly struct SteamId2 : IEquatable<SteamId2>
{
    public byte Universe { get; }
    public byte LowestBit { get; }
    public int HighestBit { get; }


    public SteamId2(byte lowestBit, int highestBit)
    {
        if (lowestBit > 1)
            throw new ArgumentOutOfRangeException(nameof(lowestBit),
                $"Invalid SteamID2 lowestBit provided {lowestBit}. Max value is 1");

        if (highestBit < 0)
            throw new ArgumentOutOfRangeException(nameof(highestBit),
                $"Invalid SteamID2 highestBit provided {highestBit}");

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

    public SteamId64 ToSteam64()
    {
        return new SteamId64(SteamId64.SEED + LowestBit + HighestBit * 2);
    }

    public SteamId3 ToSteam3(char type = 'U')
    {
        return new SteamId3(HighestBit * 2 + LowestBit, type);
    }

    public override string ToString()
    {
        return $"STEAM_{Universe}:{LowestBit}:{HighestBit}";
    }

    public bool Equals(SteamId2 other)
    {
        return Universe == other.Universe && LowestBit == other.LowestBit && HighestBit == other.HighestBit;
    }

    public override bool Equals(object? obj)
    {
        return obj is SteamId2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Universe, LowestBit, HighestBit);
    }

    public static bool operator ==(SteamId2 left, SteamId2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SteamId2 left, SteamId2 right)
    {
        return !left.Equals(right);
    }

    public static bool TryParse(string input, out SteamId2 result)
    {
        return SteamIdParser.TryParse2(input, out result);
    }

    /// <exception cref="FormatException"></exception>
    public static SteamId2 Parse(string input)
    {
        return SteamIdParser.Parse2(input);
    }
}