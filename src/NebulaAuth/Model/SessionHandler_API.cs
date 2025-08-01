﻿using System.Threading.Tasks;
using AchiesUtilities.Web.Models;
using NebulaAuth.Model.Entities;
using SteamLib.Api.Mobile;
using SteamLib.Authentication;
using SteamLib.Authentication.LoginV2;
using SteamLib.Exceptions.Authorization;
using SteamLib.Factory.Helpers;
using SteamLib.SteamMobile;
using SteamLibForked.Models.Session;

namespace NebulaAuth.Model;

public partial class SessionHandler //API
{
    public static async Task RefreshMobileToken(HttpClientHandlerPair chp, Mafile mafile)
    {
        if (mafile.SessionData is not {RefreshToken.IsExpired: false})
            throw new SessionPermanentlyExpiredException(SessionInvalidException.SESSION_NULL_MSG);

        var mobileToken = await SteamMobileApi.RefreshJwt(chp.Client, mafile.SessionData.RefreshToken.Token,
            mafile.SessionData.SteamId);
        Shell.Logger.Info("MobileToken on {name} {steamid} successfully refreshed", mafile.AccountName,
            mafile.SessionData.SteamId);

        var newToken = SteamTokenHelper.Parse(mobileToken);
        mafile.SessionData.SetMobileToken(newToken);

        //Trigger PropertyChanged event for PortableMaClient handling session updated from MaClient
        //RETHINK: it makes double operation when session handled from PortableMaClient (more often scenario) which is unwanted behaviour
        mafile.SetSessionData(mafile.SessionData);
        Storage.UpdateMafile(mafile);
        chp.Handler.CookieContainer.SetSteamMobileCookiesWithMobileToken(mafile.SessionData);
    }

    public static async Task LoginAgain(HttpClientHandlerPair chp, Mafile mafile, string password, bool savePassword)
    {
        var sgGenerator = new SteamGuardCodeGenerator(mafile.SharedSecret);
        var options = new LoginV2ExecutorOptions(StaticLoginConsumer.Instance, chp.Client)
        {
            Logger = Shell.ExtensionsLogger,
            AuthProviders = [sgGenerator],
            DeviceDetails = DeviceDetailsDefaultBuilder.GetMobileDefaultDevice(),
            WebsiteId = "Mobile"
        };
        chp.Handler.CookieContainer.ClearMobileSessionCookies();
        var result = await LoginV2Executor.DoLogin(options, mafile.AccountName, password);
        Shell.Logger.Info("Logged in again on {name} {steamid}", mafile.AccountName, result.SteamId);
        AdmissionHelper.TransferCommunityCookies(chp.Handler.CookieContainer);

        //Triggers PropertyChanged event for PortableMaClient handling session updated from MaClient
        //RETHINK: it makes double operation when session handled from PortableMaClient (more often scenario) which is unwanted behaviour
        mafile.SetSessionData((MobileSessionData) result);
        if (PHandler.IsPasswordSet)
            mafile.Password = savePassword ? PHandler.Encrypt(password) : null;
        Storage.UpdateMafile(mafile);
    }
}