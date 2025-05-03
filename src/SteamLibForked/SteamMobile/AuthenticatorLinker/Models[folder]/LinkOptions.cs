using Microsoft.Extensions.Logging;
using SteamLib.Core.Interfaces;

namespace SteamLib.SteamMobile.AuthenticatorLinker;

public class LinkOptions
{
    public ILoginConsumer LoginConsumer { get; }
    public HttpClient HttpClient { get; }
    public IEmailProvider? EmailProvider { get; }
    public ISmsCodeProvider? SmsCodeProvider { get; }
    public ICaptchaResolver? CaptchaResolver { get; }
    public IPhoneNumberProvider? PhoneNumberProvider { get; }
    public Action<MobileDataExtended>? BackupHandler { get; }
    public ILogger? Logger { get; }

    public LinkOptions(HttpClient httpClient, ILoginConsumer loginConsumer, ISmsCodeProvider? smsCodeProvider,
        ICaptchaResolver? captchaResolver, IEmailProvider? emailProvider, IPhoneNumberProvider? phoneNumberProvider,
        Action<MobileDataExtended>? backupHandler = null,
        ILogger? logger = null)
    {
        EmailProvider = emailProvider;
        SmsCodeProvider = smsCodeProvider;
        CaptchaResolver = captchaResolver;
        HttpClient = httpClient;
        LoginConsumer = loginConsumer;
        BackupHandler = backupHandler;
        PhoneNumberProvider = phoneNumberProvider;
        Logger = logger;
    }
}