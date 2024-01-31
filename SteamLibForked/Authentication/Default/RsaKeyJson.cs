// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
#pragma warning disable CS8618

namespace SteamLib.Login.Default;

/// <summary>
/// Class to Deserialize the json response strings of the getResKey request/>
/// </summary>
internal class RsaKeyJson
{
    public bool success { get; set; }
    public string publickey_mod { get; set; }
    public string publickey_exp { get; set; }
    public string timestamp { get; set; }
}