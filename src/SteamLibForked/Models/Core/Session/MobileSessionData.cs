using Newtonsoft.Json;
using SteamLibForked.Abstractions;
using SteamLibForked.Models.Core;
using System.Diagnostics.CodeAnalysis;

namespace SteamLibForked.Models.Session;

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
                Data = { { "ActualType", token.Type } }
            };

        MobileToken = token;
    }

    public override MobileSessionData Clone()
    {
        return (MobileSessionData)((ISessionData)this).Clone();
    }

    object ICloneable.Clone()
    {
        return new MobileSessionData(SessionId, SteamId, RefreshToken, MobileToken, Tokens);
    }
}