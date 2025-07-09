namespace SteamLib.Abstractions;

public enum EmailConfirmationType
{
    AttachPhoneAuthenticator
}

public interface IEmailProvider : IAuthProvider
{
    public int RetryCount { get; }

    public ValueTask<string> GetAddAuthenticatorCode(ILoginConsumer caller,
        CancellationToken cancellationToken = default);

    public Task ConfirmEmailLink(ILoginConsumer caller, EmailConfirmationType confirmationType,
        CancellationToken cancellationToken = default);
}