using System.Numerics;
using Newtonsoft.Json.Linq;
using SteamLibForked.Models.SteamIds;

namespace SteamLib.Utility.MafileSerialization;

public partial class MafileSerializer //Utility
{
    private static JToken? GetToken(JObject j, params string[] aliases)
    {
        foreach (var name in aliases)
        {
            if (j.TryGetValue(name, StringComparison.InvariantCultureIgnoreCase, out var token))
            {
                return token;
            }
        }

        return null;
    }

    private static JToken? GetToken(JObject j, Dictionary<string, JProperty> removeFrom, params string[] aliases)
    {
        foreach (var name in aliases)
        {
            if (!j.TryGetValue(name, StringComparison.InvariantCultureIgnoreCase, out var token)) continue;
            var parent = token.Parent as JProperty;
            removeFrom.Remove(parent!.Name);
            return token;
        }

        return null;
    }

    private static JToken GetTokenOrThrow(JObject j, Dictionary<string, JProperty> removeFrom, string propertyName,
        params string[] aliases)
    {
        foreach (var name in aliases)
        {
            if (!j.TryGetValue(name, StringComparison.InvariantCultureIgnoreCase, out var token)) continue;
            if (token.Type == JTokenType.Null)
            {
                throw new ArgumentException($"Required property {propertyName} is null");
            }

            var parent = token.Parent as JProperty;
            removeFrom.Remove(parent!.Name);
            return token;
        }

        throw new ArgumentException($"Required property {propertyName} not found");
    }


    private static string? GetString(JObject j, params string[] aliases)
    {
        var token = GetToken(j, aliases);
        if (token == null || token.Type == JTokenType.Null)
            return null;


        return token.Value<string>();
    }

    private static string? GetString(JObject j, Dictionary<string, JProperty> removeFrom,
        params string[] aliases)
    {
        var token = GetToken(j, removeFrom, aliases);
        if (token == null || token.Type == JTokenType.Null)
            return null;


        return token.Value<string>();
    }

    private static long? GetLong(JObject j, Dictionary<string, JProperty> removeFrom,
        params string[] aliases)
    {
        var token = GetToken(j, removeFrom, aliases);
        if (token == null || token.Type == JTokenType.Null)
            return null;

        return token.Type switch
        {
            JTokenType.Integer => token.Value<long>(),
            JTokenType.String when long.TryParse(token.Value<string>()!, out var r) => r,
            _ => null
        };
    }

    private static ulong? GetULong(JObject j, Dictionary<string, JProperty> removeFrom,
        params string[] aliases)
    {
        var token = GetToken(j, removeFrom, aliases);
        if (token == null || token.Type == JTokenType.Null)
            return null;

        return token.Type switch
        {
            JTokenType.Integer => token.Value<ulong>(),
            JTokenType.String when ulong.TryParse(token.Value<string>()!, out var r) => r,
            _ => null
        };
    }

    private static string GetBase64(string propertyName, JToken token)
    {
        if (token.Type == JTokenType.Null) throw new ArgumentNullException(propertyName, $"{propertyName} is null");
        if (token.Type == JTokenType.String)
        {
            var result = token.Value<string>()!;
            Validate.IsValidBase64(propertyName, result);
            return result;
        }

        if (token.Type == JTokenType.Array)
        {
            try
            {
                var result = token.Value<byte[]>()!;
                var base64 = Convert.ToBase64String(result);
                return base64;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"{propertyName}'s Array is not valid base64", e);
            }
        }

        throw new ArgumentOutOfRangeException(nameof(token.Type),
            $"Not valid token type for base64 property '{propertyName}'. Type: {token.Type}");
    }


    private static string GetDeviceId(JObject j, MafileSerializerSettings settings,
        Dictionary<string, JProperty> removeFrom, string propertyName,
        params string[] aliases)
    {
        var deviceId = GetString(j, removeFrom, aliases);
        if (string.IsNullOrWhiteSpace(deviceId) && !settings.DeserializationOptions.AllowDeviceIdGeneration)
        {
            throw new ArgumentException($"Required property {propertyName} not found");
        }

        return deviceId ?? GenerateDeviceId();


        static string GenerateDeviceId()
        {
            return "android:" + Guid.NewGuid();
        }
    }

    private static ulong? GetSerialNumber(JObject j, MafileSerializerSettings settings, string propertyName,
        Dictionary<string, JProperty> removeFrom,
        params string[] aliases)
    {
        var token = GetToken(j, removeFrom, aliases);
        if (token == null || token.Type == JTokenType.Null)
            return null;

        if (token.Type is JTokenType.Integer or JTokenType.String)
        {
            var bigInt = token.ToObject<BigInteger>();
            ulong res;
            if (bigInt < ulong.MinValue && bigInt > long.MinValue) //Negative, e.g. -2260921916482386064
            {
                res = settings.DeserializationOptions.RestrictOverflowSerialNumberRecovery
                    ? 0
                    : GetFromOverflow((long) bigInt);
            }
            else if (bigInt > ulong.MinValue && bigInt < ulong.MaxValue) //Valid range
            {
                res = (ulong) bigInt;
            }
            else
            {
                res = 0;
            }

            if (res == 0 && settings.DeserializationOptions.ThrowIfInvalidSerialNumber)
                throw new ArgumentException(
                    $"SerialNumber has invalid value. Value: '{token.ToObject<object>()}'. Property: '{(token as JProperty)?.Name ?? propertyName}'");
            return res;
        }

        throw new ArgumentOutOfRangeException(nameof(token.Type),
            $"Not valid token type for base64 property '{propertyName}'. Type: {token.Type}");

        static ulong GetFromOverflow(long overflow)
        {
            ulong originalValue;
            unchecked
            {
                originalValue = (ulong) overflow + ulong.MaxValue + 1;
            }

            return originalValue;
        }
    }

    private static SteamId? GetSteamId(JObject j, Dictionary<string, JProperty> removeFrom,
        params string[] aliases)
    {
        var id = GetLong(j, removeFrom, aliases);
        return id switch
        {
            null or < SteamId64.SEED => null,
            _ => SteamId.FromSteam64(id.Value)
        };
    }
}