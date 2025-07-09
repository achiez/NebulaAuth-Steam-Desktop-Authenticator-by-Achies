﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AchiesUtilities.Web.Models;
using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Entities;
using SteamLib.Api.Mobile;
using SteamLib.Api.Services;
using SteamLib.Authentication;
using SteamLib.Exceptions.Authorization;
using SteamLib.ProtoCore.Services;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.Web;

namespace NebulaAuth.Model;

public static class MaClient
{
    private static HttpClientHandler ClientHandler { get; }

    private static HttpClient Client { get; }

    private static DynamicProxy Proxy { get; }

    public static ProxyData? DefaultProxy { get; set; }


    static MaClient()
    {
        Proxy = new DynamicProxy();
        var pair = ClientBuilder.BuildMobileClient(Proxy, null);
        Client = pair.Client;
        ClientHandler = pair.Handler;
    }


    public static void SetAccount(Mafile? account)
    {
        ClientHandler.CookieContainer.ClearAllCookies();
        if (account != null)
        {
            if (account.SessionData != null)
            {
                ClientHandler.CookieContainer.SetSteamMobileCookiesWithMobileToken(account.SessionData);
            }
            else
            {
                ClientHandler.CookieContainer.ClearSteamCookies();
                ClientHandler.CookieContainer.AddMinimalMobileCookies();
                AdmissionHelper.TransferCommunityCookies(ClientHandler.CookieContainer);
            }

            Proxy.SetData(account.Proxy?.Data);
        }
    }

    public static Task<IEnumerable<Confirmation>> GetConfirmations(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        return SteamMobileConfirmationsApi.GetConfirmations(Client, mafile, mafile.SessionData!.SteamId);
    }

    public static Task LoginAgain(Mafile mafile, string password, bool savePassword)
    {
        SetProxy(mafile);
        return SessionHandler.LoginAgain(new HttpClientHandlerPair(Client, ClientHandler), mafile, password,
            savePassword);
    }


    public static Task RefreshSession(Mafile mafile)
    {
        ValidateMafile(mafile, true);
        SetProxy(mafile);
        return SessionHandler.RefreshMobileToken(new HttpClientHandlerPair(Client, ClientHandler), mafile);
    }

    public static Task<bool> SendConfirmation(Mafile mafile, Confirmation confirmation, bool confirm)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        return SteamMobileConfirmationsApi.SendConfirmation(Client, confirmation, mafile.SessionData!.SteamId, mafile,
            confirm);
    }

    public static Task<bool> SendMultipleConfirmation(Mafile mafile, IEnumerable<Confirmation> confirmations,
        bool confirm)
    {
        var enumerable = confirmations.ToList();
        if (enumerable.Count == 0)
        {
            return Task.FromResult(false);
        }

        ValidateMafile(mafile);
        SetProxy(mafile);
        return SteamMobileConfirmationsApi.SendMultipleConfirmations(Client, enumerable, mafile.SessionData!.SteamId,
            mafile, confirm);
    }

    public static Task<RemoveAuthenticator_Response> RemoveAuthenticator(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        if (mafile.RevocationCode == null)
        {
            throw new InvalidOperationException("This mafile does not have R-Code");
        }

        var token = mafile.SessionData!.GetMobileToken()!;
        return SteamMobileApi.RemoveAuthenticator(Client, token.Value.Token, mafile.RevocationCode);
    }

    private static void SetProxy(Mafile mafile)
    {
        Proxy.SetData(mafile.Proxy?.Data ?? DefaultProxy);
    }

    private static void ValidateMafile(Mafile mafile, bool ignoreAccessToken = false)
    {
        if (mafile.SessionData == null) throw new SessionInvalidException();
        if (mafile.SessionData.RefreshToken.IsExpired)
            throw new SessionPermanentlyExpiredException();

        if (ignoreAccessToken == false)
        {
            var access = mafile.SessionData.GetMobileToken();
            if (access == null || access.Value.IsExpired)
                throw new SessionPermanentlyExpiredException();
        }
    }

    public static async Task<LoginConfirmationResult> ConfirmLoginRequest(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        var token = mafile.SessionData!.GetMobileToken()!.Value;
        var sessions = await AuthenticationServiceApi.GetAuthSessionsForAccount(Client, token.Token);

        if (sessions.ClientIds.Count == 0)
        {
            return new LoginConfirmationResult
            {
                Error = LoginConfirmationError.NoRequests
            };
        }

        if (sessions.ClientIds.Count > 1)
        {
            return new LoginConfirmationResult
            {
                Error = LoginConfirmationError.MoreThanOneRequest
            };
        }

        var clientId = sessions.ClientIds.Single();
        var clientInfo = await AuthenticationServiceApi.GetAuthSessionInfo(Client, token.Token, clientId);
        var updateReq =
            AuthRequestHelper.CreateMobileConfirmationRequest(1, clientId, mafile.SessionData.SteamId.Steam64,
                mafile.SharedSecret);
        await AuthenticationServiceApi.UpdateAuthSessionWithMobileConfirmation(Client, token.Token, updateReq);
        return new LoginConfirmationResult
        {
            Country = clientInfo.Country,
            IP = clientInfo.IP,
            Success = true
        };
    }

    public static HttpClientHandlerPair GetHttpClientHandlerPair(Mafile mafile)
    {
        SetProxy(mafile);
        return new HttpClientHandlerPair(Client, ClientHandler);
    }
}