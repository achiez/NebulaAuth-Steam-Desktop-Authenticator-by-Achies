namespace SteamLib.Core.Interfaces;

public interface ICaptchaResolver
{
    public Task<string> Resolve(Uri imageUrl, HttpClient client);
}