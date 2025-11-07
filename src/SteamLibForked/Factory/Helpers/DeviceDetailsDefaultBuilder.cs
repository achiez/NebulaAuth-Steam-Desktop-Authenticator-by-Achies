using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Services;
using SteamLib.Utility;

namespace SteamLib.Factory.Helpers;

public static class DeviceDetailsDefaultBuilder
{
    [Obsolete("Not recommended")]
    public static DeviceDetails CreateDefault(string? deviceFriendlyName)
    {
        return new DeviceDetails(deviceFriendlyName ?? string.Empty, EAuthTokenPlatformType.WebBrowser, (int?) null,
            null);
    }

    public static DeviceDetails CreateCommunityDetails(string userAgent)
    {
        return new DeviceDetails(userAgent, EAuthTokenPlatformType.WebBrowser, (int?) null, null);
    }

    public static DeviceDetails CreateMobileDetails(string deviceName)
    {
        return new DeviceDetails(deviceName, EAuthTokenPlatformType.MobileApp, EOSType.AndroidUnknown, 528);
    }

    public static DeviceDetails GetMobileDefaultDevice()
    {
        return new DeviceDetails("Pixel 6 Pro", EAuthTokenPlatformType.MobileApp, -500, 528);
    }
}