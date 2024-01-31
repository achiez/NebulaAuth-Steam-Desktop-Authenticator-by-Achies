using Microsoft.Extensions.Logging;
using SteamLib.Core.Interfaces;
using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Services;

namespace SteamLib.Authentication.LoginV2;

public class LoginV2ExecutorOptions
{
    public ILoginConsumer Consumer { get; }
    public HttpClient HttpClient { get; }
    public ILogger? Logger { get; init; }
    public IEmailProvider? EmailAuthProvider { get; init; }
    public ISteamGuardProvider? SteamGuardProvider { get; init; }
    public DeviceDetails? DeviceDetails { get; init; }

    /// <summary>
    /// Gets or sets the website id that the login will be performed for.
    /// Known values are "Unknown", "Client", "Mobile", "Website", "Store", "Community", "Partner", "SteamStats".
    /// Default value will be set to "Community"
    /// </summary>
    /// <value>The website id.</value>
    public string? WebsiteId { get; init; }
    public LoginV2ExecutorOptions(ILoginConsumer consumer, HttpClient httpClient)
    {
        Consumer = consumer;
        HttpClient = httpClient;
    }

 

    public string GetWebsiteIdOrDefault()
    {
        return WebsiteId ?? "Community";
    }


    public static DeviceDetails GetMobileDefaultDevice() //FORTEST
    {
        return new DeviceDetails("Pixel 6 Pro", EAuthTokenPlatformType.MobileApp, -500, 528);
    }
}