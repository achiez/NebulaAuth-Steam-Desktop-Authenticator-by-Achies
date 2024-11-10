using NebulaAuth.Model.Entities;
using Newtonsoft.Json.Linq;
using SteamLib;
using SteamLib.Utility.MafileSerialization;
using System.Collections.Generic;
using System;
using NebulaAuth.Model.Exceptions;
using Newtonsoft.Json;

namespace NebulaAuth.Model;

public static class NebulaSerializer
{
    public static MafileSerializer Serializer { get; }

    static NebulaSerializer()
    {
        Serializer = new MafileSerializer(new MafileSerializerSettings
        {
            DeserializationOptions =
            {
                AllowDeviceIdGeneration = true,
                AllowSessionIdGeneration = true,
                ThrowIfInvalidSteamId = false
            }
        });
    }


    public static Mafile Deserialize(string cont)
    {
        var data = Serializer.Deserialize(cont);
        var mobileData = data.Data;
        var info = data.Info;
        if (info.IsExtended == false)
            throw new FormatException("Mafile is not extended data");
      

        var props = info.UnusedProperties ?? new Dictionary<string, JProperty>();
        var proxy = GetPropertyValue<MaProxy>("Proxy", props);
        var group = GetPropertyValue<string>("Group", props);
        var password = GetPropertyValue<string>("Password", props);
        var mafile = Mafile.FromMobileDataExtended((MobileDataExtended)mobileData, proxy, group, password);


        if (!info.SteamIdValid)
            throw new MafileNeedReloginException(mafile);

        return mafile;
    }

    private static T? GetPropertyValue<T>(string name, Dictionary<string, JProperty> dictionary)
    {
        if (dictionary.TryGetValue(name, out var prop) == false) return default;
        var value = prop.Value;
        try
        {
            return value.ToObject<T>();
        }
        catch (Exception ex)
        {
            Shell.Logger.Warn(ex, "Can't deserialize property {name}", name);
            return default;
        }
    }

    public static string SerializeMafile(Mafile data)
    {
        var props = new Dictionary<string, object?>
        {
            {nameof(Mafile.Proxy), data.Proxy},
            {nameof(Mafile.Group), data.Group},
            {nameof(Mafile.Password), data.Password}
        };
        return SerializeMafile(data, props);
    }


    public static string SerializeMafile(MobileDataExtended data, Dictionary<string, object?>? properties)
    {
        if (Settings.Instance.LegacyMode)
        {
            return MafileSerializer.SerializeLegacy(data, Formatting.Indented, properties);
        }
        else
        {
            return MafileSerializer.Serialize(data);
        }
    }
}