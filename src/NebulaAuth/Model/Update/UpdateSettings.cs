using System;
using System.IO;
using Newtonsoft.Json;

namespace NebulaAuth.Model.Update;

public class UpdateSettings
{
    private static readonly string FilePath = Path.Combine("settings", "update.json");

    [JsonProperty("skippedVersion")]
    public string? SkippedVersion { get; set; }

    [JsonProperty("remindAfter")]
    public DateTime? RemindAfter { get; set; }

    public static UpdateSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new UpdateSettings();
            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<UpdateSettings>(json) ?? new UpdateSettings();
        }
        catch
        {
            return new UpdateSettings();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath)!;
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    public void SkipVersion(string version)
    {
        SkippedVersion = version;
        RemindAfter = null;
        Save();
    }

    public void SetRemindAfter(DateTime remindAfter)
    {
        RemindAfter = remindAfter;
        Save();
    }

    public bool ShouldShow(string version)
    {
        if (SkippedVersion == version) return false;
        if (RemindAfter.HasValue && DateTime.Now < RemindAfter.Value) return false;
        return true;
    }
}
