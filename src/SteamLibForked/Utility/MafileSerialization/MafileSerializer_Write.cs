using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SteamLib.Utility.MafileSerialization;

public partial class MafileSerializer //Write
{
    private const string CREDITS_PROPERTY_NAME = "Credits";

    #region Instance

    public string Serialize(MobileData mobileData)
    {
        return Serialize(mobileData, Settings.SerializationOptions);
    }

    public Task<string> SerializeAsync(MobileData mobileData)
    {
        return SerializeAsync(mobileData, Settings.SerializationOptions);
    }

    public string SerializeLegacy(MobileData mobileData, IDictionary<string, object?>? additionalProperties = null)
    {
        return SerializeLegacy(mobileData, Settings.SerializationOptions, additionalProperties);
    }

    public Task<string> SerializeLegacyAsync(MobileData mobileData,
        IDictionary<string, object?>? additionalProperties = null)
    {
        return SerializeLegacyAsync(mobileData, Settings.SerializationOptions, additionalProperties);
    }

    #endregion

    #region Default

    public static string Serialize(MobileData mobileData, MafileSerializationOptions? options)
    {
        options ??= new MafileSerializationOptions();
        var j = CreateJObject(mobileData, options);
        using var w = new StringWriter();
        using var write = new JsonTextWriter(w);
        write.Formatting = options.Formatting;
        j.WriteTo(write);
        return w.ToString();
    }


    public static async Task<string> SerializeAsync(MobileData mobileData, MafileSerializationOptions? options)
    {
        options ??= new MafileSerializationOptions();
        var j = CreateJObject(mobileData, options);
        await using var w = new StringWriter();
        await using var write = new JsonTextWriter(w);
        write.Formatting = options.Formatting;
        await j.WriteToAsync(write);
        return w.ToString();
    }


    private static JObject CreateJObject(MobileData mobileData, MafileSerializationOptions options)
    {
        var sign = options.Sign;
        var credits = options.Credits;


        var j = JObject.FromObject(mobileData);
        j.Add(SIGNATURE_PROPERTY_NAME, j);
        if (sign)
        {
            credits ??= MafileCredits.Instance;
            var obj = JObject.FromObject(credits);
            j.Add(CREDITS_PROPERTY_NAME, obj);
        }

        return j;
    }

    #endregion

    #region Legacy

    public static string SerializeLegacy(MobileData mobileData, MafileSerializationOptions? options,
        IDictionary<string, object?>? additionalProperties = null)
    {
        options ??= new MafileSerializationOptions();
        var j = CreateLegacyJObject(mobileData, options, additionalProperties);
        using var w = new StringWriter();
        using var write = new JsonTextWriter(w);
        write.Formatting = options.Formatting;
        j.WriteTo(write);
        return w.ToString();
    }

    public static async Task<string> SerializeLegacyAsync(MobileData mobileData, MafileSerializationOptions? options,
        IDictionary<string, object?>? additionalProperties = null)
    {
        options ??= new MafileSerializationOptions();
        var j = CreateLegacyJObject(mobileData, options, additionalProperties);
        await using var w = new StringWriter();
        await using var write = new JsonTextWriter(w);
        write.Formatting = options.Formatting;
        await j.WriteToAsync(write);
        return w.ToString();
    }

    private static JObject CreateLegacyJObject(MobileData mobileData, MafileSerializationOptions options,
        IDictionary<string, object?>? additionalProperties = null)
    {
        var sign = options.Sign;
        var credits = options.Credits;

        var result = new LegacyMafile
        {
            SharedSecret = mobileData.SharedSecret,
            IdentitySecret = mobileData.IdentitySecret,
            DeviceId = mobileData.DeviceId
        };

        if (mobileData is MobileDataExtended ext)
        {
            result.RevocationCode = ext.RevocationCode ?? string.Empty;
            result.AccountName = ext.AccountName;
            result.SessionData = ext.SessionData == null
                ? null
                : new
                {
                    AccessToken = ext.SessionData?.MobileToken?.Token,
                    steamLoginSecure = ext.SessionData?.MobileToken?.SignedToken,
                    RefreshToken = ext.SessionData?.RefreshToken.Token,
                    SteamID = ext.SessionData?.SteamId.Steam64.Id,
                    SessionID = ext.SessionData?.SessionId
                };
            result.ServerTime = ext.ServerTime;
            result.SerialNumber = ext.SerialNumber.ToString();
            result.Uri = ext.Uri;
            result.TokenGid = ext.TokenGid;
            result.Secret1 = ext.Secret1;
            result.SteamId = ext.SteamId.Steam64.Id;
        }


        var j = JObject.FromObject(result);
        if (additionalProperties != null)
        {
            foreach (var (name, value) in additionalProperties)
            {
                JToken? jToken = null;
                if (value != null)
                {
                    jToken = JToken.FromObject(value);
                }

                j.Add(name, jToken);
            }
        }

        if (sign)
        {
            credits ??= MafileCredits.Instance;
            var obj = JObject.FromObject(credits);
            j.Add(CREDITS_PROPERTY_NAME, obj);
        }

        return j;
    }

