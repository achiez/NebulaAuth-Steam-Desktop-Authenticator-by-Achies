using System.Text.RegularExpressions;
using SteamLib.Core.Models;

namespace SteamLib.Utility;

public static class SteamIdParser
{
    public static readonly Regex Steam64Regex = new("^7656119([0-9]{10})$", RegexOptions.Compiled);

    public static readonly Regex Steam2Regex =
        new("STEAM_(?<universe>[0-9]):(?<lowestBit>[0-9]):(?<highestBits>[0-9]{1,10})", RegexOptions.Compiled);

    public static readonly Regex Steam3Regex =
        new("^\\[?(?<type>[a-zA-Z]):1:(?<id>[0-9]{1,10})\\]$", RegexOptions.Compiled);


    #region TryParse

    public static bool TryParse(string input, out SteamId result)
    {
        if (TryParse64(input, out var steam64))
        {
            result = new SteamId(steam64);
            return true;
        }

        if (TryParse2(input, out var steam2))
        {
            result = new SteamId(steam2);
            return true;
        }

        if (TryParse3(input, out var steam3))
        {
            result = new SteamId(steam3);
            return true;
        }


        result = new SteamId(new SteamId2(0, 0));
        return false;
    }

    public static bool TryParse64(string? input, out SteamId64 result)
    {
        result = default;
        if (input == null) return false;
        var match64 = Steam64Regex.Match(input);
        if (match64.Success)
        {
            return TryParse64(long.Parse(match64.Value), out result);
        }

        return false;
    }

    public static bool TryParse64(long input, out SteamId64 result)
    {
        result = default;
        if (input < SteamId64.SEED)
        {
            return false;
        }

        result = new SteamId64(input);
        return true;
    }

    public static bool TryParse2(string input, out SteamId2 result)
    {
        var match2 = Steam2Regex.Match(input);
        if (match2.Success)
        {
            var universe = byte.Parse(match2.Groups["universe"].Value);
            var lowestBit = byte.Parse(match2.Groups["lowestBit"].Value);
            var highestBits = int.Parse(match2.Groups["highestBits"].Value);
            result = new SteamId2(universe, lowestBit, highestBits);
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryParse3(string input, out SteamId3 result)
    {
        var match3 = Steam3Regex.Match(input);
        if (match3.Success)
        {
            var type = match3.Groups["type"].Value[0];
            var id = int.Parse(match3.Groups["id"].Value);
            result = new SteamId3(id, type);
            return true;
        }


        result = default;
        return false;
    }

    #endregion

    #region Parse

    public static SteamId Parse(string input)
    {
        if (TryParse64(input, out var steam64))
        {
            return new SteamId(steam64);
        }

        if (TryParse2(input, out var steam2))
        {
            return new SteamId(steam2);
        }

        if (TryParse3(input, out var steam3))
        {
            return new SteamId(steam3);
        }

        throw new FormatException(
            $"The input string '{input}' was not in a correct format or not real SteamId64, SteamId2 or SteamId3.");
    }

    public static SteamId64 Parse64(string input)
    {
        var match64 = Steam64Regex.Match(input);
        if (match64.Success)
        {
            return new SteamId64(long.Parse(match64.Value));
        }

        throw new FormatException($"The input string '{input}' was not in a correct format or not real SteamId64.");
    }

    public static SteamId2 Parse2(string input)
    {
        var match2 = Steam2Regex.Match(input);
        if (match2.Success)
        {
            var universe = byte.Parse(match2.Groups["universe"].Value);
            var lowestBit = byte.Parse(match2.Groups["lowestBit"].Value);
            var highestBits = int.Parse(match2.Groups["highestBits"].Value);
            return new SteamId2(universe, lowestBit, highestBits);
        }

        throw new FormatException($"The input string '{input}' was not in a correct format or not real SteamId2.");
    }

    public static SteamId3 Parse3(string input)
    {
        var match3 = Steam3Regex.Match(input);
        if (match3.Success)
        {
            var type = match3.Groups["type"].Value[0];
            var id = int.Parse(match3.Groups["id"].Value);
            return new SteamId3(id, type);
        }

        throw new FormatException($"The input string '{input}' was not in a correct format or not real SteamId3.");
    }

    #endregion
}