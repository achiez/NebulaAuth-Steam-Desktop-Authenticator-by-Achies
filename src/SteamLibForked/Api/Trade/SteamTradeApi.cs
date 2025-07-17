using SteamLib.Core;

namespace SteamLib.Api.Trade;

public static class SteamTradeApi
{
    public static async Task<bool> Acknowledge(HttpClient client, string sessionId,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, string>
        {
            {"sessionid", sessionId},
            {"message", "1"}
        };
        var req = new HttpRequestMessage(HttpMethod.Post, Routes.ACKNOWLEDGE);
        req.Content = new FormUrlEncodedContent(data);
        var resp = await client.SendAsync(req, cancellationToken);
        var cont = await resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken);
        return cont == "[]";
        //{"success":false} - unauthenticated
    }

    public static class Routes
    {
        public const string ACKNOWLEDGE = SteamConstants.STEAM_COMMUNITY + "//trade/new/acknowledge"; //Not a typo
    }
}