using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NebulaAuth.Core;
using NebulaAuth.Model.MAAC;
using NebulaAuth.Model.MafileExport;
using NebulaAuth.Model.Mafiles;
using NLog;
using NLog.Extensions.Logging;
using SteamLib.Core;
using SteamLib.SteamMobile;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NebulaAuth.Model;

public static class Shell
{
    public static Logger Logger { get; private set; } = null!;
    public static ILogger ExtensionsLogger { get; private set; } = null!;

    public static async Task Initialize()
    {
        File.Delete("log.log");
        LocManager.Init();
        LocManager.SetApplicationLocalization(Settings.Instance.Language);
        Logger = LogManager.GetLogger("Logger");
        var lp = new NLogLoggerProvider();
        var logger = lp.CreateLogger("SteamLib");
        SteamLibErrorMonitor.MonitorLogger = logger;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        var loggerFactory = new NLogLoggerFactory();
        ExtensionsLogger = loggerFactory.CreateLogger("Logger");

        try
        {
            await TimeAligner.AlignTimeAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to align time with Steam");
            TimeAligner.SetTimeDifference(0);
        }

        var threads = Environment.ProcessorCount > 0 ? Environment.ProcessorCount : 1;
        await Storage.Initialize(threads);
        ProxyAssignmentCache.Initialize(Storage.MaFiles);
        MAACStorage.Initialize();
        MafileExporterStorage.Initialize();
        _ = LinksManager.FetchAsync();

        ExtensionsLogger.LogDebug("Application started");
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Fatal((Exception) e.ExceptionObject);
        LogManager.Shutdown();
    }
}