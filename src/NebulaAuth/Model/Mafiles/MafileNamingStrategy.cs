using NebulaAuth.Model.Entities;
using NebulaAuth.Utility;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NebulaAuth.Model.Mafiles;

public interface IMafileNamingStrategy
{
    bool CanNameMafile(Mafile mafile);
    string GetMafileName(Mafile mafile);
}

public class MafileNamingStrategy : IMafileNamingStrategy
{
    public static MafileNamingStrategy SteamId { get; } = new("{steamid}");
    public static MafileNamingStrategy Login { get; } = new("{login}");
    public const string DEF_EXTENSION = ".mafile";

    public static readonly IDictionary<string, Func<Mafile, string?>> Placeholders =
        new Dictionary<string, Func<Mafile, string?>>
        {
            {"{steamid}", GetSteamId},
            {"{login}", GetLogin},
            {"{group}", GetGroup},
            {"{servertime}", GetServerTime}
        }.ToFrozenDictionary();


    public string Pattern { get; }
    public bool IncludeExtension { get; }

    public MafileNamingStrategy(string pattern, bool includeExtension = true)
    {
        Pattern = pattern;
        IncludeExtension = includeExtension;
    }

    public bool CanNameMafile(Mafile mafile)
    {
        if (string.IsNullOrWhiteSpace(Pattern)) return false;
        var fileName = ApplyPatternSubstitution(mafile, Pattern, IncludeExtension);
        return FileNameValidator.IsValidFileName(fileName);
    }

    public string GetMafileName(Mafile mafile)
    {
        if (!CanNameMafile(mafile)) throw new InvalidOperationException($"Cannot name mafile with the current pattern {Pattern}");
        return ApplyPatternSubstitution(mafile, Pattern, IncludeExtension);
    }

    private static string ApplyPatternSubstitution(Mafile mafile, string pattern, bool includeExtension)
    {
        var result = pattern;
        foreach (var placeholder in Placeholders)
        {
            if (result.Contains(placeholder.Key, StringComparison.OrdinalIgnoreCase))
            {
                var value = placeholder.Value(mafile);
                result = result.Replace(placeholder.Key, value, StringComparison.OrdinalIgnoreCase);
            }
        }
        return includeExtension ? result + DEF_EXTENSION : result;
    }

    private static string GetSteamId(Mafile mafile) => mafile.SteamId.Steam64.ToString();
    private static string GetLogin(Mafile mafile) => mafile.AccountName;
    private static string? GetGroup(Mafile mafile) => mafile.Group;
    private static string GetServerTime(Mafile mafile) => mafile.ServerTime.ToString();
}