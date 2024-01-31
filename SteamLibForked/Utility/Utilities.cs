using System.Text.RegularExpressions;

namespace SteamLib.Utility;

public static class Utilities //TODO: refactor
{
    public static int GetSuccessCode(string response)
    {
        Regex regexInt = new("\"success\":(\\d*)");
        var matchInt = regexInt.Match(response);

        if (!string.IsNullOrEmpty(matchInt.Groups[1].Value))
            return Convert.ToInt32(matchInt.Groups[1].Value);

        Regex regexBool = new("\"success\":(\\w*)");
        var matchBool = regexBool.Match(response);

        if (!string.IsNullOrEmpty(matchBool.Groups[1].Value))
            return bool.Parse(matchBool.Groups[1].Value) ? 1 : 0;
        return 0;
    }

    public static FormUrlEncodedContent ToFormContent(this IDictionary<string, string> dic) => new(dic); //TODO: убрать и зарефакторить

    public static int DivideByDecimal(this int i, decimal value) => (int) (i / value);
    public static int DivideByDouble(this int i, double value) => (int)(i / value);
   
}