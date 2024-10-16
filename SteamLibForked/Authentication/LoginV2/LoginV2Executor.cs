using AchiesUtilities.Web.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamLib.Account;
using SteamLib.Core;
using SteamLib.Core.Enums;
using SteamLib.Core.Interfaces;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions;
using SteamLib.Login.Default;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Exceptions;
using SteamLib.ProtoCore.Services;
using SteamLib.Utility;
using SteamLib.Web;

namespace SteamLib.Authentication.LoginV2;

public class LoginV2Executor
{
    public static ILoginConsumer NullConsumer { get; } = new NullLoginConsumer();
    public ILoginConsumer Caller { get; }
    public HttpClient HttpClient { get; }
    public ILogger? Logger { get; init; }
    public IEmailProvider? EmailAuthProvider { get; init; }
    public ICaptchaResolver? CaptchaResolver { get; init; }
    public ISteamGuardProvider? SteamGuardProvider { get; init; }
    public string WebsiteId { get; init; }
    public DeviceDetails DeviceDetails { get; init; }
    private LoginV2Executor(LoginV2ExecutorOptions options)
    {
        Caller = options.Consumer;
        HttpClient = options.HttpClient;
        Logger = options.Logger;
        EmailAuthProvider = options.EmailAuthProvider;
        SteamGuardProvider = options.SteamGuardProvider;
        WebsiteId = options.GetWebsiteIdOrDefault();
        DeviceDetails = options.DeviceDetails ?? DeviceDetails.CreateDefault();
    }


    /// <summary>
    /// Do log in on <see href="https://steamcommunity.com/">SteamCommunity</see>.<br/>
    /// Some functions require proper SessionId. But <see cref="SessionData"/> contains only SteamCommunity related SessionId and some functions on other services may not work
    /// </summary>
    /// <param name="options"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="SessionData"/> or <see cref="MobileSessionData"/> depending on which token type is returned</returns>
    /// <exception cref="LoginException"></exception>
    /// <exception cref="EResultException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<ISessionData> DoLogin(LoginV2ExecutorOptions options, string username, string password, CancellationToken cancellationToken = default) //TODO: logs
    {
        var executor = new LoginV2Executor(options);
        var client = executor.HttpClient;

        var globalData = await SteamWebApi.GetMarketGlobalInfo(client, cancellationToken);
        var sessionId = globalData.SessionId;

        var rsgMsg = new GetPasswordRSAPublicKey_Request
        {
            AccountName = username
        };
        var rsaResp = await client.GetProto<GetPasswordRSAPublicKey_Response>(
                "https://api.steampowered.com/IAuthenticationService/GetPasswordRSAPublicKey/v1", rsgMsg, cancellationToken: cancellationToken);


        var encodedPassword = EncryptionHelper.ToBase64EncryptedPassword(rsaResp.PublickKeyExp, rsaResp.PublickKeyMod, password);


        var beginAuthMsg = new BeginAuthSessionViaCredentials_Request
        {
            DeviceFriendlyName = string.Empty,
            AccountName = username,
            EncryptedPassword = encodedPassword,
            EncryptionTimestamp = rsaResp.Timestamp,
            RememberLogin = true,
            PlatformType = executor.DeviceDetails.PlatformType,
            Persistence = 1,
            WebsiteId = executor.WebsiteId,
            DeviceDetails = executor.DeviceDetails,
        };

        BeginAuthSessionViaCredentials_Response beginAuthResp;  
        try
        {
            beginAuthResp = await client.PostProto<BeginAuthSessionViaCredentials_Response>(
                "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1", beginAuthMsg, cancellationToken: cancellationToken);

        }
        catch (EResultException ex)
            when (ex.Result == EResult.InvalidPassword)
        {
            throw new LoginException(LoginError.InvalidCredentials);
        }


        var clientId = beginAuthResp.ClientId;
        var steamId = (long)beginAuthResp.Steamid;

        var conf = beginAuthResp.AllowedConfirmations.FirstOrDefault(c =>
            c.ConfirmationType is EAuthSessionGuardType.EmailCode or EAuthSessionGuardType.DeviceCode);

        conf ??= beginAuthResp.AllowedConfirmations.FirstOrDefault();

        switch (conf?.ConfirmationType)
        {
            case EAuthSessionGuardType.None:
                break;
            case EAuthSessionGuardType.DeviceCode:
            case EAuthSessionGuardType.EmailCode:
                await UpdateWithCode(executor, clientId, (ulong)steamId, conf.ConfirmationType);
                break;
            case EAuthSessionGuardType.Unknown:
            case EAuthSessionGuardType.DeviceConfirmation:
            case EAuthSessionGuardType.EmailConfirmation:
            case EAuthSessionGuardType.MachineToken:
            case EAuthSessionGuardType.LegacyMachineAuth:
                throw new NotSupportedException(
                    $"Auth confirmation type of {conf.ConfirmationType} is not implemented yet");
            default:
                throw new ArgumentOutOfRangeException(nameof(conf.ConfirmationType), conf?.ConfirmationType, "Unknown confirmation type or null");
        }

        var pollSessionMsg = new PollAuthSessionStatus_Request
        {
            ClientId = clientId,
            RequestId = beginAuthResp.RequestId,

        };

        var pollResp =
            await client.PostProto<PollAuthSessionStatus_Response>(
                "https://api.steampowered.com/IAuthenticationService/PollAuthSessionStatus/v1", pollSessionMsg, cancellationToken: cancellationToken);

        SteamAuthToken refreshToken;
        try
        {
            refreshToken = SteamTokenHelper.Parse(pollResp.RefreshToken);
            if (refreshToken.Type is not (SteamAccessTokenType.Refresh or SteamAccessTokenType.MobileRefresh))
                throw new ArgumentException("Refresh token must be of type Refresh or MobileRefresh. No 'renew' audience found in JWT.",
                    nameof(pollResp.RefreshToken)); //Argument exception for less code
        }
        catch (ArgumentException ex)
        {
            throw new LoginException(null,
                "Steam returned invalid refresh token or it's schema was unsupported. See inner exception for more details",
                LoginError.UndefinedError, ex);
        }

        var data = new Dictionary<string, string>
        {
            {"nonce", pollResp.RefreshToken},
            {"sessionid", sessionId}
        };

        var finalize = await client.PostAsync("https://login.steampowered.com/jwt/finalizelogin", new FormUrlEncodedContent(data), cancellationToken);
        var finalizeContent = await finalize.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken);

