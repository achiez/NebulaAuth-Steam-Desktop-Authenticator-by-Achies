namespace SteamLib.Exceptions;

[Serializable]
public class CantLoadConfirmationsException : Exception
{
    public LoadConfirmationsError Error { get; init; }
	public CantLoadConfirmationsException() { }
	public CantLoadConfirmationsException(string message) : base(message) { }
	public CantLoadConfirmationsException(string message, Exception inner) : base(message, inner) { }
}


public enum LoadConfirmationsError
{
	Unknown,
	TryAgainLater,
	NotSetupToReceiveConfirmations
}