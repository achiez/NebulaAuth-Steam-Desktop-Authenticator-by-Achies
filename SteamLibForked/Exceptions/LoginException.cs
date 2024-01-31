using SteamLib.Core.Interfaces;

namespace SteamLib.Exceptions;

public class LoginException : Exception
{
    public LoginError Error { get; }
    public string? Response { get; }
    public LoginException(LoginError error)
        : base($"Login was unsuccessful. Error {error}")
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

    CaptchaRequired,
    InvalidCredentials,
    InvalidEmailAuthCode,
    InvalidTwoFactorCode,
    /// <summary>
    /// SteamEmail authentication is required to login but no <see cref="IEmailProvider"/> was provided
    /// </summary>
    EmailAuthRequired,
    /// <summary>
    /// SteamGuard is required to login but no <see cref="ISteamGuardProvider"/> was provided
    /// </summary>
    SteamGuardRequired,
    /// <summary>
    /// Some error occurred while trying to login.
    /// </summary>
    UndefinedError

}