        var finalizeResp =
            SteamLibErrorMonitor.HandleResponse(finalizeContent, () => JsonConvert.DeserializeObject<FinalizeLoginJson>(finalizeContent)!);


        List<SteamAuthToken> tokens = new();
        foreach (var transferInfo in finalizeResp.TransferInfo)
        {
            var transferData = new Dictionary<string, string>
            {
                {"nonce", transferInfo.TransferInfoParams.Nonce},
                {"auth", transferInfo.TransferInfoParams.Auth},
                {"steamID", steamId.ToString()}
            };


            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, transferInfo.Url);
                req.Content = new FormUrlEncodedContent(transferData);
                req.Headers.Referrer = SteamDomains.GetDomainUri(SteamDomain.Store);
                var transferResp = await client.SendAsync(req, cancellationToken);
                var transferContent = await transferResp.ReadAsStringEnsureSuccessAsync(cancellationToken);
                var status = JObject.Parse(transferContent); 
                var result = status["result"]?.Value<int>();
                if(result != null)
                    SteamStatusCode.ValidateSuccessOrThrow(result.Value); //TODO: Fix steam.tv token transfer (result always 8). But who really cares.. 

                var tokenStr = SteamTokenHelper.ExtractJwtFromSetCookiesHeader(transferResp.Headers);
                var token = SteamTokenHelper.Parse(tokenStr);
                tokens.Add(token);
            }
            catch (Exception ex)
            {
                executor.Logger?.Log(LogLevel.Warning, ex, "Can't transfer tokens for URI: {uri}", transferInfo.Url);
            }
        }

        var accessToken = SteamTokenHelper.Parse(pollResp.AccessToken);

        if (accessToken.Type == SteamAccessTokenType.Mobile)
        {
            return new MobileSessionData(sessionId, SteamId.FromSteam64(steamId), refreshToken, accessToken, tokens);
        }
        else
        {
            return new SessionData(sessionId, SteamId.FromSteam64(steamId), refreshToken, tokens);
        }

       

    }

    private static async Task UpdateWithCode(LoginV2Executor executor, ulong clientId, ulong steamId, EAuthSessionGuardType guardType)
    {
        string? code;
        if (guardType == EAuthSessionGuardType.DeviceCode)
        {
            if(executor.SteamGuardProvider != null)
                code = await executor.SteamGuardProvider.GetSteamGuardCode(executor.Caller);
            else
            {
                throw new LoginException(LoginError.SteamGuardRequired);
            }
        }
        else
        {
            var t = executor.EmailAuthProvider?.GetEmailAuthCode(executor.Caller) ??
                    throw new LoginException(LoginError.EmailAuthRequired);
            code = await t;
        }
        var updateCodeMsg = new UpdateAuthSessionWithSteamGuardCode_Request
        {
            ClientId = clientId,
            Code = code,
            CodeType = guardType,
            Steamid = steamId
        };

        await executor.HttpClient.PostProtoEnsureSuccess(new Uri("https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1"), updateCodeMsg);
    }

}