namespace SteamLib.Core.Interfaces;

public interface ISmsCodeProvider
{
    public Task<int> GetSmsCode(ILoginConsumer caller, long? phoneNumber, string? phoneHint);
}