using AchiesUtilities.Newtonsoft.JSON;
using Newtonsoft.Json;

namespace SteamLib.Web.Scrappers.JSON;

public static class StaticJson //TODO: remove?
{
    public static JsonSerializerSettings DefaultSettings { get; }


    static StaticJson()
    {
        DefaultSettings = new JsonSerializerSettings
        {
            Converters = StaticConverters.AllDefaultConverters
        };
    }
}