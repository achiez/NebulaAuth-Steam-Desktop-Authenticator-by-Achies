using AchiesUtilities.Models;
using AchiesUtilities.Newtonsoft.JSON.Converters.Special;
using Newtonsoft.Json;
using SteamLib.Authentication;
using SteamLib.Web.Converters;
using SteamLibForked.Models.Core;

namespace SteamLibForked.Models.Session;

public readonly struct SteamAuthToken
{
    public string Token { get; }

    [JsonConverter(typeof(SteamIdToSteam64Converter))]
    public SteamId SteamId { get; }

    [JsonConverter(typeof(UnixTimeStampConverter))]
    public UnixTimeStamp Expires { get; }

    public SteamDomain Domain { get; init; }
    public SteamAccessTokenType Type { get; }

    [JsonIgnore] public bool IsExpired => Expires.Time < DateTime.UtcNow;

    [JsonIgnore] public string SignedToken { get; }

    public SteamAuthToken(string token, long steamId, UnixTimeStamp expires, SteamDomain domain,
        SteamAccessTokenType type)
    {
        Token = token;
        Expires = expires;
        Domain = domain;
        Type = type;
        SteamId = SteamId.FromSteam64(steamId);
        SignedToken = SteamTokenHelper.CombineJwtWithSteamId(SteamId.Steam64.Id, Token);
    }

    [JsonConstructor]
    public SteamAuthToken(string token, SteamId steamId, UnixTimeStamp expires, SteamDomain domain,
        SteamAccessTokenType type)
    {
        Token = token;
        SteamId = steamId;
        Expires = expires;
        Domain = domain;
        Type = type;
        SignedToken = SteamTokenHelper.CombineJwtWithSteamId(SteamId.Steam64.Id, Token);
    }
}