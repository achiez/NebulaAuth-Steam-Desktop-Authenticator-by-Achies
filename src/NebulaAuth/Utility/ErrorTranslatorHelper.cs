using CodingSeb.Localization;
using NebulaAuth.Core;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Mobile;
using SteamLib.ProtoCore.Enums;

namespace NebulaAuth.Utility;

public static class ErrorTranslatorHelper
{
    public static string TranslateLoginError(LoginError error)
    {
        var result = GetMessage("Login", error.ToString());
        return result ?? error.ToString();
    }


    public static string TranslateEResult(EResult eResult)
    {
        var result = GetMessage("EResult", eResult.ToString());
        return result ?? eResult.ToString();
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