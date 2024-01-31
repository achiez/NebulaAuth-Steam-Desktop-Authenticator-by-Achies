using AchiesUtilities.Models;
using Newtonsoft.Json;
using SteamLib.SteamMobile.Confirmations;

namespace SteamLib.Web.Models.Mobile;

public class ConfirmationsJson
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("needauth")]
    public bool NeedAuth { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }


    [JsonProperty("detail")]
    public string? Detail { get; set; }
    [JsonProperty("conf")] public List<ConfirmationJson> Conf { get; set; } = new();

}


public class ConfirmationJson
{
    [JsonProperty("type")] public ConfirmationType Type { get; set; }
    [JsonProperty("type_name")] public string TypeName { get; set; } = string.Empty;
    [JsonProperty("id")] public long Id { get; set; }
    [JsonProperty("creator_id")] public long CreatorId { get; set; }
    [JsonProperty("nonce")] public ulong Nonce { get; set; }
    [JsonProperty("creation_time")] public UnixTimeStamp CreationTime { get; set; }
    [JsonProperty("cancel")] public string Cancel { get; set; } = string.Empty;
    [JsonProperty("accept")] public string Confirm { get; set; } = string.Empty;
    [JsonProperty("icon")] public Uri? Icon { get; set; }
    [JsonProperty("multi")] public bool Multi { get; set; }
    [JsonProperty("headline")] public string Headline { get; set; } = string.Empty;
    [JsonProperty("summary")] public List<string> Summary { get; set; } = null!;
    [JsonProperty("warn")] public List<string> Warn { get; set; } = null!;
}
