namespace SteamLib.Exceptions.Mobile;


public enum AuthenticatorLinkerError
{
    NoEmailProvider,
    NoSmsCodeProvider,
    PhoneAlreadyAttached,
    InvalidPhoneNumber,
    CantAttachPhone,
    CantConfirmAttachingEmail,
    CantSendSms,
    AuthenticatorPresent,
    BadConfirmationCode,
    UnableToGenerateCorrectCodes,
    InvalidStateWithStatus2,
    GeneralFailure,

}
public class AuthenticatorLinkerException : Exception
{
    public AuthenticatorLinkerError Error { get; }
    public bool OnFinalization { get; init; }
    public AuthenticatorLinkerException(AuthenticatorLinkerError error) : base($"Linking failed due to error: {error}")
    {
        Error = error;
    }

    public AuthenticatorLinkerException(int code)
        : base($"Linking failed due to error: {AuthenticatorLinkerError.GeneralFailure} ({code})")
    {
        Error = AuthenticatorLinkerError.GeneralFailure;
    }

    public AuthenticatorLinkerException(string message) : base(message)
    {
        Error = AuthenticatorLinkerError.GeneralFailure;
    }

    public AuthenticatorLinkerException(){}

}