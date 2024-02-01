using NebulaAuth.Core;
using NebulaAuth.Model.Entities;
using SteamLib.Exceptions;
using System;
using System.Threading.Tasks;

namespace NebulaAuth.Model;

public static class SessionHandler
{
    public static event EventHandler? LoginStarted;
    public static event EventHandler? LoginCompleted;

    public static async Task<T> Handle<T>(Func<Task<T>> func, Mafile mafile)
    {
        string? password = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(mafile.Password))
            {
                password = PHandler.Decrypt(mafile.Password);
            }
        }
        catch
        {
            // ignored
        }

        var refreshed = false;
        try
        {
            return await func();
        }
        catch (SessionExpiredException) when (mafile.SessionData is not { RefreshToken.IsExpired: true})
        {
            refreshed = await TryRefresh(mafile);
        }
        catch (SessionInvalidException)
            when (password != null)
        {
        }


        if (refreshed)
        {
            Shell.Logger.Debug("Token on mafile {name} {steamid} refreshed", mafile.AccountName,
                mafile.SessionData?.SteamId);
            try
            {
                return await func();
            }
            catch (Exception ex3)
                when (password != null && ex3 is SessionExpiredException or SessionInvalidException)
            {

            }
        }


        if (password == null)
        {
            throw new SessionInvalidException();
        }

        try
        {
            LoginStarted?.Invoke(null, EventArgs.Empty);
            await MaClient.LoginAgain(mafile, password, savePassword: true, null);
            Shell.Logger.Debug("Mafile {name} {steamid} succesfully auto-relogined", mafile.AccountName,
                mafile.SessionData?.SteamId);
        }
        finally
        {
            LoginCompleted?.Invoke(null, EventArgs.Empty);
        }

        return await func();
    }

    private static async Task<bool> TryRefresh(Mafile mafile)
    {
        try
        {
            await MaClient.RefreshSession(mafile);
            SnackbarController.SendSnackbar(LocManager.GetCodeBehindOrDefault("SessionWasRefreshedAutomatically", "SessionHandler", "SessionWasRefreshedAutomatically"));
            return true;
        }
        catch (SessionInvalidException)
        {
            Shell.Logger.Debug("Token on mafile {name} {steamid} not refreshed", mafile.AccountName, mafile.SessionData?.SteamId);
            return false;
        }
    }
}