using CodingSeb.Localization;
using NebulaAuth.Core;
using SteamLib.Core.StatusCodes;
using SteamLib.Exceptions.Mobile;
using SteamLibForked.Exceptions.Authorization;

namespace NebulaAuth.Utility;

public static class ErrorTranslatorHelper
{
    public static string TranslateLoginError(LoginError error)
    {
        var result = GetMessage("Login", error.ToString());
        return result ?? error.ToString();
    }

    public static string TranslateSteamStatusCode(SteamStatusCode statusCode)
    {
        var result = GetMessage("EResult", statusCode.Name);
        return result ?? statusCode.ToString();
    }

    public static string TranslateLinkerError(AuthenticatorLinkerError error)
    {
        return GetMessage("AuthenticatorLinkerError", error.ToString()) ?? error.ToString();
    }

    public static string? GetMessage(string path, string name)
    {
        var fullPath = LocManager.CODE_BEHIND_PATH_PART + "." + "ErrorTranslator" + "." + path + "." + name;
        var message = Loc.Tr(fullPath, "|ABSENT|");
        if (message == "|ABSENT|") return null;
        return message;
    }
}