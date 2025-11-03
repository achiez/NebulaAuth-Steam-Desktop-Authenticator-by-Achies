using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Services;
using System.Security.Cryptography;

namespace SteamLib.Authentication;

public static class AuthRequestHelper
{
    #region MobileGuard

    public static UpdateAuthSessionWithSteamGuardCode_Request CreateMobileCodeRequest(string code, ulong clientId,
        SteamId steamId)
    {
        return CreateMobileCodeRequest(code, clientId, steamId.Steam64.ToUlong());
    }

    public static UpdateAuthSessionWithSteamGuardCode_Request CreateMobileCodeRequest(string code, ulong clientId,
        ulong steamId)
    {
        return new UpdateAuthSessionWithSteamGuardCode_Request
        {
            Code = code,
            ClientId = clientId,
            Steamid = steamId,
            CodeType = EAuthSessionGuardType.DeviceCode
        };
    }

    #endregion

    #region EmailGuard

    public static UpdateAuthSessionWithSteamGuardCode_Request CreateEmailCodeRequest(string code, ulong clientId,
        SteamId steamId)
    {
        return CreateEmailCodeRequest(code, clientId, steamId.Steam64.ToUlong());
    }

    public static UpdateAuthSessionWithSteamGuardCode_Request CreateEmailCodeRequest(string code, ulong clientId,
        ulong steamId)
    {
        return new UpdateAuthSessionWithSteamGuardCode_Request
        {
            Code = code,
            ClientId = clientId,
            Steamid = steamId,
            CodeType = EAuthSessionGuardType.EmailCode
        };
    }

    #endregion

    #region MobileConf

    public static UpdateAuthSessionWithMobileConfirmation_Request CreateMobileConfirmationRequest(int version,
        ulong clientId, SteamId steamId, string sharedSecret)
    {
        return new UpdateAuthSessionWithMobileConfirmation_Request
        {
            Version = version,
            ClientId = clientId,
            Steamid = steamId.Steam64.ToUlong(),
            Signature = ComputeConfirmationSignature(version, clientId, steamId, sharedSecret),
            Confirm = true,
            Persistence = 1
        };
    }

    public static void EnrichMobileConfirmationWithSignature(UpdateAuthSessionWithMobileConfirmation_Request request,
        string sharedSecret)
    {
        request.Signature = ComputeConfirmationSignature(request.Version, request.ClientId,
            request.Steamid, sharedSecret);
    }

    public static byte[] ComputeConfirmationSignature(int version, ulong clientId, SteamId steamId, string sharedSecret)
    {
        return ComputeConfirmationSignature(version, clientId, steamId.Steam64.ToUlong(), sharedSecret);
    }

    public static byte[] ComputeConfirmationSignature(int version, ulong clientId, ulong steamId, string sharedSecret)
    {
        var signatureData = new byte[2 + 8 + 8];
        BitConverter.GetBytes(version).CopyTo(signatureData, 0);
        BitConverter.GetBytes(clientId).CopyTo(signatureData, 2);
        BitConverter.GetBytes(steamId).CopyTo(signatureData, 10);

        using var hmac = new HMACSHA256(Convert.FromBase64String(sharedSecret));
        return hmac.ComputeHash(signatureData);
    }

    #endregion
}