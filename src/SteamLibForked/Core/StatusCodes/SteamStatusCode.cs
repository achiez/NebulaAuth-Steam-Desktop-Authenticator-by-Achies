using AchiesUtilities.Models;
using SteamLib.Exceptions;
using SteamLib.ProtoCore.Enums;
using SteamLib.Utility;

namespace SteamLib.Core.StatusCodes;

public partial class SteamStatusCode : Enumeration<SteamStatusCode>
{
    protected SteamStatusCode(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    ///     Tries to translate status code. If no status code was found <see cref="SteamStatusCodeException" /> with
    ///     <see cref="Undefined" /> will be thrown
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="SteamStatusCodeException"></exception>
    [Obsolete("Not recommended")]
    public static SteamStatusCode Translate(string response)
    {
        int successCode;
        try
        {
            successCode = Utilities.GetSuccessCode(response);
        }
        catch (Exception ex)
        {
            throw new SteamStatusCodeException(Undefined, ex);
        }

        return Translate(successCode);
    }

    public static SteamStatusCode Translate(int statusCode)
    {
        var fromId = FromId(statusCode);
        if (fromId != null)
        {
            return fromId;
        }

        return new SteamStatusCode(statusCode, nameof(Unknown));
    }


    /// <exception cref="SteamStatusCodeException"></exception>
    [Obsolete("Not recommended")]
    public static void ValidateSuccessOrThrow(string response)
    {
        var translated = Translate(response);
        if (translated.Id != 1)
            throw new SteamStatusCodeException(translated);
    }

    /// <exception cref="SteamStatusCodeException"></exception>
    public static void ValidateSuccessOrThrow(int statusCode, StatusCodeContext context = StatusCodeContext.None)
    {
        if (statusCode == 1) return;
        var translated = Translate(statusCode);
        throw new SteamStatusCodeException(translated)
        {
            Context = context
        };
    }

    public static SteamStatusCode FromEResult(EResult result)
    {
        var r = (int) result;
        var fromId = FromId(r);
        return fromId ?? new SteamStatusCode(r, "Unknown");
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}