namespace SteamLib.Core.Interfaces;

public enum EmailConfirmationType
{
    AttachPhoneAuthenticator
}

public interface IEmailProvider
{
    public int MaxRetryCount { get; }
    public Task<string> GetEmailAuthCode(ILoginConsumer caller);
    public Task<string> GetAddAuthenticatorCode(ILoginConsumer caller);

    public Task ConfirmEmailLink(ILoginConsumer caller, EmailConfirmationType confirmationType);
}