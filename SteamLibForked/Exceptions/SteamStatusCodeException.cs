using SteamLib.Core.StatusCodes;

namespace SteamLib.Exceptions;

public class SteamStatusCodeException : Exception
{
    public SteamStatusCode StatusCode { get; }
    public string? Response { get; }

    public SteamStatusCodeException(SteamStatusCode statusCode, string? response, Exception? innerException = null)
        : base($"Steam return not successful status code {statusCode}", innerException)
    {
        StatusCode = statusCode;
        Response = response;
    }

    public SteamStatusCodeException(string message, SteamStatusCode statusCode, string? response = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Response = response;
    }
}