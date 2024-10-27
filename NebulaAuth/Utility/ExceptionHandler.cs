using NebulaAuth.Core;
using NebulaAuth.Model;
using SteamLib.Exceptions;
using SteamLib.Exceptions.General;
using SteamLib.ProtoCore.Exceptions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NebulaAuth.Utility;

public static class ExceptionHandler
{
    private const string EXCEPTION_HANDLER_LOC_PATH = "ExceptionHandler";
    public static bool Handle(Exception ex, string? prefix = null, string? postfix = null, bool handleAllExceptions = false)
    {
        string msg;
        Shell.Logger.Error(ex);
        switch (ex)
        {
            case SessionPermanentlyExpiredException:
                {
                    msg = "SessionExpiredException".GetCodeBehindLocalization();
                    break;
                }
            case SessionInvalidException:
                {
                    msg = "SessionExpiredException".GetCodeBehindLocalization();
                    break;
                }
            case TaskCanceledException e:
                {
                    msg = e.InnerException is TimeoutException
                        ? "TimeoutException".GetCodeBehindLocalization()
                        : "TaskCanceledException".GetCodeBehindLocalization();
                    break;
                }
            case HttpRequestException e:
                {
                    var str = "RequestError".GetCommonLocalization() + ": ";
                    if (e.StatusCode != null)
                    {
                        msg = str + e.StatusCode;
                    }
                    else if (e.InnerException != null)
                    {
                        msg = (str + e.InnerException.Message);
                    }
                    else
                    {
                        msg = (str + e.Message);
                    }

                    break;
                }
            case UnsupportedResponseException:
                {
                    msg = "UnsupportedResponseException".GetCodeBehindLocalization();
                    break;
                }
            case CantLoadConfirmationsException e:
                {
                    msg = LocManager.GetCodeBehindOrDefault(nameof(CantLoadConfirmationsException), EXCEPTION_HANDLER_LOC_PATH, nameof(CantLoadConfirmationsException), "Common");
                    if (e.Error == LoadConfirmationsError.Unknown)
                    {
                        msg += e.ErrorMessage;
                        msg += ' ';
                        msg += e.ErrorDetails;
                    }
                    else
                    {
                        msg += LocManager.GetCodeBehindOrDefault(e.Error.ToString(), EXCEPTION_HANDLER_LOC_PATH, nameof(CantLoadConfirmationsException),  e.Error.ToString());
                    }
                    break;
                }
            case EResultException e:
                {
                    msg = "Error".GetCommonLocalization() + ": " + ErrorTranslatorHelper.TranslateEResult(e.Result);
                    break;
                }
            case LoginException e:
                {
                    msg = "LoginException".GetCodeBehindLocalization() + ": " + ErrorTranslatorHelper.TranslateLoginError(e.Error);
                    break;
                }
            case not null when handleAllExceptions:
                {
                    msg = "UnknownException".GetCodeBehindLocalization() + ex.Message;
                    break;
                }
            default:
                return false;
        }

        SnackbarController.SendSnackbar(prefix + msg + postfix);
        return true;
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