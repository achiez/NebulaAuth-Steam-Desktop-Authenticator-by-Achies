namespace SteamLib.Exceptions.General;

public class UnsupportedResponseException : Exception
{
    public string Response { get; }

    public UnsupportedResponseException(string response) : base("Request failed with unsupported response")
    {
        Response = response;
    }

    public UnsupportedResponseException(string response, Exception? inner) : base(
        "Request failed with unsupported response", inner)
    {
        Response = response;
    }

    public UnsupportedResponseException(string response, string? message, Exception? inner = null) : base(message,
        inner)
    {
        Response = response;
    }
}