using System.Collections.ObjectModel;
using AchiesUtilities.Models;
using SteamLib.Exceptions;
using SteamLib.Utility;

namespace SteamLib.Core.StatusCodes;

public partial class SteamStatusCode : Enumeration
{
    public static IReadOnlyDictionary<int, SteamStatusCode> StatusCodes { get; }

    static SteamStatusCode()
    {
        StatusCodes =
            new ReadOnlyDictionary<int, SteamStatusCode>(GetAll<SteamStatusCode>()
                .ToDictionary(ssc => ssc.Id, ssc => ssc));
    }

    protected SteamStatusCode(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    ///     Tries to translate status code. If no status code was found <see cref="SteamStatusCodeException" /> with
    ///     <see cref="Undefined" /> will be thrown
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <param name="isUniversal"></param>
    /// <returns>
    ///     <see cref="SteamStatusCode" /> if value is universal. <typeparamref name="T" /> if value is
    ///     <typeparamref name="T" /> specific
    /// </returns>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static SteamStatusCode Translate<T>(string response, out bool isUniversal) where T : SteamStatusCode
    {
        int statusCode;
        try
        {
            statusCode = Utilities.GetSuccessCode(response);
        }
        catch
        {
            throw new SteamStatusCodeException(Undefined, response);
        }

        return Translate<T>(statusCode, out isUniversal);
    }


    /// <summary>
    ///     Translate <paramref name="statusCode" /> to specific <see cref="T" /> or <see cref="SteamStatusCode" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCode"></param>
    /// <param name="isUniversal"></param>
    /// <returns>
    ///     <see cref="SteamStatusCode" /> if value is universal. <typeparamref name="T" /> if value is
    ///     <typeparamref name="T" /> specific
    /// </returns>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static SteamStatusCode Translate<T>(int statusCode, out bool isUniversal) where T : SteamStatusCode
    {
        if (statusCode < 2)
        {
            var status = StatusCodes[statusCode];
            isUniversal = true;
            return status;
        }

        var allSpecific = GetAll<T>();
        var translated = allSpecific.FirstOrDefault(s => s.Id == statusCode);

        isUniversal = translated == null;

        if (translated != null)
        {
            return translated;
        }

        if (translated == null && StatusCodes.TryGetValue(statusCode, out var universal))
        {
            return universal;
        }

        return translated ?? new SteamStatusCode(statusCode, nameof(Unknown));
    }


    public override bool Equals(object? obj)
    {
        if (obj is not SteamStatusCode translated)
            return false;

        if (Id < 2)
        {
            return translated.Id == Id;
        }

        return base.Equals(translated);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


    /// <summary>
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static void ValidateSuccessOrThrow<T>(string response) where T : SteamStatusCode
    {
        var translated = Translate<T>(response, out _);
        if (translated.Id != 1)
            throw new SteamStatusCodeException(translated, response);
    }

    /// <summary>
    ///     Same as <see cref="ValidateSuccessOrThrow{T}(string)" /> with <see cref="SteamStatusCode" /> generic parameter
    ///     <br />
    ///     <b>(Used in case when steam status code is not defined for this operation)</b>
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static void ValidateSuccessOrThrow(string response)
    {
        ValidateSuccessOrThrow<SteamStatusCode>(response);
    }

    /// <summary>
    /// </summary>
    /// <param name="statusCode"></param>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static void ValidateSuccessOrThrow<T>(int statusCode) where T : SteamStatusCode
    {
        if (statusCode == 1) return;
        var translated = Translate<T>(statusCode, out _);
        throw new SteamStatusCodeException(translated, statusCode.ToString());
    }


    /// <summary>
    ///     Same as <see cref="ValidateSuccessOrThrow{T}(int)" /> with <see cref="SteamStatusCode" /> generic parameter <br />
    ///     <b>(Used in case when steam status code is not defined for this operation)</b>
    /// </summary>
    /// <param name="statusCode"></param>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static void ValidateSuccessOrThrow(int statusCode)
    {
        ValidateSuccessOrThrow<SteamStatusCode>(statusCode);
    }

    /// <summary>
    ///     Validates that status code is <see cref="Ok" /> or <typeparamref name="T" /> specific
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <param name="isUniversal"></param>
    /// <exception cref="SteamStatusCodeException"></exception>
    public static SteamStatusCode TranslateOrThrow<T>(string response, out bool isUniversal) where T : SteamStatusCode
    {
        var translated = Translate<T>(response, out isUniversal);
        if (translated.Id < 1)
            throw new SteamStatusCodeException(translated, response);
        return translated;
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}