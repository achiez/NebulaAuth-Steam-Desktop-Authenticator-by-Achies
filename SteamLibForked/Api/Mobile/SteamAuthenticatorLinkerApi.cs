using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SteamLib.Authentication;
using SteamLib.Core;
using SteamLib.Core.Enums;
using SteamLib.Core.Interfaces;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Mobile;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Exceptions;
using SteamLib.ProtoCore.Services;
using SteamLib.SteamMobile;
using SteamLib.SteamMobile.AuthenticatorLinker;

namespace SteamLib.Api.Mobile;

[PublicAPI]
public static class SteamAuthenticatorLinkerApi
{
    private const string PHONE_REQ = SteamConstants.STEAM_COMMUNITY + "steamguard/phoneajax";

    #region Global

    public static Task<AccountPhoneStatus_Response> HasPhone(HttpClient client, string accessToken)
    {
        const string uri = SteamConstants.STEAM_API + "IPhoneService/AccountPhoneStatus/v1";

        var reqUri = AddAccessToken(uri, accessToken);
        return client.PostProto<AccountPhoneStatus_Response>(reqUri, request: null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="data"></param>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticatorLinkerException"></exception>
    /// <exception cref="EResultException"></exception>
    public static async Task<AddAuthenticator_Response> LinkRequest(HttpClient client, ISessionData data, string deviceId)
    {
        const string uri = SteamConstants.STEAM_API + "ITwoFactorService/AddAuthenticator/v1";
        data.EnsureValidated();
        var reqUri = AddAccessToken(uri, data.GetAccessToken());
        var req = new AddAuthenticator_Request
        {
            SteamId = (ulong)data.SteamId.Steam64.Id,
            AuthenticatorType = 1,
            DeviceIdentifier = deviceId,
            Version = 2
        };

        var resp = await client.PostProtoMsg<AddAuthenticator_Response>(reqUri, req);
        if (resp is { Result: EResult.InvalidState, ResponseMsg.Status: 2 })
        {
            throw new AuthenticatorLinkerException(AuthenticatorLinkerError.InvalidStateWithStatus2);
        }

        return resp.GetResponseEnsureSuccess();
    }

    public static async Task<LinkResult> FinalizeLink(HttpClient client, string confirmationCode, ISessionData data,
        byte[] sharedSecret, bool validateSmsCode)
    {
        const string uri = SteamConstants.STEAM_API + "ITwoFactorService/FinalizeAddAuthenticator/v1";
        data.EnsureValidated();
        var reqUri = AddAccessToken(uri, data.GetAccessToken());
        var time = await TimeAligner.GetSteamTimeAsync();
        if (validateSmsCode)
        {
            var validateSmsReq = new FinalizeAddAuthenticator_Request
            {
                SteamId = data.SteamId.Steam64.ToUlong(),
                AuthenticatorTime = (ulong)time,
                ConfirmationCode = confirmationCode,
                ValidateConfirmationCode = true
            };

            var validateResp = await client.PostProto<FinalizeAddAuthenticator_Response>(reqUri, validateSmsReq);
            if (validateResp.Success == false || validateResp.Status != 2)
            {
                if (validateResp.Status == 89)
                    throw new AuthenticatorLinkerException(AuthenticatorLinkerError.BadConfirmationCode);
                throw new AuthenticatorLinkerException("Can't accept sms code. Status: " + validateResp.Status);
            }
        }

        var tries = 0;


        while (tries < 30)
        {
            tries++;
            time = await TimeAligner.GetSteamTimeAsync();
            var code = SteamGuardCodeGenerator.GenerateCode(sharedSecret, time);

            var req = new FinalizeAddAuthenticator_Request
            {
                SteamId = data.SteamId.Steam64.ToUlong(),
                AuthenticatorCode = code,
                AuthenticatorTime = (ulong)time,
                ConfirmationCode = confirmationCode,
            };


            var resp = await client.PostProto<FinalizeAddAuthenticator_Response>(reqUri, req);

            if (resp.Success && resp.WantMore == false)
            {
                return new LinkResult();
            }

            if (resp.Status == 89)
            {
                return new LinkResult(LinkError.BadConfirmationCode);
            }


            if (resp.Status == 88 && tries >= 30)
            {
                return new LinkResult(LinkError.UnableToGenerateCorrectCodes);
            }
        }
        return new LinkResult(LinkError.GeneralFailure);
    }

    public static async Task<bool> IsValidPhoneNumber(HttpClient client, long phoneNumber, string sessionId)
    {
        return await CheckPhoneNumber(client, phoneNumber, sessionId) == CheckPhoneResult.Valid;
    }
    public static async Task<CheckPhoneResult> CheckPhoneNumber(HttpClient client, long phoneNumber, string sessionId)
    {
        var phone = '+' + phoneNumber.ToString();
        var data = new Dictionary<string, string>
        {
            {"phoneNumber", phone},
            {"sessionID", sessionId}
        };

        var reqContent = new FormUrlEncodedContent(data);
        var req = new HttpRequestMessage(HttpMethod.Post, SteamConstants.STEAM_STORE + "phone/validate");
        req.Content = reqContent;
        req.Headers.Referrer = new Uri("https://store.steampowered.com/phone/add");
        var resp = await client.SendAsync(req);
        var content = await resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        var j = JObject.Parse(content);

        if (j["success"]!.Value<bool>() == false)
            return CheckPhoneResult.GeneralFailure;

        if (j["is_voip"]!.Value<bool>())
            return CheckPhoneResult.VoIp;

        return j["is_valid"]!.Value<bool>() ? CheckPhoneResult.Valid : CheckPhoneResult.NotValid;
    }

    public static async Task<bool> AttachPhone(HttpClient client, long phoneNumber, string accessToken)
    {
        const string uri = SteamConstants.STEAM_API + "IPhoneService/SetAccountPhoneNumber/v1";
        var phone = '+' + phoneNumber.ToString();
        var req = new SetAccountPhoneNumber_Request
        {
            PhoneNumber = phone
        };

        var reqUri = AddAccessToken(uri, accessToken);
        var resp =
            await client.PostProtoMsg<SetAccountPhoneNumber_Response>(reqUri, req);

        return resp.Result == EResult.Pending;
    }
    public static async Task<bool> CheckEmailConfirmation(HttpClient client, string accessToken)
    {
        const string uri = SteamConstants.STEAM_API + "IPhoneService/IsAccountWaitingForEmailConfirmation/v1";

        var reqUri = AddAccessToken(uri, accessToken);

        var i = 0;
        while (i < 5)
        {
            i++;
            var resp = await client.PostProto<IsAccountWaitingForEmailConfirmation_Response>(reqUri, new EmptyMessage());

            if (resp.IsWaiting == false) return true;

            await Task.Delay(resp.SecondsToWait * 1000);
        }

        return false;
    }
    public static Task<EResult> SendSmsCode(HttpClient client, string accessToken)
    {
        const string uri = SteamConstants.STEAM_API + "IPhoneService/SendPhoneVerificationCode/v1";

        var reqUri = AddAccessToken(uri, accessToken);
        return client.PostProto(reqUri, new SendPhoneVerificationCode_Request());
    }
    public static async Task<bool> CheckSmsCode(HttpClient client, int smsCode, string sessionId)
    {
        var data = new Dictionary<string, string>
        {
            {"op", "check_sms_code"},
            {"arg", smsCode.ToString()},
            {"checkfortos", "0"},
            {"skipvoip", "1"},
            {"sessionid", sessionId}
        };

        var content = new FormUrlEncodedContent(data);
        var resp = await client.PostAsync(PHONE_REQ, content);
        var respContent = await resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        var success = SteamStatusCode.Translate<SteamStatusCode>(respContent, out _).Equals(SteamStatusCode.Ok);

        if (success) return true;

        await Task.Delay(5000);
        var hasPhone = await HasPhone(client, sessionId);
        return hasPhone.HasPhone;
    }
    private static FormUrlEncodedContent CreateAjaxContent(string op, string arg, string sessionId)
    {
        var data = new Dictionary<string, string>
        {
            {"op",op},
            {"arg", arg},
            {"sessionid", sessionId}
        };
        return new FormUrlEncodedContent(data);
    }

    public static string GenerateDeviceId() => "android:" + Guid.NewGuid();
    private static string AddAccessToken(string uri, string accessToken) => uri + "?access_token=" + accessToken;

    #endregion

    #region ForLinker

    internal static Task<AddAuthenticator_Response> LinkRequest(this SteamAuthenticatorLinker linker)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return LinkRequest(linker.Client, linker.SessionData, linker.DeviceId ??= GenerateDeviceId());
    }

    internal static Task<bool> IsValidPhoneNumber(this SteamAuthenticatorLinker linker, long phoneNumber)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return IsValidPhoneNumber(linker.Client, phoneNumber, linker.SessionData.SessionId);
    }

