using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using SteamLib.Core.Enums;
using SteamLib.Core.Interfaces;
using SteamLib.Core.Models;
using SteamLib.Exceptions;

namespace SteamLib.Authentication;

public static class SessionDataValidator
{
    [PublicAPI] public static Dictionary<Type, IValidateOptions<ISessionData>> Validators { get; } = new();

    public static ValidateOptionsResult Validate(string? name, ISessionData data)
    {
        if (Validators.TryGetValue(data.GetType(), out var validator))
            return validator.Validate(name, data);

        return Validate(data);
    }

    public static ValidateOptionsResult Validate<T>(string? name, T data) where T : ISessionData
    {
        if (Validators.TryGetValue(typeof(T), out var validator))
            return validator.Validate(name, data);

        return Validate(data);
    }


    private static ValidateOptionsResult Validate(ISessionData data)
    {
        if (data == null!)
        {
            return ValidateOptionsResult.Fail("SessionData cannot be null");
        }

        if (string.IsNullOrWhiteSpace(data.SessionId))
        {
            return ValidateOptionsResult.Fail("SessionId cannot be null or empty");
        }

        if (data.SteamId.Steam64.Id < SteamId64.SEED)
        {
            return ValidateOptionsResult.Fail($"SteamId '{data.SteamId.Steam64}' is invalid.");
        }

        if (string.IsNullOrWhiteSpace(data.RefreshToken.Token))
        {
            return ValidateOptionsResult.Fail("RefreshToken cannot be null or empty");
        }

        if (data.RefreshToken.SteamId.Steam64.Id < SteamId64.SEED)
        {
            return ValidateOptionsResult.Fail($"SteamId in RefreshToken '{data.SteamId.Steam64}' is invalid.");
        }


        if (data is IMobileSessionData && data.RefreshToken.Type != SteamAccessTokenType.MobileRefresh)
        {
            return ValidateOptionsResult.Fail($"RefreshToken '{data.RefreshToken.Token}' is not of type MobileRefresh");
        }

        if (data.RefreshToken.Type is not (SteamAccessTokenType.Refresh or SteamAccessTokenType.MobileRefresh))
        {
            return ValidateOptionsResult.Fail(
                $"RefreshToken '{data.RefreshToken.Token}' is not of type Refresh or MobileRefresh");
        }

        if (data.RefreshToken.Expires.ToLong() == 0L)
        {
            return ValidateOptionsResult.Fail($"RefreshToken '{data.RefreshToken.Token}' has invalid expiration date");
        }


        return ValidateOptionsResult.Success;
    }

    public static bool IsValid(this ISessionData data)
    {
        return Validate(null, data).Succeeded;
    }

    public static void EnsureValidated(this ISessionData data)
    {
        if (data.IsValid == true) return;
        if (data.IsValid == false)
            throw new SessionInvalidException(
                "SessionData was validated before and validation was not passed. Session is invalid");
        var validation = Validate(null, data);
        data.IsValid = validation.Succeeded;
        if (validation.Failed)
        {
            throw new SessionInvalidException("SessionData is invalid. " + validation.FailureMessage);
        }
    }
}