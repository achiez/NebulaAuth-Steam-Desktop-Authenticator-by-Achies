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
        var userRegion = new RegionInfo(userCulture.Name).TwoLetterISORegionName;


        switch (userLang)
        {
            case "ru": return LocalizationLanguage.Russian;
            case "uk": return LocalizationLanguage.Ukrainian;
            case "en": return LocalizationLanguage.English;
            case "zh": return LocalizationLanguage.ChineseSimplified;
            case "fr": return LocalizationLanguage.French;
        }

        if (userRegion.EndsWith("UA", StringComparison.OrdinalIgnoreCase))
            return LocalizationLanguage.Ukrainian;

        string[] cisRegions =
        [
            "RU", "BY", "KZ", "KG", "TJ", "TM", "UZ", "AM", "AZ", "GE", "MD"
        ];

        return cisRegions.Any(r => userRegion.EndsWith(r, StringComparison.OrdinalIgnoreCase)) 
            ? LocalizationLanguage.Russian 
            : LocalizationLanguage.English;
    }
}