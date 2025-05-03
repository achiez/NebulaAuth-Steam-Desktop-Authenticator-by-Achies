namespace SteamLib.Core.Interfaces;

public interface IPhoneNumberProvider
{
    public Task<long?> GetPhoneNumber(ILoginConsumer caller);
}