    internal static Task<AccountPhoneStatus_Response> HasPhone(this SteamAuthenticatorLinker linker)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return HasPhone(linker.Client, linker.SessionData.GetAccessToken());
    }
    internal static Task<bool> AttachPhone(this SteamAuthenticatorLinker linker, long phoneNumber)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return AttachPhone(linker.Client, phoneNumber, linker.SessionData.GetAccessToken());
    }
    internal static Task<bool> CheckEmailConfirmation(this SteamAuthenticatorLinker linker)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return CheckEmailConfirmation(linker.Client, linker.SessionData.GetAccessToken());
    }

    internal static Task<EResult> SendSmsCode(this SteamAuthenticatorLinker linker)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return SendSmsCode(linker.Client, linker.SessionData.GetAccessToken());
    }
    internal static Task<bool> CheckSmsCode(this SteamAuthenticatorLinker linker, int smsCode)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return CheckSmsCode(linker.Client, smsCode, linker.SessionData.SessionId);
    }

    internal static Task<LinkResult> FinalizeLink(this SteamAuthenticatorLinker linker, string confirmationCode, byte[] sharedSecret, bool validateSmsCode)
    {
        if (linker.SessionData == null)
            throw new InvalidOperationException("SessionData is null");

        return FinalizeLink(linker.Client, confirmationCode, linker.SessionData, sharedSecret, validateSmsCode);
    }


    #endregion
    private static string GetAccessToken(this ISessionData s) => s.GetToken(SteamDomain.Community)?.Token ?? throw new SessionInvalidException("Access token was null. MobileEndpoints requires valid AccessToken");
}