namespace SteamLib.Exceptions;

public class UnknownSteamDomainException : Exception
{
    public string? Domain { get; }
    public UnknownSteamDomainException(string? domain)
    {
        Domain = domain;
    }

    public UnknownSteamDomainException(string? domain, string? message) : base(message)
    {
        Domain = domain;
    }

    public UnknownSteamDomainException(string? domain, string? message, Exception? inner) : base(message, inner)
    {
        Domain = domain;
    }

    public static UnknownSteamDomainException Create(string? domain, Exception? inner = null)
    {
        return new(domain, $"Unknown Steam domain: {domain}", inner);
    }   
}