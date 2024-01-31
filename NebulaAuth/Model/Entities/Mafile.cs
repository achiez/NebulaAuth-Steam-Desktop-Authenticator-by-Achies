using SteamLib;

namespace NebulaAuth.Model.Entities;

public class Mafile : MobileDataExtended
{
    public MaProxy? Proxy { get; set; }
    public string? Group { get; set; }
    public string? Password { get; set; }

    public static Mafile FromMobileDataExtended(MobileDataExtended data)
    {
        return new Mafile
        {
            AccountName = data.AccountName,
            DeviceId = data.DeviceId,
            IdentitySecret = data.IdentitySecret,
            RevocationCode = data.RevocationCode,
            Secret1 = data.Secret1,
            SerialNumber = data.SerialNumber,
            SessionData = data.SessionData,
            SharedSecret = data.SharedSecret,
            ServerTime = data.ServerTime,
            TokenGid = data.TokenGid,
            Uri = data.Uri,
        };
    }


    public static Mafile FromMobileDataExtended(MobileDataExtended data, MaProxy? proxy, string? group, string? password)
    {
        var result = FromMobileDataExtended(data);
        result.Proxy = proxy;
        result.Group = group;
        result.Password = password;
        return result;
    }
}