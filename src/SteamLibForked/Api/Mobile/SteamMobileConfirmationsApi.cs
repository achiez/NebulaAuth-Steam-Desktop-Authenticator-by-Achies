﻿using System.Net;
using AchiesUtilities.Web.Extensions;
using SteamLib.Core;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Authorization;
using SteamLib.SteamMobile;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.Utility;
using SteamLib.Web.Scrappers.JSON;

namespace SteamLib.Api.Mobile;

public static class SteamMobileConfirmationsApi
{
    public static async Task<IEnumerable<Confirmation>> GetConfirmations(HttpClient client, MobileData data,
        SteamId steamId, CancellationToken cancellationToken = default)
    {
        var nvc = GetConfirmationKvp(steamId, data.DeviceId, data.IdentitySecret, "list");

        var req = new Uri(Routes.CONF_LIST + nvc.ToQueryString());
        var reqMsg = new HttpRequestMessage(HttpMethod.Get, req);
        var resp = await client.SendAsync(reqMsg, cancellationToken);


        if (resp.StatusCode == HttpStatusCode.Redirect)
        {
            throw new SessionInvalidException("Mobile session expired");
        }

        var respStr = await resp.Content.ReadAsStringAsync(cancellationToken);
        resp.EnsureSuccessStatusCode();

        try
        {
            return MobileConfirmationScrapper.Scrap(respStr);
        }
        catch (Exception ex)
            when (ex is not (SessionInvalidException or CantLoadConfirmationsException))
        {
            SteamLibErrorMonitor.LogErrorResponse(respStr, ex);
            throw;
        }
    }

    public static async Task<bool> SendConfirmation(HttpClient client, Confirmation confirmation, SteamId steamId,
        MobileData data, bool confirm, CancellationToken cancellationToken = default)
    {
        var op = confirm ? "allow" : "cancel";

        var query = GetConfirmationKvp(steamId, data.DeviceId, data.IdentitySecret, op).ToList();

        var id = confirmation.Id.ToString();
        var key = confirmation.Nonce.ToString();

        query.Insert(0, new KeyValuePair<string, string>("op", op));
        query.Add(new KeyValuePair<string, string>("cid", id));
        query.Add(new KeyValuePair<string, string>("ck", key));

        var req = Routes.CONF_OP + query.ToQueryString();
        var resp = await client.GetStringAsync(req, cancellationToken);
        var successCode =
            SteamLibErrorMonitor.HandleResponse(resp, () => SteamStatusCode.Translate(resp));

        return successCode.Equals(SteamStatusCode.Ok);
    }


    public static async Task<bool> SendMultipleConfirmations(HttpClient client, IEnumerable<Confirmation> confirmations,
        SteamId steamId, MobileData data, bool confirm, CancellationToken cancellationToken = default)
    {
        var op = confirm ? "allow" : "cancel";
        var query = GetConfirmationKvp(steamId, data.DeviceId, data.IdentitySecret, op).ToList();
        query.Insert(0, new KeyValuePair<string, string>("op", op));
        var list = confirmations.ToList();
        foreach (var confirmation in list)
        {
            query.Add(new KeyValuePair<string, string>("cid[]", confirmation.Id.ToString()));
            query.Add(new KeyValuePair<string, string>("ck[]", confirmation.Nonce.ToString()));
        }

        var content = new FormUrlEncodedContent(query);
        var resp = await client.PostAsync(Routes.MULTI_CONF_OP, content, cancellationToken);
        var respStr = await resp.Content.ReadAsStringAsync(cancellationToken);
        var successCode = SteamLibErrorMonitor.HandleResponse(respStr,
            () => SteamStatusCode.Translate(respStr));
        return successCode.Equals(SteamStatusCode.Ok);
    }

    internal static IEnumerable<KeyValuePair<string, string>> GetConfirmationKvp(SteamId steamId, string deviceId,
        string identitySecret, string tag = "conf")
    {
        var time = TimeAligner.GetSteamTime();
        var hash = EncryptionHelper.GenerateConfirmationHash(time, identitySecret, tag);
        return
        [
            KeyValuePair.Create("p", deviceId),
            KeyValuePair.Create("a", steamId.Steam64.ToString()),
            KeyValuePair.Create("k", hash),
            KeyValuePair.Create("t", time.ToString()),
            KeyValuePair.Create("m", "react"),
            KeyValuePair.Create("tag", tag)
        ];
    }

    public static class Routes
    {
        public const string CONF = SteamConstants.STEAM_COMMUNITY + "/mobileconf";
        public const string CONF_LIST = SteamConstants.STEAM_COMMUNITY + "/mobileconf/getlist";
        public const string CONF_OP = SteamConstants.STEAM_COMMUNITY + "/mobileconf/ajaxop";
        public const string MULTI_CONF_OP = SteamConstants.STEAM_COMMUNITY + "/mobileconf/multiajaxop";
    }
}