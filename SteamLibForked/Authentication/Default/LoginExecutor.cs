using Microsoft.Extensions.Logging;
using SteamLib.Authentication;
using SteamLib.Core.Interfaces;
using SteamLib.Exceptions;

namespace SteamLib.Login.Default;

[Obsolete("Method removed from Steam")]
public class LoginExecutor
{
    public static ILoginConsumer NullConsumer { get; } = new NullLoginConsumer();
    public ILoginConsumer Caller { get; }
    public HttpClient HttpClient { get; }
    public ILogger? Logger { get; init; }
    public IEmailProvider? EmailAuthProvider { get; init; }
    public ICaptchaResolver? CaptchaResolver { get; init; }
    public ISteamGuardProvider? SteamGuardProvider { get; init; }

    private LoginExecutor(LoginExecutorOptions options)
    {
        Caller = options.Consumer;
        HttpClient = options.HttpClient;
        Logger = options.Logger;
        EmailAuthProvider = options.EmailAuthProvider;
        SteamGuardProvider = options.SteamGuardProvider;
        CaptchaResolver = options.CaptchaResolver;
    }


    public static async Task<TransferParameters> DoLogin(LoginExecutorOptions options, string username, string password,
        CancellationToken cancellationToken = default) //TODO: logs
    {
        var executor = new LoginExecutor(options);
        var client = executor.HttpClient;

        LoginStage loginStage = new GetRsaStage(username);
        loginStage = await ((GetRsaStage) loginStage).Proceed(client, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (loginStage is ReadyToLoginStage rdyToLoginStage)
        {
            loginStage = await rdyToLoginStage.Proceed(client, password, string.Empty, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var twoFactorTry = 0;
        var emailTry = 0;
        while (loginStage.GetType() != typeof(LoginErrorStage) && loginStage is not LoginSuccessStage)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (loginStage)
            {
                case CaptchaNeededStage captchaNeededStage:
                {
                    if (executor.CaptchaResolver == null) throw new LoginException(LoginError.CaptchaRequired);
                    var captchaText = await executor.CaptchaResolver.Resolve(captchaNeededStage.CaptchaImage, client);
                    loginStage = await captchaNeededStage.Proceed(client, captchaText, cancellationToken);
                    break;
                }
                case EmailAuthStage emailAuthStage:
                {
                    if (executor.EmailAuthProvider == null) throw new LoginException(LoginError.EmailAuthRequired);
                    if (emailTry > executor.EmailAuthProvider.MaxRetryCount)
                        throw new LoginException(LoginError.InvalidEmailAuthCode);
                    var code = await executor.EmailAuthProvider.GetEmailAuthCode(executor.Caller);
                    loginStage = await emailAuthStage.Proceed(client, code, cancellationToken);
                    emailTry++;
                    break;
                }
                case TwoFactorStage twoFactorStage:
                {
                    if (executor.SteamGuardProvider == null) throw new LoginException(LoginError.SteamGuardRequired);
                    if (twoFactorTry > executor.SteamGuardProvider.MaxRetryCount)
                        throw new LoginException(LoginError.InvalidTwoFactorCode);
                    var twoFactor = await executor.SteamGuardProvider.GetSteamGuardCode(executor.Caller);
                    loginStage = await twoFactorStage.Proceed(client, twoFactor, cancellationToken);
                    twoFactorTry++;
                    break;
                }
                case ReadyToLoginStage readyToLoginStage: //When captcha proceeded, stage goes there
                {
                    loginStage = await readyToLoginStage.Proceed(client, password, string.Empty, cancellationToken);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginStage));
            }
        }

        if (loginStage is LoginErrorStage error)
        {
            var content = await error.Response.Content.ReadAsStringAsync(cancellationToken);
            if (content.Contains("The account name or password that you have entered is incorrect"))
            {
                throw new LoginException(LoginError.InvalidCredentials);
            }

            throw new LoginException(content);
        }


        if (loginStage is not LoginSuccessStage successStage)
        {
            throw new InvalidOperationException("Unexpected login stage at this point. " + loginStage.GetType())
            {
                Data = {{"stage", loginStage}}
            };
        }

        return successStage.TransferParameters;
    }
}