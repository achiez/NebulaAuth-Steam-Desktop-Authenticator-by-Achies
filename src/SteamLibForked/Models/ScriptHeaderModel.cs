namespace SteamLib.Models;

public class ScriptHeaderModel
{
    public required string SessionId { get; set; }
    public SteamId? SteamId { get; set; }
}