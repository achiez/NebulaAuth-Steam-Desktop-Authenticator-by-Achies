using System;
using System.Net.Http;
using System.Threading.Tasks;
using NebulaAuth.Model;
using Newtonsoft.Json;

namespace NebulaAuth.Core;

public static class LinksManager
{
    private const string LINKS_URL =
        "https://raw.githubusercontent.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/master/NebulaAuth/links.json";

    private static readonly HttpClient HttpClient = new();

    public static string? WebsiteUrl { get; private set; }
    public static string? DocumentationUrl { get; private set; }

    public static async Task FetchAsync()
    {
        try
        {
            var json = await HttpClient.GetStringAsync(LINKS_URL).ConfigureAwait(false);
            var links = JsonConvert.DeserializeObject<RemoteLinks>(json);
            WebsiteUrl = links?.Website;
            DocumentationUrl = links?.Documentation;
        }
        catch (Exception ex)
        {
            Shell.Logger.Debug(ex, "Failed to fetch remote links");
        }
    }
}