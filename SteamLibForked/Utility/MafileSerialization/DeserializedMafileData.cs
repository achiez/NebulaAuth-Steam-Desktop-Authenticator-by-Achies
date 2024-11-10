﻿using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SteamLib.Core.Models;

namespace SteamLib.Utility.MafileSerialization;

public enum DeserializedMafileSessionResult
{
    Missing,
    Invalid,
    Valid,
}

[PublicAPI]
public class DeserializedMafileData
{
    public MobileData Data { get; init; }
    public DeserializedMafileInfo Info { get; }


    private DeserializedMafileData(MobileData mobileData, DeserializedMafileInfo info)
    {
        Data = mobileData;
        Info = info;
    }

    internal static DeserializedMafileData Create(MobileData mobileData, int? version = null, Dictionary<string, JProperty>? properties = null, HashSet<string>? missingProperties = null, DeserializedMafileSessionResult sessionResult = DeserializedMafileSessionResult.Missing)
    {
        var info = DeserializedMafileInfo.Create(mobileData, version, properties, missingProperties, sessionResult);
        return new DeserializedMafileData(mobileData, info);
    }

    internal static DeserializedMafileData CreateActual(MobileData mobileData, Dictionary<string, JProperty>? properties = null, DeserializedMafileSessionResult sessionResult = DeserializedMafileSessionResult.Valid)
    {
        var info = DeserializedMafileInfo.Create(mobileData, MafileSerializer.MAFILE_VERSION, properties, null, sessionResult);
        return new DeserializedMafileData(mobileData, info);
    }
}


/// <summary>
/// Represents information about deserialized mafile
/// </summary>
[PublicAPI]
public class DeserializedMafileInfo
{
    public static readonly HashSet<string> ImportantProperties =
    [
        nameof(MobileDataExtended.RevocationCode),
        nameof(MobileDataExtended.AccountName),
    ];

    public static readonly HashSet<string> NotImportantProperties =
    [
        nameof(MobileDataExtended.ServerTime),
        nameof(MobileDataExtended.SerialNumber),
        nameof(MobileDataExtended.Uri),
        nameof(MobileDataExtended.TokenGid),
        nameof(MobileDataExtended.Secret1),
        nameof(MobileDataExtended.SteamId)
    ];

    public int? Version { get; init; }

    public bool IsExtended { get; init; }
    public DeserializedMafileSessionResult SessionResult { get; init; }

    public HashSet<string>? MissingImportantProperties { get; init; } = [];
    public HashSet<string>? MissingProperties { get; init; } = [];
    public Dictionary<string, JProperty>? UnusedProperties { get; init; }

    public bool IsActual => Version == MafileSerializer.MAFILE_VERSION;
    public bool HasUnusedProperties => UnusedProperties is { Count: > 0 };
    public bool HasMissingProperties => MissingProperties is { Count: > 0 };
    public bool HasMissingImportantProperties => MissingImportantProperties is { Count: > 0 };
    public bool HasSession => SessionResult == DeserializedMafileSessionResult.Valid;
    public bool HasIdentificationProperty { get; init; }

    internal static DeserializedMafileInfo Create(MobileData mobileData, int? version = null, Dictionary<string, JProperty>? unusedProperties = null, HashSet<string>? missingProperties = null,
        DeserializedMafileSessionResult sessionResult = DeserializedMafileSessionResult.Missing)
    {
        HashSet<string>? missingImportantProperties = null;
        var hasIdentificationProperty = false;
        var isExtended = false;
        if (mobileData is MobileDataExtended ext)
        {
            hasIdentificationProperty = !string.IsNullOrWhiteSpace(ext.AccountName) || ext.SteamId.Steam64.Id > SteamId64.SEED;
            isExtended = true;
        }

        if (isExtended && missingProperties is { Count: > 0 })
        {
            var important = missingProperties.Intersect(ImportantProperties).ToList();
            if (important.Count > 0) missingImportantProperties = important.ToHashSet();
            var notImportant = missingProperties.Intersect(NotImportantProperties).ToList();
            if (notImportant.Count > 0) missingProperties = notImportant.ToHashSet();
        }

        return new DeserializedMafileInfo
        {
            Version = version,
            IsExtended = isExtended,
            MissingImportantProperties = missingImportantProperties,
            MissingProperties = missingProperties,
            UnusedProperties = unusedProperties,
            SessionResult = sessionResult,
            HasIdentificationProperty = hasIdentificationProperty
        };
    }

}