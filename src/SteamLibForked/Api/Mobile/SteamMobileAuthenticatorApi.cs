using System.Net;
using SteamLib.Core;
using SteamLib.Exceptions;
using SteamLib.ProtoCore;
using SteamLib.ProtoCore.Services;

namespace SteamLib.Api.Mobile;

public static class SteamMobileAuthenticatorApi
{
    public static async Task<GetAuthSessionsForAccount_Response> GetAuthSessionsForAccount(HttpClient client,
        string accessToken, CancellationToken cancellationToken = default)
    {
        var req = SteamConstants.STEAM_API + "IAuthenticationService/GetAuthSessionsForAccount/v1?access_token=" +
                  accessToken;

        try
        {
            return await client.GetProto<GetAuthSessionsForAccount_Response>(req, new EmptyMessage(),
                cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SessionInvalidException(SessionInvalidException.GOT_401_MSG, ex);
        }
    }

    public static async Task<GetAuthSessionInfo_Response> GetAuthSessionInfo(HttpClient client, string accessToken,
        ulong clientId, CancellationToken cancellationToken = default)
    {
        var req = SteamConstants.STEAM_API + "IAuthenticationService/GetAuthSessionInfo/v1?access_token=" + accessToken;
        var reqData = new GetAuthSessionInfo_Request
        {
            ClientId = clientId
        };
        try
        {
            return await client.PostProto<GetAuthSessionInfo_Response>(req, reqData, cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SessionInvalidException(SessionInvalidException.GOT_401_MSG, ex);
        }
    }


    public static async Task<bool> UpdateAuthSessionStatus(HttpClient client, string accessToken, string sharedSecret,
        UpdateAuthSessionWithMobileConfirmation_Request request)
    {
        var req = SteamConstants.STEAM_API +
                  "IAuthenticationService/UpdateAuthSessionWithMobileConfirmation/v1?access_token=" + accessToken;

        request.ComputeSignature(sharedSecret);
        await client.PostProtoEnsureSuccess(req, request);
        return true;
    }
}