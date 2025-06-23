using SteamLib.Authentication.LoginV2;
using SteamLib.ProtoCore.Enums;

namespace SteamLib.Abstractions;

public interface IAuthProvider
{
    bool IsSupportedGuardType(ILoginConsumer consumer, EAuthSessionGuardType type);

    /// <summary>
    /// </summary>
    /// <param name="authClient">Client which initiated auth request (typically <see cref="LoginV2Executor" />'s client)</param>
    /// <param name="loginConsumer"></param>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAuthSession(HttpClient authClient, ILoginConsumer loginConsumer, UpdateAuthSessionModel model,
        CancellationToken cancellationToken = default);
}

//TODO: Move
public class UpdateAuthSessionModel
{
    public EAuthSessionGuardType Type { get; }
    public ulong ClientId { get; }
    public ulong SteamId { get; }

    public UpdateAuthSessionModel(EAuthSessionGuardType type, ulong clientId, ulong steamId)
    {
        Type = type;
        ClientId = clientId;
        SteamId = steamId;
    }
}