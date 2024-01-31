using Newtonsoft.Json;

namespace SteamLib.Login.Default;

#pragma warning disable CS8618
/// <summary>
/// Class to Deserialize the json response strings after the login/>
/// </summary>
internal class LoginResultJson
{
    [JsonProperty("success")] public bool Success { get; set; }
    [JsonProperty("message")] public string Message { get; set; }
    [JsonProperty("captcha_needed")] public bool CaptchaNeeded { get; set; }
    [JsonProperty("captcha_gid")] public string CaptchaGid { get; set; }
    [JsonProperty("emailauth_needed")] public bool EmailAuthNeeded { get; set; }
    [JsonProperty("emailsteamid")] public string EmailSteamId { get; set; }
    [JsonProperty("requires_twofactor")] public bool RequiresTwoFactor { get; set; }
}