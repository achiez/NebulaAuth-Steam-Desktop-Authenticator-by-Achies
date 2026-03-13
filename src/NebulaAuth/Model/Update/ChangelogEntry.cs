using System.Collections.Generic;
using Newtonsoft.Json;

namespace NebulaAuth.Model.Update;

    public class ChangelogEntry
    {
        [JsonProperty("version")] public string Version { get; set; } = string.Empty;

        [JsonProperty("date")] public string Date { get; set; } = string.Empty;

        [JsonProperty("changes")] public List<ChangeItem> Changes { get; set; } = new();
    }

    public class ChangeItem
    {
        [JsonProperty("type")] public string Type { get; set; } = string.Empty;

        [JsonProperty("text")] public string Text { get; set; } = string.Empty;

        [JsonProperty("link")] public string? Link { get; set; }
    }