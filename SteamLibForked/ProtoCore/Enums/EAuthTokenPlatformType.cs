using ProtoBuf;

namespace SteamLib.ProtoCore.Enums;

[ProtoContract]
public enum EAuthTokenPlatformType
{
    Unknown = 0,
    SteamClient = 1,
    WebBrowser = 2,
    MobileApp = 3,
}