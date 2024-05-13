using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Entities;
using SteamLib.Account;
using SteamLib.Api.Mobile;
using SteamLib.Authentication;
using SteamLib.Authentication.LoginV2;
using SteamLib.Core.Interfaces;
using SteamLib.Exceptions;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Services;
using SteamLib.SteamMobile;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SteamLib.Api;
using SteamLib.Core.Enums;

namespace NebulaAuth.Model;


public static class MaClient
{
    private static HttpClientHandler ClientHandler { get; }

    private static HttpClient Client { get; }

    private static DynamicProxy Proxy { get; }

    public static ProxyData? DefaultProxy { get; set; }

    static MaClient()
    {
        Proxy = new DynamicProxy(null);
        var pair = ClientBuilder.BuildMobileClient(Proxy, null);
        Client = pair.Client;
        ClientHandler = pair.Handler;
    }

    public static void ClearCookies()
    {
        foreach (Cookie allCookie in ClientHandler.CookieContainer.GetAllCookies())
        {
            allCookie.Expired = true;
        }
    }

    public static void SetAccount(Mafile? account)
    {
        ClearCookies();
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
        return SteamMobileConfirmationsApi.GetConfirmations(Client, mafile, mafile.SessionData!.SteamId.Steam64);
    }

    public static async Task LoginAgain(Mafile mafile, string password, bool savePassword, ICaptchaResolver? resolver)
    {
        SetProxy(mafile);
        var sgGenerator = new SteamGuardCodeGenerator(mafile.SharedSecret);
        var options = new LoginV2ExecutorOptions(LoginV2Executor.NullConsumer, Client)
        {
            Logger = Shell.ExtensionsLogger,
            SteamGuardProvider = sgGenerator,
            DeviceDetails = LoginV2ExecutorOptions.GetMobileDefaultDevice(),
            WebsiteId = "Mobile"
        };
        ClientHandler.CookieContainer.ClearMobileSessionCookies();
        var result = await LoginV2Executor.DoLogin(options, mafile.AccountName, password);
        AdmissionHelper.TransferCommunityCookies(ClientHandler.CookieContainer);
        mafile.SessionData = (MobileSessionData)result;
        if(PHandler.IsPasswordSet)
            mafile.Password = (savePassword ? PHandler.Encrypt(password) : null);
        Storage.UpdateMafile(mafile);
    }


    public static async Task RefreshSession(Mafile mafile)
    {
        ValidateMafile(mafile, true);
        SetProxy(mafile);
        var token = mafile.SessionData!.GetMobileToken();
        if (token == null || token.Value.IsExpired)
        {
            var sessionToken = await SteamMobileApi.RefreshJwt(Client, mafile.SessionData!.RefreshToken.Token, mafile.SessionData.SteamId.Steam64);
            var newToken = SteamTokenHelper.Parse(sessionToken);
            mafile.SessionData.SetMobileToken(newToken);
        }

        //RETHINK: Do we need this? Mobile token is enough
        var communityToken = mafile.SessionData!.GetToken(SteamDomain.Community);
        if (communityToken == null || communityToken.Value.IsExpired)
        {
            var communityTokenString = await SteamGlobalApi.RefreshJwt(Client, SteamDomain.Community);
            var newToken = SteamTokenHelper.Parse(communityTokenString);
            mafile.SessionData.SetToken(SteamDomain.Community, newToken);
        }

        Storage.UpdateMafile(mafile);
        ClientHandler.CookieContainer.SetSteamMobileCookiesWithMobileToken(mafile.SessionData);
    }

    public static Task<bool> SendConfirmation(Mafile mafile, Confirmation confirmation, bool confirm)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        return SteamMobileConfirmationsApi.SendConfirmation(Client, confirmation, mafile.SessionData!.SteamId.Steam64, mafile, confirm);
    }

    public static Task<bool> SendMultipleConfirmation(Mafile mafile, IEnumerable<Confirmation> confirmations, bool confirm)
    {
        var enumerable = confirmations.ToList();
        if (!enumerable.Any())
        {
            return Task.FromResult(result: false);
        }

        ValidateMafile(mafile);
        SetProxy(mafile);
        return SteamMobileConfirmationsApi.SendMultipleConfirmations(Client, enumerable, mafile.SessionData!.SteamId.Steam64, mafile, confirm);
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
            throw new SessionExpiredException();

        if (ignoreAccessToken == false)
        {
            var access = mafile.SessionData.GetMobileToken();
            if (access == null || access.Value.IsExpired)
                throw new SessionExpiredException();
        }
        
    }

    public static async Task<LoginConfirmationResult> ConfirmLoginRequest(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        var token = mafile.SessionData!.GetMobileToken()!.Value;
        

        var uri = "https://api.steampowered.com/IAuthenticationService/GetAuthSessionsForAccount/v1?access_token=" + token.Token;
        GetAuthSessionsForAccount_Response getsess;
        try
        {
            getsess = await Client.GetProto<GetAuthSessionsForAccount_Response>(uri, new EmptyMessage());
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SessionExpiredException(string.Empty, ex);
        }

        if (getsess.ClientIds.Count == 0)
        {
            return new LoginConfirmationResult
            {
                Error = LoginConfirmationError.NoRequests
            };
        }
        if (getsess.ClientIds.Count > 1)
        {
            return new LoginConfirmationResult
            {
                Error = LoginConfirmationError.MoreThanOneRequest
            };
        }
        var clientId = getsess.ClientIds.Single();
        var infoUri = "https://api.steampowered.com/IAuthenticationService/GetAuthSessionInfo/v1?access_token=" + token.Token;
        var infoReq = new GetAuthSessionInfo_Request
        {
            ClientId = clientId
        };
        var infoResp = await Client.PostProto<GetAuthSessionInfo_Response>(infoUri, infoReq);
        var updateUri = "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithMobileConfirmation/v1?access_token=" + token.Token;
        var updateReq = new UpdateAuthSessionWithMobileConfirmation_Request
        {
            ClientId = clientId,
            Confirm = true,
            Persistence = 1,
            Steamid = mafile.SessionData.SteamId.Steam64.ToUlong(),
            Version = 1
        };
        updateReq.ComputeSignature(mafile.SharedSecret);
        await Client.PostProtoEnsureSuccess(updateUri, updateReq);
        return new LoginConfirmationResult
        {
            Country = infoResp.Country,
            IP = infoResp.IP,
            Success = true
        };
    }
}