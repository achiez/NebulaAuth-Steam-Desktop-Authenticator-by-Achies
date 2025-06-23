using SteamLib.ProtoCore.Enums;

namespace SteamLibForked.Exceptions.Authorization;

public class LoginException : Exception
{
    public LoginError Error { get; }
    public string? Response { get; }

    public LoginException(LoginError error, Exception? inner = null)
        : base($"Login was unsuccessful. Error {error}", inner)
    {
        Error = error;
    }

    public LoginException(string response)
    {
        Error = LoginError.UndefinedError;
        Response = response;
    }

    public LoginException(string? response, string? message, LoginError? error, Exception? innerException) : base(
        message, innerException)
    {
        Response = response;
        Error = error ?? LoginError.UndefinedError;
    }
}

public enum LoginError
{
    /// <summary>
    ///     Some error occurred while trying to log in.
    /// </summary>
    UndefinedError = 0,
    [Obsolete] CaptchaRequired = 1,
    InvalidCredentials = 2,
    InvalidEmailAuthCode = 3,
    InvalidTwoFactorCode = 4,

    /// <summary>
    ///     The shared secret is invalid when trying to confirm for <see cref="EAuthSessionGuardType.DeviceConfirmation" />
    /// </summary>
    InvalidSharedSecret = 7,

    /// <summary>
    ///     Auth provider didn't update session properly and polling failed.
    /// </summary>
    SessionPollingFailed = 8
}