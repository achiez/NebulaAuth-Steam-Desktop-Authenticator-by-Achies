using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamLib.Account;

namespace SteamLib.Utility.MaFiles;

public static partial class MafileSerializer
{
    public const int MAFILE_VERSION = 2;

    public const string SIGNATURE_PROPERTY_NAME = "@SLSV";
    private static readonly HashSet<string> ActualProperties = typeof(MobileDataExtended).GetProperties().Select(x => x.Name).ToHashSet();


    public static MobileData Deserialize(string json, out DeserializedMafileData mafileData)
    {

        var j = JObject.Parse(json);

        var unusedProperties = j.Properties().ToDictionary(x => x.Name, x => x);
        var versionToken = GetToken(j, SIGNATURE_PROPERTY_NAME);
        unusedProperties.Remove(CREDITS_PROPERTY_NAME);
        int? version = null;
        if (versionToken is { Type: JTokenType.Integer })
        {
            version = versionToken.Value<int>();
        }

        unusedProperties.Remove(SIGNATURE_PROPERTY_NAME);
        if (version == MAFILE_VERSION)
        {
            try
            {
                var unused = new Dictionary<string, JProperty>();
                foreach (var (key, prop) in unusedProperties)
                {
                    if (ActualProperties.Contains(key))
                        continue;
                    unused.Add(key, prop);
                }

                var data = j.ToObject<MobileDataExtended>()!;
                data.SessionData = Validate.ValidateMobileData(data, out var sessionResult);

                mafileData = DeserializedMafileData.CreateActual(unused, sessionResult);
                return data;

            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("MafileSerializer found actual 'Version' signature but some error occurred during deserialization", ex);
            }
        }

        var sharedSecretToken = GetTokenOrThrow(j, nameof(MobileData.SharedSecret), unusedProperties, "sharedsecret", "shared_secret", "shared");
        var identitySecretToken = GetTokenOrThrow(j, nameof(MobileData.IdentitySecret), unusedProperties, "identitysecret", "identity_secret", "identity");
        var deviceIdToken = GetTokenOrThrow(j, nameof(MobileData.DeviceId), unusedProperties, "deviceid", "device_id", "device");


        var sharedSecret = GetBase64(nameof(MobileData.SharedSecret), sharedSecretToken);
        var identitySecret = GetBase64(nameof(MobileData.IdentitySecret), identitySecretToken);
        var deviceId = deviceIdToken.Value<string>();
        Validate.NotNullOrEmpty(nameof(MobileData.DeviceId), deviceId);

        var accountNameToken = GetToken(j, unusedProperties, nameof(MobileDataExtended.AccountName), "account_name", "accountname");
        var revocationCodeToken = GetToken(j, unusedProperties, nameof(MobileDataExtended.RevocationCode), "revocation_code", "rcode", "r_code", "revocationcode");
        var sessionDataToken = GetToken(j, unusedProperties, nameof(MobileDataExtended.SessionData), "session_data", "sessiondata", "session");

        var accountName = accountNameToken?.Value<string>();
        var revocationCode = revocationCodeToken?.Value<string>();


        var minified
            = string.IsNullOrWhiteSpace(accountName) 
              && string.IsNullOrWhiteSpace(revocationCode) 
              && (sessionDataToken == null || sessionDataToken.Type == JTokenType.Null);


        if (minified)
        {
            mafileData = DeserializedMafileData.Create(version, false, unusedProperties);
            return new MobileData
            {
                DeviceId = deviceId,
                IdentitySecret = identitySecret,
                SharedSecret = sharedSecret
            };
        }


        var missingProperties = new List<string>();
        if (string.IsNullOrWhiteSpace(accountName)) missingProperties.Add(nameof(MobileDataExtended.AccountName));
        if (string.IsNullOrWhiteSpace(revocationCode)) missingProperties.Add(nameof(MobileDataExtended.RevocationCode));
        
        var serverTime = GetLong(j,  unusedProperties, nameof(MobileDataExtended.ServerTime), "server_time", "servertime");
        var serialNumber = GetULong(j, unusedProperties, nameof(MobileDataExtended.SerialNumber), "serial_number", "serialnumber");
        var uri = GetString(j,unusedProperties, nameof(MobileDataExtended.Uri), "url", "uri");
        var tokenGid = GetString(j, unusedProperties, nameof(MobileDataExtended.TokenGid), "token_gid", "tokengid");
        var secret1 = GetString(j, unusedProperties, nameof(MobileDataExtended.Secret1), "secret_1", "seecret1");

        if(serverTime == null) missingProperties.Add(nameof(MobileDataExtended.ServerTime));
        if(serialNumber == null) missingProperties.Add(nameof(MobileDataExtended.SerialNumber));
        if(string.IsNullOrWhiteSpace(uri)) missingProperties.Add(nameof(MobileDataExtended.Uri));
        if(string.IsNullOrWhiteSpace(tokenGid)) missingProperties.Add(nameof(MobileDataExtended.TokenGid));
        if(string.IsNullOrWhiteSpace(secret1)) missingProperties.Add(nameof(MobileDataExtended.Secret1));

        MobileSessionData? sessionData = null;
        var sResult = DeserializedMafileSessionResult.Missing;
        if (sessionDataToken is { Type: JTokenType.Object })
        {
            sessionData = DeserializeMobileSessionData((JObject) sessionDataToken, out sResult);
        }


        mafileData = DeserializedMafileData.Create(version, true, unusedProperties, missingProperties.ToHashSet(), sResult);
        return new MobileDataExtended
        {
            DeviceId = deviceId,
            IdentitySecret = identitySecret,
            SharedSecret = sharedSecret,
            AccountName = accountName ?? string.Empty,
            RevocationCode = revocationCode,
            ServerTime = serverTime.GetValueOrDefault(),
            SerialNumber = serialNumber.GetValueOrDefault(),
            Uri = uri ?? string.Empty,
            TokenGid = tokenGid ?? string.Empty,
            Secret1 = secret1 ?? string.Empty,
            SessionData = sessionData
        };

    }




}