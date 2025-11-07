using Microsoft.Extensions.Logging;
using NebulaAuth.Model.Exceptions;
using NLog;
using NLog.Extensions.Logging;
using SteamLib.Core;
using SteamLib.SteamMobile;
using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NebulaAuth.Model;

public static class Shell
{
    public static Logger Logger { get; private set; } = null!;
    public static ILogger ExtensionsLogger { get; private set; } = null!;

    public static void Initialize()
    {
        Logger = LogManager.GetLogger("Logger");
        var lp = new NLogLoggerProvider();
        var logger = lp.CreateLogger("SteamLib");
        SteamLibErrorMonitor.MonitorLogger = logger;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        var loggerFactory = new NLogLoggerFactory();
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
        LogManager.Shutdown();
    }
}