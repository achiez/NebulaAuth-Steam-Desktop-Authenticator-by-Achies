using SteamLibForked.Models.Core;
using SteamLibForked.Models.Session;

namespace SteamLibForked.Abstractions;

//WARNING: Any changes here should be reflected in MafileSerializer.cs
public interface ISessionData : ICloneable
{
    [Obsolete("Will be removed in V2")] internal bool? IsValid { get; set; }

    public bool IsExpired => RefreshToken.IsExpired;
    public string SessionId { get; }
    public SteamId SteamId { get; }
    public SteamAuthToken RefreshToken { get; }

    public SteamAuthToken? GetToken(SteamDomain domain);
    public void SetToken(SteamDomain domain, SteamAuthToken token);
}

public interface IMobileSessionData : ISessionData
{
    public SteamAuthToken? GetMobileToken();
    public void SetMobileToken(SteamAuthToken token);
}