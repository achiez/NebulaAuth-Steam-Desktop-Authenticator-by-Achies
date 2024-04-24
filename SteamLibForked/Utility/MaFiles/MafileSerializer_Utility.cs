using Newtonsoft.Json.Linq;

namespace SteamLib.Utility.MaFiles;

public partial class MafileSerializer //Utility
{
    private static JToken? GetToken(JObject j,  params string[] aliases)
    {
        foreach (var name in aliases)
        {
            if (j.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var token))
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
            if (!j.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var token)) continue;
            var parent = token.Parent as JProperty;
            removeFrom.Remove(parent!.Name);
            return token;

        }
        return null;
    }

    private static JToken GetTokenOrThrow(JObject j, string propertyName, Dictionary<string, JProperty> removeFrom, params string[] aliases)
    {
        foreach (var name in aliases)
        {
            if (!j.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var token)) continue;
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
        else if (token.Type == JTokenType.Array)
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

}