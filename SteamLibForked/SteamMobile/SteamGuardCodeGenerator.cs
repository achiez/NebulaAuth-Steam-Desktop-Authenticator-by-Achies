using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SteamLib.Core.Interfaces;

namespace SteamLib.SteamMobile;

public class SteamGuardCodeGenerator : ISteamGuardProvider
{
    private static readonly byte[] SteamGuardCodeTranslations =
        {50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89};

    public string SharedSecret { get; }

    int ISteamGuardProvider.MaxRetryCount => ProviderMaxRetryCount;
    public int ProviderMaxRetryCount { get; set; }

    public SteamGuardCodeGenerator(string sharedSecret)
    {
        SharedSecret = sharedSecret;
    }


    public ValueTask<string> GetSteamGuardCode(ILoginConsumer caller)
    {
        return ValueTask.FromResult(GenerateCode());
    }

    public string GenerateCode()
    {
        return GenerateCode(SharedSecret);
    }


    public static string GenerateCode(string sharedSecret)
    {
        return GenerateCode(sharedSecret, TimeAligner.GetSteamTime());
    }

    public static string GenerateCode(string sharedSecret, long time)
    {
        var sharedSecretUnescaped = Regex.Unescape(sharedSecret);
        var sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
        return GenerateCode(sharedSecretArray, time);
    }

    public static string GenerateCode(byte[] sharedSecret, long time)
    {
        var timeArray = new byte[8];

        time /= 30L;

        for (var i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte) time;
            time >>= 8;
        }

        using HMACSHA1 hmacGenerator = new();
        hmacGenerator.Key = sharedSecret;
        var hashedData = hmacGenerator.ComputeHash(timeArray);
        var codeArray = new byte[5];

        var b = (byte) (hashedData[19] & 0xF);
        var codePoint = ((hashedData[b] & 0x7F) << 24) | ((hashedData[b + 1] & 0xFF) << 16) |
                        ((hashedData[b + 2] & 0xFF) << 8) | (hashedData[b + 3] & 0xFF);

        for (var i = 0; i < 5; ++i)
        {
            codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
            codePoint /= SteamGuardCodeTranslations.Length;
        }

        return Encoding.UTF8.GetString(codeArray);
    }
}