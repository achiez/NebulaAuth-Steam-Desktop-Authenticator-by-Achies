using SteamLib.Abstractions;

namespace SteamLib.Authentication;

public class StaticLoginConsumer : ILoginConsumer
{
    public string FriendlyName => nameof(StaticLoginConsumer);
    public static StaticLoginConsumer Instance { get; } = new();
}