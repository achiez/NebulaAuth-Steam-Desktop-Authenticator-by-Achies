using System.Threading.Tasks;
using AutoUpdaterDotNET;



namespace NebulaAuth.Core;

public static class UpdateManager
{
    private const string UPDATE_URL = "https://raw.githubusercontent.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/master/NebulaAuth/update.json";
    public static void CheckForUpdates()
    {
        AutoUpdater.Start(UPDATE_URL);

    }
}