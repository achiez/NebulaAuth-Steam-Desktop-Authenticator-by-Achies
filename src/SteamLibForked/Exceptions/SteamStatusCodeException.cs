using SteamLib.Core.StatusCodes;

namespace SteamLib.Exceptions;

public class SteamStatusCodeException : Exception
{
    public SteamStatusCode StatusCode { get; }
    public StatusCodeContext Context { get; init; } = StatusCodeContext.None;
    public bool IsUnknownOrUndefined => StatusCode.Id == 0 || StatusCode.Name.Equals(nameof(SteamStatusCode.Unknown));

    public SteamStatusCodeException(SteamStatusCode statusCode, Exception? innerException = null)
        : base($"Steam returned not successful status code {statusCode}", innerException)
    {
        StatusCode = statusCode;
    }

    public SteamStatusCodeException(SteamStatusCode statusCode, string? message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}