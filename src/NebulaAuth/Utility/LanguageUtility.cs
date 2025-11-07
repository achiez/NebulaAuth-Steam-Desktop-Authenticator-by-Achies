using System;
using System.Globalization;
using System.Linq;
using NebulaAuth.Core;

namespace NebulaAuth.Utility;

public static class LanguageUtility
{
    public static LocalizationLanguage DetectPreferredLanguage()
    {
        var userCulture = CultureInfo.CurrentUICulture;
        var userLang = userCulture.TwoLetterISOLanguageName;
        var userRegion = userCulture.Name;

        switch (userLang)
        {
            case "ru": return LocalizationLanguage.Russian;
            case "uk": return LocalizationLanguage.Ukrainian;
            case "en": return LocalizationLanguage.English;
        }

        if (userRegion.EndsWith("UA", StringComparison.OrdinalIgnoreCase))
            return LocalizationLanguage.Ukrainian;

        string[] cisRegions =
        [
            "RU", "BY", "KZ", "KG", "TJ", "TM", "UZ", "AM", "AZ", "GE", "MD"
        ];

        if (cisRegions.Any(r => userRegion.EndsWith(r, StringComparison.OrdinalIgnoreCase)))
            return LocalizationLanguage.Russian;

        return LocalizationLanguage.English;
    }
}