using NebulaAuth.Model.Exceptions;
using NLog;
using SteamLib.Core;
using SteamLib.SteamMobile;
using System;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NebulaAuth.Model;

public static class Shell
{
    public static Logger Logger { get; } = LogManager.GetLogger("Logger");
    public static ILogger ExtensionsLogger { get; private set; } = null!;
    public static void Initialize()
    {

        var lp = new NLog.Extensions.Logging.NLogLoggerProvider();
        var logger = lp.CreateLogger("SteamLib");
        HealthMonitor.FatalLogger = logger;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        var loggerFactory = new NLog.Extensions.Logging.NLogLoggerFactory();
        ExtensionsLogger = loggerFactory.CreateLogger("Logger");

        try
        {
            TimeAligner.AlignTime();
        }
        catch (Exception ex)
        {
            throw new CantAlignTimeException("", ex);
        }
        ExtensionsLogger.LogDebug("Application started");
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Fatal((Exception)e.ExceptionObject);
    }
}