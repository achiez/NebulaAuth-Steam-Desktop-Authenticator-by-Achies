namespace SteamLib.Exceptions;
public class SessionExpiredException : SessionInvalidException
{
    public const string SESSION_EXPIRED_MSG = "Session expired and won't longer work. You must login to get new session";
    public SessionExpiredException() { }
    public SessionExpiredException(string message) : base(message) { }
    public SessionExpiredException(string message, Exception inner) : base(message, inner) { }
    protected SessionExpiredException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
