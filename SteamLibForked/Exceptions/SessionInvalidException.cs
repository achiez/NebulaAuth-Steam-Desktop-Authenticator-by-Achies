namespace SteamLib.Exceptions;

public class SessionInvalidException : Exception
{
    public const string SESSION_NULL_MSG = "Session was null. Before acting SteamClient must be logged in";
    public SessionInvalidException() { }
	public SessionInvalidException(string message) : base(message) { }
	public SessionInvalidException(string message, Exception? inner) : base(message, inner) { }
	protected SessionInvalidException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
