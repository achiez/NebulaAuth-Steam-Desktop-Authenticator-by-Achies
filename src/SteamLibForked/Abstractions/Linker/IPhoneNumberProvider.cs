using SteamLib.Abstractions;

namespace SteamLibForked.Abstractions.Linker;

public interface IPhoneNumberProvider
{
    public Task<long?> GetPhoneNumber(ILoginConsumer caller);
}