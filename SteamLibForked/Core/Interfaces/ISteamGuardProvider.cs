namespace SteamLib.Core.Interfaces;

public interface ISteamGuardProvider
{
    public int MaxRetryCount { get; }
    public ValueTask<string> GetSteamGuardCode(ILoginConsumer caller);
}