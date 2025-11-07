using System.Diagnostics;
using AchiesUtilities.Models;
using Newtonsoft.Json.Linq;
using SteamLib.Core;

namespace SteamLib.SteamMobile;

//TODO: Refactor

public static class TimeAligner
{
    private const string TIME_ALIGN_ENDPOINT = "/ITwoFactorService/QueryTime/v0001";
    private static bool _aligned;
    private static int _timeDifference;
    public static UnixTimeStamp UtcNow => UnixTimeStamp.FromDateTime(DateTime.UtcNow);


    public static async ValueTask<long> GetSteamTimeAsync()
    {
        if (!_aligned)
        {
            await AlignTimeAsync();
        }

        return UtcNow.ToLong() + _timeDifference;
    }

    public static long GetSteamTime()
    {
        if (!_aligned)
        {
            AlignTime();
        }

        return UtcNow.ToLong() + _timeDifference;
    }

    public static void AlignTime()
    {
        var client = new HttpClient();
        var sw = new Stopwatch();
        try
        {
            sw.Start();
            var req = new HttpRequestMessage(HttpMethod.Post, SteamConstants.STEAM_API + TIME_ALIGN_ENDPOINT);
            var response = client.Send(req).EnsureSuccessStatusCode();
            sw.Stop();
            using var stream = new StreamReader(response.Content.ReadAsStream());
            var respStr = stream.ReadToEnd();
            var j = JObject.Parse(respStr);
            var time = j["response"]!["server_time"]!.Value<long>();
            var now = UtcNow - sw.Elapsed;
            _timeDifference = (int) (time - now.ToLong());
            _aligned = true;
        }
        finally
        {
            client.Dispose();
        }
    }

    public static async Task AlignTimeAsync()
    {
        var client = new HttpClient();
        var sw = new Stopwatch();
        try
        {
            sw.Start();
            var response = await client.PostAsync(SteamConstants.STEAM_API + TIME_ALIGN_ENDPOINT, null);
            sw.Stop();
            var respStr = await response.Content.ReadAsStringAsync();
            var j = JObject.Parse(respStr);
            var time = j["response"]!["server_time"]!.Value<long>();
            var now = UtcNow - sw.Elapsed;
            _timeDifference = (int) (time - now.ToLong());
            _aligned = true;
        }
        finally
        {
            client.Dispose();
        }
    }
}