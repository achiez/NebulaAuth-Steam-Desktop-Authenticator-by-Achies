using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SteamLib.Utility.MaFiles;

public static partial class MafileSerializer //Write
{
    private const string CREDITS_PROPERTY_NAME = "Credits";
    public static string Serialize(MobileData mobileData, Formatting formatting = Formatting.Indented, bool sign = true, MafileCredits? credits = null)
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

    public static async Task<string> SerializeAsync(MobileData mobileData, Formatting formatting = Formatting.Indented, bool sign = true, MafileCredits? credits = null)
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

    public static string SerializeLegacy(MobileData mobileData, Formatting formatting, Dictionary<string, object?>? additionalProperties = null, bool sign = true, MafileCredits? credits = null)
    {
        var result = new LegacyMafile
        {
            SharedSecret = mobileData.SharedSecret,
            IdentitySecret = mobileData.IdentitySecret,
            DeviceId = mobileData.DeviceId,

        };

        if (mobileData is MobileDataExtended ext)
        {
            result.RevocationCode = ext.RevocationCode ?? string.Empty;
            result.AccountName = ext.AccountName;
            result.SessionData = ext.SessionData == null
                ? null
                : new
                {
                    AccesToken = ext.SessionData?.MobileToken?.Token,
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

}