namespace SteamLib.Exceptions.Authorization;

/// <summary>
///     Unlike <see cref="SessionInvalidException" />, this exception indicates a definite session expiration. Refreshing
///     the JWT token will not help.
/// </summary>
public class SessionPermanentlyExpiredException : SessionInvalidException
{
    public const string SESSION_EXPIRED_MSG =
        "Session expired and won't longer work. You must login to get new session";

    public SessionPermanentlyExpiredException()
    {
    }

    public SessionPermanentlyExpiredException(string message) : base(message)
    {
    }

    public SessionPermanentlyExpiredException(string message, Exception inner) : base(message, inner)
    {
    }
}