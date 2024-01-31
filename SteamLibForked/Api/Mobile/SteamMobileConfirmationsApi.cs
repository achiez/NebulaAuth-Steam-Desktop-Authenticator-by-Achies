using SteamLib.Core;
using SteamLib.Exceptions;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.SteamMobile;
using System.Net;
using AchiesUtilities.Web.Extensions;
using SteamLib.Web.Scrappers.JSON;
using SteamLib.Core.StatusCodes;
using SteamLib.Utility;

namespace SteamLib.Api.Mobile;

public static class SteamMobileConfirmationsApi
{
    private const string CONF = SteamConstants.STEAM_COMMUNITY + "mobileconf";
    private const string CONF_OP = SteamConstants.STEAM_COMMUNITY + "mobileconf/ajaxop";
    private const string MULTI_CONF_OP = SteamConstants.STEAM_COMMUNITY + "mobileconf/multiajaxop";

    public static async Task<IEnumerable<Confirmation>> GetConfirmations(HttpClient client, MobileData data, long steamId)
    {
        var nvc = GetConfirmationKvp(steamId, data.DeviceId, data.IdentitySecret, "list");

        var req = new Uri(CONF + "/getlist" + nvc.ToQueryString());
        var reqMsg = new HttpRequestMessage(HttpMethod.Get, req);
        var resp = await client.SendAsync(reqMsg);
        
        var respStr = await resp.Content.ReadAsStringAsync();
        if (resp.StatusCode == HttpStatusCode.Redirect)
        {
            throw new SessionExpiredException("Mobile session expired");
        }

        resp.EnsureSuccessStatusCode();


        return HealthMonitor.LogOnException(respStr, MobileConfirmationScrapper.Scrap, typeof(SessionExpiredException));
    }

    public static async Task<bool> SendConfirmation(HttpClient client, Confirmation confirmation, long steamId, MobileData data, bool confirm)
    {
        var op = confirm ? "allow" : "cancel";

        var query = GetConfirmationKvp(steamId, data.DeviceId, data.IdentitySecret, op).ToList();

        var id = confirmation.Id.ToString();
        var key = confirmation.Nonce.ToString();

        query.Insert(0, new KeyValuePair<string, string>("op", op));
        query.Add(new KeyValuePair<string, string>("cid", id));
        query.Add(new KeyValuePair<string, string>("ck", key));

        var req = CONF_OP + query.ToQueryString();
        var resp = await client.GetStringAsync(req);
        var successCode = HealthMonitor.LogOnException(resp, () => SteamStatusCode.Translate<SteamStatusCode>(resp, out _));

        return successCode.Equals(SteamStatusCode.Ok);

    }


    public static async Task<bool> SendMultipleConfirmations(HttpClient client, IEnumerable<Confirmation> confirmations,
        long steamId, MobileData data, bool confirm)
    {
        var op = confirm ? "allow" : "cancel";
        var query = GetConfirmationKvp(steamId, data.DeviceId, data.IdentitySecret, op).ToList();
        query.Insert(0, new KeyValuePair<string, string>("op", op));

        foreach (var confirmation in confirmations)
        {
            query.Add(new KeyValuePair<string, string>("cid[]", confirmation.Id.ToString()));
            query.Add(new KeyValuePair<string, string>("ck[]", confirmation.Nonce.ToString()));
        }

        var content = new FormUrlEncodedContent(query);
        var resp = await client.PostAsync(MULTI_CONF_OP, content);
        var respStr = await resp.Content.ReadAsStringAsync();
        var successCode = HealthMonitor.LogOnException(respStr, () => SteamStatusCode.Translate<SteamStatusCode>(respStr, out _));
        return successCode.Equals(SteamStatusCode.Ok);

    }

    internal static IEnumerable<KeyValuePair<string, string>> GetConfirmationKvp(long steamId, string deviceId, string identitySecret, string tag = "conf")
    {
        var time = TimeAligner.GetSteamTime();
        var hash = EncryptionHelper.GenerateConfirmationHash(time, identitySecret, tag);
        return new KeyValuePair<string, string>[]
        {
            new("p", deviceId),
            new("a", steamId.ToString()),
            new("k", hash),
            new("t", time.ToString()),
            new("m", "react"),
            new("tag", tag)
        };
    }
}