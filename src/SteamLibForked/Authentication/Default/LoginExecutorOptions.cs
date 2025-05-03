using Microsoft.Extensions.Logging;
using SteamLib.Core.Interfaces;

namespace SteamLib.Login.Default;

public class LoginExecutorOptions
{
    public ILoginConsumer Consumer { get; }
    public HttpClient HttpClient { get; }
    public ILogger? Logger { get; init; }
    public IEmailProvider? EmailAuthProvider { get; init; }
    public ICaptchaResolver? CaptchaResolver { get; init; }
    public ISteamGuardProvider? SteamGuardProvider { get; init; }

    public LoginExecutorOptions(ILoginConsumer consumer, HttpClient httpClient)
    {
        Consumer = consumer;
        HttpClient = httpClient;
    }
}