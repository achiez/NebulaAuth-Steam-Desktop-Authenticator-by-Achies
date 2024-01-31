using Newtonsoft.Json;
using SteamLib.Account;

namespace SteamLib;

//WARNING: Any changes here should be reflected in MafileSerializer.cs

public class MobileData
{
    [JsonRequired] public string SharedSecret { get; set; } = null!;
    [JsonRequired] public string IdentitySecret { get; set; } = null!;
    [JsonRequired] public string DeviceId { get; set; } = null!;

}

public class MobileDataExtended : MobileData
{
    public string? RevocationCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public MobileSessionData? SessionData { get; set; }


    #region Unused
    public long ServerTime { get; set; } //Unused
    public ulong SerialNumber { get; set; } //Unused //greater than long must be ulong or string
    public string Uri { get; set; } = null!;//Unused
    public string TokenGid { get; set; } = null!;//Unused
    public string Secret1 { get; set; } = null!; //Unused

    #endregion
}