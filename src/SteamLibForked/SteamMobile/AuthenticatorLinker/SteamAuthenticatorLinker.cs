﻿using Microsoft.Extensions.Logging;
using SteamLib.Abstractions;
using SteamLib.Api.Mobile;
using SteamLib.Authentication;
using SteamLib.Exceptions.Authorization;
using SteamLib.Exceptions.Mobile;
using SteamLib.ProtoCore.Enums;
using SteamLibForked.Abstractions;
using SteamLibForked.Models.Session;

namespace SteamLib.SteamMobile.AuthenticatorLinker;

public class SteamAuthenticatorLinker
{
    public ILoginConsumer Consumer { get; }
    public LinkOptions Options { get; }
    public ISessionData? SessionData { get; private set; }
    internal HttpClient Client => Options.HttpClient;
    internal string? DeviceId { get; set; }
    private ILogger? Logger => Options.Logger;

    public SteamAuthenticatorLinker(LinkOptions options)
    {
        Options = options;
        Consumer = options.LoginConsumer;
    }

    public async Task<MobileDataExtended> LinkAccount(MobileSessionData data)
    {
        Dictionary<string, string> scopeData = new()
        {
            {"consumer", Consumer.FriendlyName}
        };
        using var ctx = Options.Logger?.BeginScope(scopeData);

        SessionData = data;

        data.EnsureValidated();
        if (data.IsExpired)
        {
            Logger?.LogError("Session expired");
            throw new SessionPermanentlyExpiredException(SessionPermanentlyExpiredException.SESSION_EXPIRED_MSG);
        }

        var accessToken = data.GetMobileToken();
        if (accessToken == null)
            throw new SessionInvalidException("SessionData must provide valid AccessToken to access mobile endpoints");

        if (accessToken.Value.IsExpired || accessToken.Value.Type != SteamAccessTokenType.Mobile)
        {
            if (accessToken.Value.Type != SteamAccessTokenType.Mobile)
                Logger?.LogWarning("Provided access token is not of type Mobile. Actual type: {actualType}",
                    accessToken.Value.Type);

            var refreshed = await SteamMobileApi.RefreshJwt(Options.HttpClient, data.RefreshToken.Token,
                data.RefreshToken.SteamId);
            accessToken = SteamTokenHelper.Parse(refreshed);
            data.SetMobileToken(accessToken.Value);
        }

        if (accessToken.Value.Type != SteamAccessTokenType.Mobile)
            throw new SessionInvalidException(
                "SessionData must provide AccessToken of type Mobile to access mobile endpoints");


        var hasPhoneResp = await this.HasPhone();
        var hasPhone = hasPhoneResp.HasPhone;


        if (hasPhone && Options.SmsCodeProvider == null)
        {
            throw new AuthenticatorLinkerException(AuthenticatorLinkerError.PhoneAlreadyAttached);
        }

        long? phoneNumber = null;

        if (hasPhone == false && Options.PhoneNumberProvider != null)
        {
            phoneNumber = await Options.PhoneNumberProvider.GetPhoneNumber(Consumer);
        }

        if (hasPhone == false && phoneNumber != null)
        {
            Logger?.LogInformation("Attaching phone number {phoneNumber}", phoneNumber);
            //if (await this.IsValidPhoneNumber(phoneNumber.Value) == false)
            //    throw new AuthenticatorLinkerException(AuthenticatorLinkerError.InvalidPhoneNumber);

            if (await this.AttachPhone(phoneNumber.Value) == false)
                throw new AuthenticatorLinkerException(AuthenticatorLinkerError.CantAttachPhone);


            var emailValid = false;
            for (var i = 0; i < Options.EmailProvider!.RetryCount; i++)
            {
                await Options.EmailProvider!.ConfirmEmailLink(Consumer, EmailConfirmationType.AttachPhoneAuthenticator);
                emailValid = await this.CheckEmailConfirmation();
                if (emailValid) break;
            }


            if (!emailValid)
                throw new AuthenticatorLinkerException(AuthenticatorLinkerError.CantConfirmAttachingEmail);

            var sendSms = await this.SendSmsCode();
            if (sendSms != EResult.OK)
                throw new AuthenticatorLinkerException($"Can't send SMS code: {sendSms} ({(int) sendSms})");
            Logger?.LogDebug("SMS code sent");
        }

        Logger?.LogInformation("Starting LinkRequest");
        var resp = await this.LinkRequest();

        var mobileData = resp.ToMobileDataExtended(DeviceId!, data.Clone());
        Options.BackupHandler?.Invoke(mobileData);

        if (resp.Status == 29)
        {
            throw new AuthenticatorLinkerException(AuthenticatorLinkerError.AuthenticatorPresent);
        }

        if (resp.Status != 1)
        {
            throw new AuthenticatorLinkerException(resp.Status);
        }


        string code;
        var isPhone = resp.ConfirmType is 1 or 2;
        if (isPhone)
        {
            if (Options.SmsCodeProvider == null)
                throw new AuthenticatorLinkerException(AuthenticatorLinkerError.NoSmsCodeProvider);

            var smsCode = await Options.SmsCodeProvider.GetSmsCode(Consumer, phoneNumber, resp.PhoneNumberHint);
            code = smsCode.ToString("D5");
        }
        else if (resp.ConfirmType == 3)
        {
            if (Options.EmailProvider == null)
                throw new AuthenticatorLinkerException(AuthenticatorLinkerError.NoEmailProvider);

            code = await Options.EmailProvider.GetAddAuthenticatorCode(Consumer);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(resp.ConfirmType), resp.ConfirmType,
                $"ConfirmType {resp.ConfirmType} not supported");
        }

        Logger?.LogInformation("Finalizing link");
        var result = await this.FinalizeLink(code, resp.SharedSecret, isPhone);

        if (result.Success == false)
        {
            var error = result.Error switch
            {
                LinkError.GeneralFailure => throw new AuthenticatorLinkerException(result.Code!.Value)
                    {OnFinalization = true},
                LinkError.BadConfirmationCode => AuthenticatorLinkerError.BadConfirmationCode,
                LinkError.UnableToGenerateCorrectCodes => AuthenticatorLinkerError.UnableToGenerateCorrectCodes,
                _ => throw new ArgumentOutOfRangeException(nameof(result.Error), result.Error,
                    $"LinkError {result.Error} not supported")
            };

            throw new AuthenticatorLinkerException(error) {OnFinalization = true};
        }

        Logger?.LogInformation("Linking completed");
        return mobileData;
    }
}