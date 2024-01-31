using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace SteamLib.Utility.MaFiles;

public enum DeserializedMafileSessionResult
{
    Missing,
    Invalid,
    Expired,
    Valid,
}
public class DeserializedMafileData
{
    public int? Version { get; init; }
    public bool IsExtended { get; init; }
    public DeserializedMafileSessionResult SessionResult { get; init; }
    public Dictionary<string, JProperty>? UnusedProperties { get; init; } = null;
    public HashSet<string>? MissingProperties { get; init; } = new();


    public bool IsActual => Version == MafileSerializer.MAFILE_VERSION;
    public bool IsLegacy => Version == 0;
    public bool HasUnusedProperties => UnusedProperties is { Count: > 0 };
    public bool HasMissingProperties => MissingProperties is { Count: > 0 };

    internal static DeserializedMafileData Create(int? version = null, bool isExtended = false, Dictionary<string, JProperty>? properties = null, HashSet<string>? missingProperties = null, DeserializedMafileSessionResult sessionResult = DeserializedMafileSessionResult.Missing)
    {
        return new DeserializedMafileData
        {
            Version = version,
            UnusedProperties = properties,
            IsExtended = isExtended,
            MissingProperties = missingProperties,
            SessionResult = sessionResult
        };
    }

    internal static DeserializedMafileData CreateActual(Dictionary<string, JProperty>? properties = null, DeserializedMafileSessionResult sessionResult = DeserializedMafileSessionResult.Valid)
    {
        return new DeserializedMafileData
        {
            Version = MafileSerializer.MAFILE_VERSION,
            UnusedProperties = properties,
            IsExtended = true,
            SessionResult = sessionResult
        };
    }
}


/// <summary>
/// Representation class just for displaying deserialization result
/// </summary>
[PublicAPI]
public class DeserializedMafileResult
{
    public static readonly HashSet<string> ImportantProperties = new()
    {
        nameof(MobileDataExtended.RevocationCode),
        nameof(MobileDataExtended.AccountName),
    };

    public static readonly HashSet<string> NotImportantProperties = new()
    {
        nameof(MobileDataExtended.ServerTime),
        nameof(MobileDataExtended.SerialNumber),
        nameof(MobileDataExtended.Uri),
        nameof(MobileDataExtended.TokenGid),
        nameof(MobileDataExtended.Secret1),
    };

    public int? Version { get; init; }

    public bool IsExtended { get; init; }
    public DeserializedMafileSessionResult SessionResult { get; init; }

    public HashSet<string>? MissingImportantProperties { get; init; } = new();
    public HashSet<string>? MissingProperties { get; init; } = new();
    public Dictionary<string, string>? UnusedProperties { get; init; }

    public bool IsActual => Version == MafileSerializer.MAFILE_VERSION;
    public bool HasUnusedProperties => UnusedProperties is { Count: > 0 };
    public bool HasMissingProperties => MissingProperties is { Count: > 0 };
    public bool HasMissingImportantProperties => MissingImportantProperties is { Count: > 0 };
    public bool HasSession => SessionResult == DeserializedMafileSessionResult.Valid;


    public static DeserializedMafileResult FromData(DeserializedMafileData data)
    {
        HashSet<string>? missingImportantProperties = null;
        HashSet<string>? missingProperties = null;
        if (data is { IsExtended: true, HasMissingProperties: true })
        {
            var important = data.MissingProperties?.Intersect(ImportantProperties).ToList();
            if (important?.Count > 0) missingImportantProperties = important.ToHashSet();
            var notImportant = data.MissingProperties?.Intersect(NotImportantProperties).ToList();
            if (notImportant?.Count > 0) missingProperties = notImportant.ToHashSet();
        }

        return new DeserializedMafileResult
        {
            Version = data.Version,
            IsExtended = data.IsExtended,
            MissingImportantProperties = missingImportantProperties,
            MissingProperties = missingProperties,
            UnusedProperties = data.UnusedProperties?.ToDictionary(x => x.Key, x => JTokenToString(x.Value)),
            SessionResult = data.SessionResult
        };
    }

    private static string JTokenToString(JProperty property)
    {
        switch (property.Value.Type)
        {
            case JTokenType.None:
            case JTokenType.Null:
                return "null";
            case JTokenType.Object:
                return "object";
            case JTokenType.Array:
                return "array";
            case JTokenType.Integer:
                return property.Value.Value<long>().ToString();
            case JTokenType.Float:
                return property.Value.Value<double>().ToString(CultureInfo.InvariantCulture);
            case JTokenType.String:
                return property.Value.Value<string>()!;
            case JTokenType.Boolean:
                return property.Value.Value<bool>().ToString();
            case JTokenType.Date:
                return property.Value.Value<DateTime>().ToString(CultureInfo.InvariantCulture);
            case JTokenType.Guid:
                return property.Value.Value<Guid>().ToString();
            case JTokenType.Uri:
                return property.Value.Value<Uri>()!.ToString();
            case JTokenType.TimeSpan:
                return property.Value.Value<TimeSpan>().ToString();
            default:
                return "unknown";
        }
    }
}