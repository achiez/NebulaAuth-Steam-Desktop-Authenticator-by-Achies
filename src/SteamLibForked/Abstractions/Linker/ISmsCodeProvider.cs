using SteamLib.Abstractions;

namespace SteamLibForked.Abstractions.Linker;

public interface ISmsCodeProvider
{
    public Task<int> GetSmsCode(ILoginConsumer caller, long? phoneNumber, string? phoneHint);
}