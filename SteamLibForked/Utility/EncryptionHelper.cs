using System.Security.Cryptography;
using System.Text;

namespace SteamLib.Utility;

public static class EncryptionHelper
{
    public static byte[] HexStringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }

    public static string ToBase64EncryptedPassword(string keyExp, string keyMod, string password)
    {
        // RSA Encryption.
        RSACryptoServiceProvider rsa = new();
        RSAParameters rsaParameters = new()
        {
            Exponent = HexStringToByteArray(keyExp),
            Modulus = HexStringToByteArray(keyMod)
        };
        rsa.ImportParameters(rsaParameters);

        // Encrypt the password and convert it.
        byte[] bytePassword = Encoding.ASCII.GetBytes(password);
        byte[] encodedPassword = rsa.Encrypt(bytePassword, false);
        return Convert.ToBase64String(encodedPassword);
    }

    public static string GenerateConfirmationHash(long time, string identitySecret, string tag = "conf")
    {
        var hashedBytes = GenerateConfirmationHashBytes(time, identitySecret, tag);
        var encodedData = Convert.ToBase64String(hashedBytes, Base64FormattingOptions.None);
        var hash = encodedData;
        return hash;
    }
    public static byte[] GenerateConfirmationHashBytes(long time, string identitySecret, string tag = "conf")
    {
        byte[] decode = Convert.FromBase64String(identitySecret);
        int n2;
        if (tag.Length > 32)
        {
            n2 = 8 + 32;
        }
        else
        {
            n2 = 8 + tag.Length;
        }
        byte[] array = new byte[n2];
        int n3 = 8;
        while (true)
        {
            int n4 = n3 - 1;
            if (n3 <= 0)
            {
                break;
            }
            array[n4] = (byte)time;
            time >>= 8;
            n3 = n4;
        }
        Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);

        var hmacGenerator = new HMACSHA1
        {
            Key = decode
        };
        var hashedData = hmacGenerator.ComputeHash(array);
        return hashedData;
    }
}