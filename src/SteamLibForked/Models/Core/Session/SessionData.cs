using Newtonsoft.Json;
using SteamLib.Web.Converters;
using SteamLibForked.Abstractions;
using SteamLibForked.Models.Core;
using System.Collections.Concurrent;

namespace SteamLibForked.Models.Session;

public class SessionData : ISessionData
{
    [JsonIgnore]
    [Obsolete("Will be removed in V2")]
    public bool? IsValid { get; set; }

    [JsonIgnore] public bool IsExpired => RefreshToken.IsExpired;

    public string SessionId { get; }

    [JsonConverter(typeof(SteamIdToSteam64Converter))]
    public SteamId SteamId { get; }

    public SteamAuthToken RefreshToken { get; }
    public ConcurrentDictionary<SteamDomain, SteamAuthToken> Tokens { get; }

    [JsonConstructor]
    public SessionData(string sessionId, SteamId steamId, SteamAuthToken refreshToken,
        IDictionary<SteamDomain, SteamAuthToken>? tokens)
    {
        SessionId = sessionId;
        SteamId = steamId;
        RefreshToken = refreshToken;
        Tokens = new ConcurrentDictionary<SteamDomain, SteamAuthToken>(tokens ??
                                                                       new Dictionary<SteamDomain, SteamAuthToken>());
    }

    public SessionData(string sessionId, SteamId steamId, SteamAuthToken refreshToken,
        IEnumerable<SteamAuthToken>? tokensCollection)
    {
        SessionId = sessionId;
        SteamId = steamId;
        RefreshToken = refreshToken;
        tokensCollection ??= [];
        Tokens = new ConcurrentDictionary<SteamDomain, SteamAuthToken>(
            tokensCollection.ToDictionary(t => t.Domain, t => t));
    }

    public virtual SteamAuthToken? GetToken(SteamDomain domain)
    {
        if (domain == SteamDomain.Undefined)
            throw new ArgumentException("SteamDomain.Undefined is not allowed");
        if (Tokens.TryGetValue(domain, out var token))
            return token;
        return null;
    }

    public void SetToken(SteamDomain domain, SteamAuthToken token)
    {
        Tokens[domain] = token;
    }


    public virtual SessionData Clone()
    {
        return (SessionData)((ICloneable)this).Clone();
    }

    object ICloneable.Clone()
    {
        return new SessionData(SessionId, SteamId, RefreshToken, Tokens);
    }
}