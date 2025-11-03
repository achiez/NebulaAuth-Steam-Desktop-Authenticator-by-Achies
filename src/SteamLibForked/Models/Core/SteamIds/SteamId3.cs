using SteamLib.Utility;

namespace SteamLibForked.Models.SteamIds;

public readonly struct SteamId3 : IEquatable<SteamId3>
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


    public SteamId64 ToSteam64()
    {
        return new SteamId64(SteamId64.SEED + Id);
    }

    public override string ToString()
    {
        return $"[{Type}:1:{Id}]";
    }

    public string ToString(bool withBrackets)
    {
        if (withBrackets)
        {
            return ToString();
        }

        return $"{Type}:1:{Id}";
    }

    public int ToInt()
    {
        return Id;
    }

    public bool Equals(SteamId3 other)
    {
        return Type == other.Type && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is SteamId3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Id);
    }

    public static bool operator ==(SteamId3 left, SteamId3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SteamId3 left, SteamId3 right)
    {
        return !left.Equals(right);
    }

    public static bool TryParse(string input, out SteamId3 result)
    {
        return SteamIdParser.TryParse3(input, out result);
    }

    /// <exception cref="FormatException"></exception>
    public static SteamId3 Parse(string input)
    {
        return SteamIdParser.Parse3(input);
    }
}