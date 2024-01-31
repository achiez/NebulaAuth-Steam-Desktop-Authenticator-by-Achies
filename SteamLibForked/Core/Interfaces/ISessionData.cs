using SteamLib.Account;
using SteamLib.Core.Enums;

namespace SteamLib.Core.Interfaces;

//WARNING: Any changes here should be reflected in MafileSerializer.cs
public interface ISessionData : ICloneable
{
    internal bool? IsValid { get; set; }
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