using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Exceptions;
using SteamLib.ProtoCore.Interfaces;

namespace SteamLib.ProtoCore;

public static class ProtoHttpClientExtension
{
    private static async Task<ProtoResponse<TProtoResponse>> SendProtoWithResponse<TProtoResponse>(
        this HttpClient client, Uri uri, HttpMethod method, string? protoMsg, CancellationToken cancellationToken)
        where TProtoResponse : class, IProtoMsg
    {
        var resp = await SendProto(client, uri, method, protoMsg, cancellationToken);
        return await ProtoResponse<TProtoResponse>.FromHttpResponseAsync(resp, cancellationToken);
    }

    private static async Task<EResult> SendProtoWithEResult(HttpClient client, Uri uri, HttpMethod method,
        string? protoMsg,
        CancellationToken cancellationToken)
    {
        var resp = await SendProto(client, uri, method, protoMsg, cancellationToken);
        resp.EnsureSuccessStatusCode();
        return ProtoHelpers.GetEResult(resp);
    }


    public static Task<HttpResponseMessage> SendProto(this HttpClient client, Uri uri, HttpMethod method,
        IProtoMsg? protoMsg, CancellationToken cancellationToken = default)
    {
        return client.SendProto(uri, method, ProtoHelpers.ProtoToString(protoMsg), cancellationToken);
    }

    public static Task<HttpResponseMessage> SendProto(this HttpClient client, Uri uri, HttpMethod method,
        string? protoMsg, CancellationToken cancellationToken = default)
    {
        HttpContent? content = null;

        protoMsg ??= string.Empty;

        if (method.Equals(HttpMethod.Get))
        {
            var uriStr = uri.ToString();
            var uriWithQuery = uriStr.Contains('?')
                ? uriStr + "&input_protobuf_encoded=" + protoMsg
                : uriStr + "?input_protobuf_encoded=" + protoMsg;
            uri = new Uri(uriWithQuery);
        }
        else
        {
            var cont = new KeyValuePair<string, string>[]
            {
                new("input_protobuf_encoded", protoMsg)
            };
            content = new FormUrlEncodedContent(cont);
        }

        var req = new HttpRequestMessage(method, uri)
        {
            Content = content
        };

        return client.SendAsync(req, cancellationToken);
    }

    public static void EnsureSuccessEResult(this EResult eResult)
    {
        if (eResult != EResult.OK)
        {
            throw new EResultException(eResult);
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="EResultException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    public static T GetResponseEnsureSuccess<T>(this ProtoResponse<T> response) where T : class, IProtoMsg
    {
        if (response.Result != EResult.OK)
        {
            throw new EResultException(response.Result);
        }

        return response.GetResponse();
    }

    #region Post

    #region ProtoResp

    public static Task<ProtoResponse<TProtoResponse>> PostProtoMsg<TProtoResponse>(this HttpClient client, Uri uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithResponse<TProtoResponse>(client, uri, HttpMethod.Post, str, cancellationToken);
    }

    public static Task<ProtoResponse<TProtoResponse>> PostProtoMsg<TProtoResponse>(this HttpClient client, string uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithResponse<TProtoResponse>(client, new Uri(uri), HttpMethod.Post, str, cancellationToken);
    }

    #endregion

    #region ProtoConverted

    public static async Task<TProtoResponse> PostProto<TProtoResponse>(this HttpClient client, Uri uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithResponse<TProtoResponse>(client, uri, HttpMethod.Post, str, cancellationToken);
        return res.GetResponseEnsureSuccess();
    }

    public static async Task<TProtoResponse> PostProto<TProtoResponse>(this HttpClient client, string uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithResponse<TProtoResponse>(client, new Uri(uri), HttpMethod.Post, str,
            cancellationToken);
        return res.GetResponseEnsureSuccess();
    }

    #endregion

    #region EResult

    public static Task<EResult> PostProto(this HttpClient client, Uri uri, IProtoMsg request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithEResult(client, uri, HttpMethod.Post, str, cancellationToken);
    }

    public static Task<EResult> PostProto(this HttpClient client, string uri, IProtoMsg request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithEResult(client, new Uri(uri), HttpMethod.Post, str, cancellationToken);
    }


    public static async Task PostProtoEnsureSuccess(this HttpClient client, Uri uri, IProtoMsg request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithEResult(client, uri, HttpMethod.Post, str, cancellationToken);
        res.EnsureSuccessEResult();
    }

    public static async Task PostProtoEnsureSuccess(this HttpClient client, string uri, IProtoMsg request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithEResult(client, new Uri(uri), HttpMethod.Post, str, cancellationToken);
        res.EnsureSuccessEResult();
    }

    #endregion

    #endregion

    #region Get

    #region ProtoResp

    public static Task<ProtoResponse<TProtoResponse>> GetProtoMsg<TProtoResponse>(this HttpClient client, Uri uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithResponse<TProtoResponse>(client, uri, HttpMethod.Get, str, cancellationToken);
    }

    public static Task<ProtoResponse<TProtoResponse>> GetProtoMsg<TProtoResponse>(this HttpClient client, string uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithResponse<TProtoResponse>(client, new Uri(uri), HttpMethod.Get, str, cancellationToken);
    }

    #endregion

    #region ProtoConverted

    public static async Task<TProtoResponse> GetProto<TProtoResponse>(this HttpClient client, Uri uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithResponse<TProtoResponse>(client, uri, HttpMethod.Get, str, cancellationToken);
        res.Result.EnsureSuccessEResult();

        return res.ResponseMsg ?? throw new NullReferenceException("ProtoMsg in response was null");
    }

    public static async Task<TProtoResponse> GetProto<TProtoResponse>(this HttpClient client, string uri,
        IProtoMsg? request,
        CancellationToken cancellationToken = default) where TProtoResponse : class, IProtoMsg
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithResponse<TProtoResponse>(client, new Uri(uri), HttpMethod.Get, str,
            cancellationToken);
        res.Result.EnsureSuccessEResult();
        return res.ResponseMsg ?? throw new NullReferenceException("ProtoMsg in response was null");
    }

    #endregion

    #region EResult

    public static Task<EResult> GetProto(this HttpClient client, Uri uri, IProtoMsg? request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithEResult(client, uri, HttpMethod.Get, str, cancellationToken);
    }

    public static Task<EResult> GetProto(this HttpClient client, string uri, IProtoMsg? request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        return SendProtoWithEResult(client, new Uri(uri), HttpMethod.Get, str, cancellationToken);
    }

    public static async Task GetProtoEnsureSuccess(this HttpClient client, Uri uri, IProtoMsg? request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithEResult(client, uri, HttpMethod.Get, str, cancellationToken);
        res.EnsureSuccessEResult();
    }

    public static async Task GetProtoEnsureSuccess(this HttpClient client, string uri, IProtoMsg? request,
        CancellationToken cancellationToken = default)
    {
        var str = ProtoHelpers.ProtoToString(request);
        var res = await SendProtoWithEResult(client, new Uri(uri), HttpMethod.Get, str, cancellationToken);
        res.EnsureSuccessEResult();
    }

    #endregion

    #endregion
}