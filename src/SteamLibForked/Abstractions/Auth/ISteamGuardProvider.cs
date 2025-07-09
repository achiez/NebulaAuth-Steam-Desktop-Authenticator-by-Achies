using SteamLib.Abstractions;

namespace SteamLibForked.Abstractions.Auth;

public interface ISteamGuardProvider : IAuthProvider
{
    public ValueTask<string> GetSteamGuardCode();
}