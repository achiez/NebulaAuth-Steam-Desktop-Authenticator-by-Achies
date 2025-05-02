using System.Diagnostics.CodeAnalysis;

namespace SteamLib.SteamMobile.AuthenticatorLinker;

public class LinkResult
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success { get; }

    public LinkError? Error { get; }
    public int? Code { get; }

    public LinkResult()
    {
        Success = true;
    }

    public LinkResult(LinkError error)
    {
        Success = false;
        Error = error;
    }

    public LinkResult(int code)
    {
        Success = false;
        Code = code;
        Error = LinkError.GeneralFailure;
    }
}

public enum LinkError
{
    GeneralFailure,
    BadConfirmationCode,
    UnableToGenerateCorrectCodes
}