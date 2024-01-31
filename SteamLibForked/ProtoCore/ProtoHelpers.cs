using System.Diagnostics.CodeAnalysis;
using ProtoBuf;
using SteamLib.ProtoCore.Enums;
using SteamLib.ProtoCore.Exceptions;
using SteamLib.ProtoCore.Interfaces;

namespace SteamLib.ProtoCore;

public static class ProtoHelpers
{
    public static byte[] ProtoToBytes(IProtoMsg obj)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, obj);
        return stream.ToArray();
    }

    [return: NotNullIfNotNull("obj")]
    public static string? ProtoToString(IProtoMsg? obj)
    {
        return obj == null ? null : Convert.ToBase64String(ProtoToBytes(obj));
    }

    public static EResult GetEResult(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-eresult", out var val))
        {
            var eResultInt = Convert.ToInt32(val.Single());

            if (Enum.IsDefined(typeof(EResult), eResultInt) == false)
                throw new UnknownEResultException(eResultInt);

            return (EResult)eResultInt;
        }

        return EResult.Invalid;
    }
}