    #endregion

    #region Obsolete

    [Obsolete("Use Serialize(MobileData, MafileSerializationOptions) instead.")]
    public static string Serialize(MobileData mobileData, Formatting formatting, bool sign = true,
        MafileCredits? credits = null)
    {
        using var w = new StringWriter();
        using var write = new JsonTextWriter(w);
        write.Formatting = formatting;
        var j = JObject.FromObject(mobileData);
        j.Add(SIGNATURE_PROPERTY_NAME, MAFILE_VERSION);
        if (sign)
        {
            credits ??= MafileCredits.Instance;
            var obj = JObject.FromObject(credits);
            j.Add(CREDITS_PROPERTY_NAME, obj);
        }

        j.WriteTo(write);
        return w.ToString();
    }

    [Obsolete("Use SerializeAsync(MobileData, MafileSerializationOptions) instead.")]
    public static async Task<string> SerializeAsync(MobileData mobileData, Formatting formatting, bool sign = true,
        MafileCredits? credits = null)
    {
        await using var w = new StringWriter();
        await using var write = new JsonTextWriter(w);
        write.Formatting = formatting;
        var j = JObject.FromObject(mobileData);
        j.Add(SIGNATURE_PROPERTY_NAME, j);
        if (sign)
        {
            credits ??= MafileCredits.Instance;
            var obj = JObject.FromObject(credits);
            j.Add(CREDITS_PROPERTY_NAME, obj);
        }


        await j.WriteToAsync(write);
        return w.ToString();
    }

    [Obsolete("Use SerializeLegacy(MobileData, MafileSerializationOptions, Dictionary<string, object?>) instead.")]
    public static string SerializeLegacy(MobileData mobileData, Formatting formatting,
        Dictionary<string, object?>? additionalProperties = null, bool sign = true, MafileCredits? credits = null)
    {
        var result = new LegacyMafile
        {
            SharedSecret = mobileData.SharedSecret,
            IdentitySecret = mobileData.IdentitySecret,
            DeviceId = mobileData.DeviceId
        };

        if (mobileData is MobileDataExtended ext)
        {
            result.RevocationCode = ext.RevocationCode ?? string.Empty;
            result.AccountName = ext.AccountName;
            result.SessionData = ext.SessionData == null
                ? null
                : new
                {
                    AccessToken = ext.SessionData?.MobileToken?.Token,
                    steamLoginSecure = ext.SessionData?.MobileToken?.SignedToken,
                    RefreshToken = ext.SessionData?.RefreshToken.Token,
                    SteamID = ext.SessionData?.SteamId.Steam64.Id,
                    SessionID = ext.SessionData?.SessionId
                };
            result.ServerTime = ext.ServerTime;
            result.SerialNumber = ext.SerialNumber.ToString();
            result.Uri = ext.Uri;
            result.TokenGid = ext.TokenGid;
            result.Secret1 = ext.Secret1;
            result.SteamId = ext.SteamId.Steam64.Id;
        }


        using var w = new StringWriter();
        using var write = new JsonTextWriter(w);
        write.Formatting = formatting;
        var j = JObject.FromObject(result);
        if (additionalProperties != null)
        {
            foreach (var (name, value) in additionalProperties)
            {
                JToken? jToken = null;
                if (value != null)
                {
                    jToken = JToken.FromObject(value);
                }

                j.Add(name, jToken);
            }
        }

        if (sign)
        {
            credits ??= MafileCredits.Instance;
            var obj = JObject.FromObject(credits);
            j.Add(CREDITS_PROPERTY_NAME, obj);
        }

        j.WriteTo(write);
        return w.ToString();
    }

    #endregion
}