using Newtonsoft.Json;
using SteamLib.Core.Models;

namespace SteamLib.Web.Converters;

public class SteamIdToSteam64Converter : JsonConverter<SteamId>
{
    public override void WriteJson(JsonWriter writer, SteamId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Steam64.ToLong());
    }

    public override SteamId ReadJson(JsonReader reader, Type objectType, SteamId existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is long l)
        {
            return SteamId.FromSteam64(l);
        }

        var str = (string)reader.Value!;
        return new SteamId(SteamId64.Parse(str));
    }
}

public class SteamIdToSteam2Converter : JsonConverter<SteamId>
{
    public override void WriteJson(JsonWriter writer, SteamId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Steam2.ToString());
    }

    public override SteamId ReadJson(JsonReader reader, Type objectType, SteamId existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {

        var str = (string)reader.Value!;
        return new SteamId(SteamId2.Parse(str));
    }
}


public class SteamIdToSteam3Converter : JsonConverter<SteamId>
{
    public override void WriteJson(JsonWriter writer, SteamId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Steam3.ToString());
    }

    public override SteamId ReadJson(JsonReader reader, Type objectType, SteamId existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {

        var str = (string)reader.Value!;
        return new SteamId(SteamId3.Parse(str));
    }
}

public class Steam64ToLongConverter : JsonConverter<SteamId64>
{
    public override void WriteJson(JsonWriter writer, SteamId64 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToLong());
    }

    public override SteamId64 ReadJson(JsonReader reader, Type objectType, SteamId64 existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is long l)
        {
            return new SteamId64(l);
        }
        var str = (string)reader.Value!;
        return SteamId64.Parse(str);
    }
}

public class SteamId2ToStringConverter : JsonConverter<SteamId2>
{
    public override void WriteJson(JsonWriter writer, SteamId2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override SteamId2 ReadJson(JsonReader reader, Type objectType, SteamId2 existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var str = (string)reader.Value!;
        return SteamId2.Parse(str);
    }
}

public class SteamId3ToStringConverter : JsonConverter<SteamId3>
{
    public override void WriteJson(JsonWriter writer, SteamId3 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override SteamId3 ReadJson(JsonReader reader, Type objectType, SteamId3 existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var str = (string)reader.Value!;
        return SteamId3.Parse(str);
    }
}