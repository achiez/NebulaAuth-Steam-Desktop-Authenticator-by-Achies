using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using SteamLib.Core;
using SteamLibForked.Abstractions;
using SteamLibForked.Models.Core;

namespace SteamLibForked.Models.Session;

//WARNING: Any changes here should be reflected in MafileSerializer.cs

public class MobileSessionData : SessionData, IMobileSessionData
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

    public override SteamAuthToken? GetToken(SteamDomain domain)
    {
        var isWeb = SteamDomains.WebDomains.Contains(domain);
        if (isWeb)
        {
            // Mobile-issued tokens usually contain the "web" audience,
            // so we assume they can also be used for all web:* domains.
            // See: SteamAuthToken 'TODO' for more details
            return MobileToken ?? base.GetToken(domain);
        }

        return base.GetToken(domain);
    }


    [MemberNotNull(nameof(MobileToken))]
    public virtual void SetMobileToken(SteamAuthToken token)
    {
        if (token.Type != SteamAccessTokenType.Mobile)
            throw new ArgumentException("Token must be of type MobileAccess", nameof(token))
            {
                Data = {{"ActualType", token.Type}}
            };

        MobileToken = token;
    }

    object ICloneable.Clone()
    {
        return new MobileSessionData(SessionId, SteamId, RefreshToken, MobileToken, Tokens);
    }

    public override MobileSessionData Clone()
    {
        return (MobileSessionData) ((ISessionData) this).Clone();
    }
}