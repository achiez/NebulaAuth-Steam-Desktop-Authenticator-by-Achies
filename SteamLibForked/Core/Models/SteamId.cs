using SteamLib.Utility;

namespace SteamLib.Core.Models;

public readonly struct SteamId : IEquatable<SteamId> //TODO: validation in parse methods (in siblings also)
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