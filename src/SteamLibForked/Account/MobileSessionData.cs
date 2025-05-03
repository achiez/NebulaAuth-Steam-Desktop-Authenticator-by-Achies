using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using SteamLib.Core.Enums;
using SteamLib.Core.Interfaces;
using SteamLib.Core.Models;

namespace SteamLib.Account;

//WARNING: Any changes here should be reflected in MafileSerializer.cs

public sealed class MobileSessionData : SessionData, IMobileSessionData
{
    public SteamAuthToken? MobileToken { get; private set; }

    [JsonConstructor]
    public MobileSessionData(string sessionId, SteamId steamId, SteamAuthToken refreshToken,
        SteamAuthToken? mobileToken, IDictionary<SteamDomain, SteamAuthToken>? tokens)
        : base(sessionId, steamId, refreshToken, tokens)
    {
        MobileToken = mobileToken;
    }

    public MobileSessionData(string sessionId, SteamId steamId, SteamAuthToken refreshToken,
        SteamAuthToken? mobileToken, IEnumerable<SteamAuthToken>? tokensCollection)
        : base(sessionId, steamId, refreshToken, tokensCollection)
    {
        MobileToken = mobileToken;
    }

    public SteamAuthToken? GetMobileToken()
    {
        return MobileToken;
    }


    [MemberNotNull(nameof(MobileToken))]
    public void SetMobileToken(SteamAuthToken token)
    {
        if (token.Type != SteamAccessTokenType.Mobile)
            throw new ArgumentException("Token must be of type MobileAccess", nameof(token))
            {
                Data = {{"ActualType", token.Type}}
            };

        MobileToken = token;
    }

    public override MobileSessionData Clone()
    {
        return (MobileSessionData) ((ISessionData) this).Clone();
    }

    object ICloneable.Clone()
    {
        return new MobileSessionData(SessionId, SteamId, RefreshToken, MobileToken, Tokens);
    }
}