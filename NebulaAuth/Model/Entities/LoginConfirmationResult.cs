using System.Diagnostics.CodeAnalysis;

namespace NebulaAuth.Model.Entities;

public class LoginConfirmationResult
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success { get; set; }
    public string IP { get; set; } = null!;
    public string Country { get; set; } = null!;
    public LoginConfirmationError? Error { get; set; }

    
}


public enum LoginConfirmationError
{
    NoRequests,
    MoreThanOneRequest
}