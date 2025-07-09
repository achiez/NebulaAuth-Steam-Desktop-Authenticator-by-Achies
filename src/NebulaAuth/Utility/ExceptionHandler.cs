using System;
using System.Net.Http;
using System.Threading.Tasks;
using NebulaAuth.Core;
using NebulaAuth.Model;
using SteamLib.Exceptions;
using SteamLib.Exceptions.Authorization;
using SteamLib.Exceptions.General;
using SteamLibForked.Exceptions.Authorization;

namespace NebulaAuth.Utility;

public static class ExceptionHandler
{
    private const string EXCEPTION_HANDLER_LOC_PATH = "ExceptionHandler";

    public static bool Handle(Exception ex, string? prefix = null, string? postfix = null,
        bool handleAllExceptions = false)
    {
        Shell.Logger.Error(ex);
        var msg = GetExceptionString(ex, handleAllExceptions);
        if (msg == null) return false;
        SnackbarController.SendSnackbar(prefix + msg + postfix);
        return true;
    }


    public static string? GetExceptionString(Exception exception, bool handleAllExceptions = true)
    {
        switch (exception)
        {
            case SessionPermanentlyExpiredException:
            {
                return "SessionExpiredException".GetCodeBehindLocalization();
            }
            case SessionInvalidException:
            {
                return "SessionExpiredException".GetCodeBehindLocalization();
            }
            case TaskCanceledException e:
            {
                return e.InnerException is TimeoutException
                    ? "TimeoutException".GetCodeBehindLocalization()
                    : "TaskCanceledException".GetCodeBehindLocalization();
            }
            case HttpRequestException e:
            {
                var str = "RequestError".GetCommonLocalization() + ": ";

                if (e.StatusCode != null)
                {
                    str = str + e.StatusCode;
                }
                else if (e.InnerException != null)
                {
                    str = str + e.InnerException.Message;
                }
                else
                {
                    str = str + e.Message;
                }

                return str;
            }
            case UnsupportedResponseException:
            {
                return "UnsupportedResponseException".GetCodeBehindLocalization();
            }
            case CantLoadConfirmationsException e:
            {
                var msg = LocManager.GetCodeBehindOrDefault(nameof(CantLoadConfirmationsException),
                    EXCEPTION_HANDLER_LOC_PATH, nameof(CantLoadConfirmationsException), "Common");
                if (e.Error == LoadConfirmationsError.Unknown)
                {
                    msg += e.ErrorMessage;
                    msg += ' ';
                    msg += e.ErrorDetails;
                }
                else
                {
                    msg += LocManager.GetCodeBehindOrDefault(e.Error.ToString(), EXCEPTION_HANDLER_LOC_PATH,
                        nameof(CantLoadConfirmationsException), e.Error.ToString());
                }

                return msg;
            }
            case SteamStatusCodeException e:
            {
                return "Error".GetCommonLocalization() + ": " +
                       ErrorTranslatorHelper.TranslateSteamStatusCode(e.StatusCode);
            }
            case LoginException e:
            {
                return "LoginException".GetCodeBehindLocalization() + ": " +
                       ErrorTranslatorHelper.TranslateLoginError(e.Error);
            }
            case not null when handleAllExceptions:
            {
                return "UnknownException".GetCodeBehindLocalization() + exception.Message;
            }
        }

        return null;
    }

    private static string GetCommonLocalization(this string key)
    {
        return LocManager.GetCommonOrDefault(key, key);
    }

    private static string GetCodeBehindLocalization(this string key)
    {
        return LocManager.GetCodeBehindOrDefault(key, EXCEPTION_HANDLER_LOC_PATH, key);
    }
}