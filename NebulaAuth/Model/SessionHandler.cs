using AchiesUtilities.Web.Models;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Core;
using NebulaAuth.Model.Entities;
using SteamLib.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NebulaAuth.View.Dialogs;
using SteamLib.Utility;

namespace NebulaAuth.Model;

public static partial class SessionHandler
{

    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static async Task<T> Handle<T>(Func<Task<T>> func, Mafile mafile,
        SocketsClientHandlerPair? chp = null, string? snackbarPrefix = null)
    {
        chp ??= MaClient.Chp;
        await Semaphore.WaitAsync();
        try
        {
            return await HandleInternal(func, chp.Value, mafile, snackbarPrefix);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task<T> HandleInternal<T>(Func<Task<T>> func, SocketsClientHandlerPair chp, Mafile mafile,
        string? snackbarPrefix = null)
    {
        using var scope = Shell.Logger.PushScopeProperty("Scope", "SessionHandler");
        var mobileTokenExpired = MobileTokenExpired(mafile);
        var refreshTokenExpired = RefreshTokenExpired(mafile);
        var password = GetPassword(mafile);

        if (!mobileTokenExpired)
        {
            try
            {
                return await func();
            }
            catch (SessionInvalidException ex)
                when (refreshTokenExpired == false || password != null)
            {
                if (ex is SessionPermanentlyExpiredException)
                {
                    Shell.Logger.Debug(ex, "RefreshToken on mafile {name} {steamid} is expired", mafile.AccountName, mafile.SessionData?.SteamId);
                    refreshTokenExpired = true;
                }
                else
                {
                    Shell.Logger.Debug(ex, "MobileToken on mafile {name} {steamid} is expired", mafile.AccountName, mafile.SessionData?.SteamId);
                }
            }
        }


        //State: mobileToken invalid/expired, refreshToken maybe not expired
        if (!refreshTokenExpired)
        {
            var refreshed = await RefreshInternal(chp, mafile);
            if (refreshed)
            {
                SnackbarController.SendSnackbar(snackbarPrefix + LocManager.GetCodeBehindOrDefault("SessionWasRefreshedAutomatically", "SessionHandler", "SessionWasRefreshedAutomatically"));
                try
                {
                    return await func();
                }
                catch (SessionInvalidException ex)
                {
                    Shell.Logger.Info(ex, "MobileToken on {name} {steamid} was refreshed but after it, error occured", mafile.AccountName, mafile.SessionData?.SteamId);
                    if (password == null)
                        throw;
                }
            }
        }

        Shell.Logger.Debug("Session on mafile {name} {steamid} is invalid/expired", mafile.AccountName, mafile.SessionData?.SteamId);

        //State: mobileToken invalid/expired, refreshToken invalid/expired
        if (password != null)
        {
            var logged = await LoginAgainInternal(chp, mafile, password, true);
            if (logged)
            {
                Shell.Logger.Debug("Mafile {name} {steamid} was succesfully auto-relogined", mafile.AccountName, mafile.SessionData?.SteamId);
                return await func();
            }
        }

        //Nothing to do more, everything is expired
        throw new SessionPermanentlyExpiredException(SessionPermanentlyExpiredException.SESSION_EXPIRED_MSG);
    }



    private static bool MobileTokenExpired(Mafile mafile)
    {
        var mobileToken = mafile.SessionData?.GetMobileToken();
        return mobileToken == null || mobileToken.Value.IsExpired;
    }

    private static bool RefreshTokenExpired(Mafile mafile)
    {
        return mafile.SessionData?.RefreshToken.IsExpired != false;
    }

    private static string? GetPassword(Mafile mafile)
    {
        try
        {
            if (PHandler.IsPasswordSet && !string.IsNullOrWhiteSpace(mafile.Password))
            {
                return PHandler.Decrypt(mafile.Password);
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }


    private static async Task<bool> RefreshInternal(SocketsClientHandlerPair chp, Mafile mafile)
    {
        try
        {
            await RefreshMobileToken(chp, mafile);
            return true;
        }
        catch(Exception ex)
        {
            Shell.Logger.Debug(ex, "Failed to refresh session on mafile {name} {steamid}", mafile.AccountName, mafile.SessionData?.SteamId);
            return false;
        }
    }

    private static async Task<bool> LoginAgainInternal(SocketsClientHandlerPair chp, Mafile mafile, string password,
        bool savePassword)
    {
        var t = Task.Run(OnLoginStarted);
        try
        {

            await LoginAgain(chp, mafile, password, savePassword);
            return true;
        }
        catch (Exception ex)
        {
            Shell.Logger.Debug(ex, "Failed to relogin mafile {name} {steamid}", mafile.AccountName,
                mafile.SessionData?.SteamId);
            return false;
        }
        finally
        {
            OnLoginCompleted();
            await t;
        }
    }

    private static async Task OnLoginStarted()
    {
        if (DialogHost.IsDialogOpen(null)) return;
        await Application.Current.Dispatcher.BeginInvoke(async () =>
        {
            await DialogHost.Show(new WaitLoginDialog());
        });
    }

    private static void OnLoginCompleted()
    {
        var currentSession = DialogHost.GetDialogSession(null);
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (currentSession is { Content: WaitLoginDialog, IsEnded: false })
            {
                try
                {
                    currentSession.Close();
                }
                catch
                {
                    //Ignored
                }
            }
        });
    }


}