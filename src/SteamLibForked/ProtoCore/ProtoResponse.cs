using ProtoBuf;
using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Interfaces;

namespace SteamLib.ProtoCore;

public class ProtoResponse
{
    public EResult Result { get; }
    public IProtoMsg? ProtoObject { get; protected set; }


    protected ProtoResponse(EResult result, IProtoMsg? protoObject)
    {
        Result = result;
        ProtoObject = protoObject;
    }
}

public class ProtoResponse<TProtoResponse> : ProtoResponse
    where TProtoResponse : IProtoMsg
{
    public TProtoResponse? ResponseMsg
    {
        get => ProtoObject == null ? default : (TProtoResponse)ProtoObject;
        set => ProtoObject = value;
    }

    public ProtoResponse(EResult result, TProtoResponse? protoObject) : base(result, protoObject)
    {
    }

    public static ProtoResponse<TProtoResponse> FromHttpResponse(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var eResult = EResult.Invalid;
        if (response.Headers.TryGetValues("x-eresult", out var val))
        {
            var eResultInt = Convert.ToInt32(val.Single());
            eResult = (EResult)eResultInt;
        }

        var msg = default(TProtoResponse);
        Stream? stream = null;
        try
        {
            stream = response.Content.ReadAsStream();
            msg = Serializer.Deserialize<TProtoResponse>(stream);
        }
        catch
        {
            //Ignored
        }
        finally
        {
            stream?.Dispose();
        }

        return new ProtoResponse<TProtoResponse>(eResult, msg);
    }

    public static async Task<ProtoResponse<TProtoResponse>> FromHttpResponseAsync(HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        response.EnsureSuccessStatusCode();
        var eResult = EResult.Invalid;
        if (response.Headers.TryGetValues("x-eresult", out var val))
        {
            var eResultInt = Convert.ToInt32(val.Single());
            eResult = (EResult)eResultInt;
        }

        var msg = default(TProtoResponse);
        Stream? stream = null;
        try
        {
            stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            msg = Serializer.Deserialize<TProtoResponse>(stream);
        }
        catch
        {
            //Ignored
        }
        finally
        {
            if (stream != null) await stream.DisposeAsync();
        }


        return new ProtoResponse<TProtoResponse>(eResult, msg);
    }

    /// <summary>
    ///     Ensures that <see cref="ResponseMsg" /> is not <see langword="null" /> and returns it otherwise throws
    ///     <see cref="NullReferenceException" />.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public TProtoResponse GetResponse()
    {
        if (ResponseMsg == null)
            throw new NullReferenceException("ResponseMsg in response was null");
        return ResponseMsg;
    }
}