using System.Threading.Tasks;
using AutoUpdaterDotNET;



namespace NebulaAuth.Core;

public static class UpdateManager
{
    public static async Task CheckForUpdates()
    {
        AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.xml");

    }
}