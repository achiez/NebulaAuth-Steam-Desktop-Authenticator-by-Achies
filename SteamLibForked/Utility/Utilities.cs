﻿using System.Text.RegularExpressions;

namespace SteamLib.Utility;

public static class Utilities //TODO: refactor
{
    private static readonly Regex RegexInt = new("\"success\":\\s?(\\d*)", RegexOptions.Compiled);
    private static readonly Regex RegexBool = new("\"success\":\\s?(\\w*)", RegexOptions.Compiled);

    public static int GetSuccessCode(string response)
    {
        var length = Math.Min(response.Length, 100);
        var matchInt = RegexInt.Match(response, 0, length);
        if (!string.IsNullOrEmpty(matchInt.Groups[1].Value))
            return Convert.ToInt32(matchInt.Groups[1].Value);


        var matchBool = RegexBool.Match(response, 0, length);

        if (!string.IsNullOrEmpty(matchBool.Groups[1].Value))
            return bool.Parse(matchBool.Groups[1].Value) ? 1 : 0;
        return 0;
    }
}