using Newtonsoft.Json;

namespace SteamLib.Web.Converters;

public class StringToLongIfNeededConverter : JsonConverter<string>
{
    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }

    public override string ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return reader.Value?.ToString() ?? "null";
    }
}