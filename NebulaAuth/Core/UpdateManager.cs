using AutoUpdaterDotNET;
using NebulaAuth.Model;
using System;
using System.IO;


namespace NebulaAuth.Core;

public static class UpdateManager
{
    private const string UPDATE_URL = "https://raw.githubusercontent.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/master/NebulaAuth/update.xml";
    public static void CheckForUpdates()
    {
        string jsonPath = Path.Combine(Environment.CurrentDirectory, "update-settings.json");
        AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
        AutoUpdater.ShowSkipButton = false;
        if (Settings.Instance.AllowAutoUpdate)
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
        AutoUpdater.Start(UPDATE_URL);
       

    }

}