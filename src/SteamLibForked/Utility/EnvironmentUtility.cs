using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SteamLib.Utility;

public static class EnvironmentUtility
{
    // ReSharper disable once InconsistentNaming
    public static EOSType GetOSType()
    {
        var osVer = Environment.OSVersion;
        var ver = osVer.Version;

        return osVer.Platform switch
        {
            PlatformID.Win32Windows => ver.Minor switch
            {
                0 => EOSType.Win95,
                10 => EOSType.Win98,
                90 => EOSType.WinME,
                _ => EOSType.WinUnknown
            },

            PlatformID.Win32NT => ver.Major switch
            {
                4 => EOSType.WinNT,
                5 => ver.Minor switch
                {
                    0 => EOSType.Win2000,
                    1 => EOSType.WinXP,
                    // Assume nobody runs Windows XP Professional x64 Edition
                    // It's an edition of Windows Server 2003 anyway.
                    2 => EOSType.Win2003,
                    _ => EOSType.WinUnknown
                },
                6 => ver.Minor switch
                {
                    0 => EOSType.WinVista, // Also Server 2008
                    1 => EOSType.Windows7, // Also Server 2008 R2
                    2 => EOSType.Windows8, // Also Server 2012
                    // Note: The OSVersion property reports the same version number (6.2.0.0) for both Windows 8 and Windows 8.1.- http://msdn.microsoft.com/en-us/library/system.environment.osversion(v=vs.110).aspx
                    // In practice, this will only get hit if the application targets Windows 8.1 in the app manifest.
                    // See http://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx for more info.
                    3 => EOSType.Windows81, // Also Server 2012 R2
                    _ => EOSType.WinUnknown
                },
                10 when ver.Build >= 22000 => EOSType.Win11,
                10 => EOSType.Windows10, // Also Server 2016, Server 2019, Server 2022
                _ => EOSType.WinUnknown
            },

            // The specific minor versions only exist in Valve's enum for LTS versions
            PlatformID.Unix when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => ver.Major switch
            {
                2 => ver.Minor switch
                {
                    2 => EOSType.Linux22,
                    4 => EOSType.Linux24,
                    6 => EOSType.Linux26,
                    _ => EOSType.LinuxUnknown
                },
                3 => ver.Minor switch
                {
                    2 => EOSType.Linux32,
                    5 => EOSType.Linux35,
                    6 => EOSType.Linux36,
                    10 => EOSType.Linux310,
                    16 => EOSType.Linux316,
                    18 => EOSType.Linux318,
                    _ => EOSType.Linux3x
                },
                4 => ver.Minor switch
                {
                    1 => EOSType.Linux41,
                    4 => EOSType.Linux44,
                    9 => EOSType.Linux49,
                    14 => EOSType.Linux414,
                    19 => EOSType.Linux419,
                    _ => EOSType.Linux4x
                },
                5 => ver.Minor switch
                {
                    4 => EOSType.Linux54,
                    10 => EOSType.Linux510,
                    _ => EOSType.Linux5x
                },
                6 => EOSType.Linux6x,
                7 => EOSType.Linux7x,
                _ => EOSType.LinuxUnknown
            },

            PlatformID.Unix when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ver.Major switch
            {
                11 => EOSType.MacOS107, // "Lion"
                12 => EOSType.MacOS108, // "Mountain Lion"
                13 => EOSType.MacOS109, // "Mavericks"
                14 => EOSType.MacOS1010, // "Yosemite"
                15 => EOSType.MacOS1011, // El Capitan
                16 => EOSType.MacOS1012, // Sierra
                17 => EOSType.Macos1013, // High Sierra
                18 => EOSType.Macos1014, // Mojave
                19 => EOSType.Macos1015, // Catalina
                20 => EOSType.MacOS11, // Big Sur
                21 => EOSType.MacOS12, // Monterey
                _ => EOSType.MacOSUnknown
            },

            _ => EOSType.Unknown
        };
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum EOSType
{
    Unknown = -1,
    Web = -700,
    IOSUnknown = -600,
    IOS1 = -599,
    IOS2 = -598,
    IOS3 = -597,
    IOS4 = -596,
    IOS5 = -595,
    IOS6 = -594,
    IOS6_1 = -593,
    IOS7 = -592,
    IOS7_1 = -591,
    IOS8 = -590,
    IOS8_1 = -589,
    IOS8_2 = -588,
    IOS8_3 = -587,
    IOS8_4 = -586,
    IOS9 = -585,
    IOS9_1 = -584,
    IOS9_2 = -583,
    IOS9_3 = -582,
    IOS10 = -581,
    IOS10_1 = -580,
    IOS10_2 = -579,
    IOS10_3 = -578,
    IOS11 = -577,
    IOS11_1 = -576,
    IOS11_2 = -575,
    IOS11_3 = -574,
    IOS11_4 = -573,
    IOS12 = -572,
    IOS12_1 = -571,
    AndroidUnknown = -500,
    Android6 = -499,
    Android7 = -498,
    Android8 = -497,
    Android9 = -496,
    UMQ = -400,
    PS3 = -300,
    MacOSUnknown = -102,
    MacOS104 = -101,
    MacOS105 = -100,
    MacOS1058 = -99,
    MacOS106 = -95,
    MacOS1063 = -94,
    MacOS1064_slgu = -93,
    MacOS1067 = -92,
    MacOS107 = -90,
    MacOS108 = -89,
    MacOS109 = -88,
    MacOS1010 = -87,
    MacOS1011 = -86,
    MacOS1012 = -85,
    Macos1013 = -84,
    Macos1014 = -83,
    Macos1015 = -82,
    MacOS1016 = -81,
    MacOS11 = -80,
    MacOS111 = -79,
    MacOS1017 = -78,
    MacOS12 = -77,
    MacOS13 = -76,
    MacOSMax = -1,
    LinuxUnknown = -203,
    Linux22 = -202,
    Linux24 = -201,
    Linux26 = -200,
    Linux32 = -199,
    Linux35 = -198,
    Linux36 = -197,
    Linux310 = -196,
    Linux316 = -195,
    Linux318 = -194,
    Linux3x = -193,
    Linux4x = -192,
    Linux41 = -191,
    Linux44 = -190,
    Linux49 = -189,
    Linux414 = -188,
    Linux419 = -187,
    Linux5x = -186,
    Linux54 = -185,
    Linux6x = -184,
    Linux7x = -183,
    Linux510 = -182,
    LinuxMax = -101,
    WinUnknown = 0,
    Win311 = 1,
    Win95 = 2,
    Win98 = 3,
    WinME = 4,
    WinNT = 5,
    Win2000 = 6,
    WinXP = 7,
    Win2003 = 8,
    WinVista = 9,
    Windows7 = 10,
    Win2008 = 11,
    Win2012 = 12,
    Windows8 = 13,
    Windows81 = 14,
    Win2012R2 = 15,
    Windows10 = 16,
    Win2016 = 17,
    Win2019 = 18,
    Win2022 = 19,
    Win11 = 20,
    WinMAX = 21
}