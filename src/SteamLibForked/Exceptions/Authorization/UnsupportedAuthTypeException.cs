using SteamLib.ProtoCore.Enums;

namespace SteamLib.Exceptions.Authorization;

public class UnsupportedAuthTypeException : Exception
{
    public EAuthSessionGuardType[] AllowedGuardTypes { get; }

    public UnsupportedAuthTypeException(EAuthSessionGuardType[] allowedGuardTypes)
        : base($"No provider found for required guard types. Allowed types: {string.Join(", ", allowedGuardTypes)}")
    {
        AllowedGuardTypes = allowedGuardTypes;
    }

    public UnsupportedAuthTypeException(string message, EAuthSessionGuardType[] allowedGuardTypes)
        : base(message)
    {
        AllowedGuardTypes = allowedGuardTypes;
    }
}