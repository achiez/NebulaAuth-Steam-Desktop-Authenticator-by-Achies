using SteamLib.Core.Interfaces;

namespace SteamLib.Login.Default;

internal class NullLoginConsumer : ILoginConsumer
{
    public string FriendlyName { get; } = "null";
}