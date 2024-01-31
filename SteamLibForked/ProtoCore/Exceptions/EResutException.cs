using SteamLib.ProtoCore.Enums;

namespace SteamLib.ProtoCore.Exceptions;

[Serializable]
public class EResultException : Exception
{
    public EResult Result { get; }
    public EResultException() { }
    public EResultException(EResult result) : base("EResult error: " + result)
    {
        Result = result;
    }

    public EResultException(string message, EResult result, Exception inner) : base(message, inner)
    {
        Result = result;
    }

    public EResultException(EResult result, Exception inner) : base("EResult error: " + result, inner)
    {
        Result = result;
    }

	protected EResultException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
