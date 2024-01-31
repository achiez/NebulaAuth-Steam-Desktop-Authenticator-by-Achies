using Microsoft.Extensions.Logging;

namespace SteamLib.Core;

public static class HealthMonitor
{
    public static ILogger? FatalLogger { get; set; }
    internal static void LogUnexpected(string response, Exception ex)
    {
        FatalLogger?.LogCritical(ex, "Unexpected response detected:\n{response}", response);
    }


    internal static T LogOnException<T>(string resp, Func<T> del)
    {
        try
        {
            return del();
        }
        catch (Exception ex)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }

    internal static T LogOnException<T>(string resp, Func<string, T> del)
    {
        try
        {
            return del(resp);
        }
        catch (Exception ex)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }
    internal static T LogOnException<T>(string resp, Func<T> del, params Type[] exceptExceptions)
    {
        try
        {
            return del();
        }
        catch (Exception ex)
            when (exceptExceptions.Any(t => t == ex.GetType()) == false)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }

    internal static T LogOnException<T>(string resp, Func<string, T> del, params Type[] exceptExceptions)
    {
        try
        {
            return del(resp);
        }
        catch (Exception ex)
            when (exceptExceptions.Any(t => t == ex.GetType()) == false)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }

    internal static T LogOnException<T>(string resp, Func<T> del, Func<Exception, bool> logPredicate)
    {
        try
        {
            return del();
        }
        catch (Exception ex)
            when (logPredicate(ex))
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }

    internal static T LogOnException<T>(string resp, Func<string, T> del, Func<Exception, bool> logPredicate)
    {
        try
        {
            return del(resp);
        }
        catch (Exception ex)
            when (logPredicate(ex))
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }

    internal static void LogOnException(string resp, Action del)
    {
        try
        {
            del();
        }
        catch (Exception ex)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }
    internal static void LogOnException(string resp, Action del, Func<Exception, bool> logPredicate)
    {
        try
        {
            del();
        }
        catch (Exception ex)
            when (logPredicate(ex))
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }


    internal static async Task<T> LogOnExceptionAsync<T>(string resp, Func<Task<T>> del)
    {
        try
        {
            return await del();
        }
        catch (Exception ex)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }
    internal static async Task<T> LogOnExceptionAsync<T>(string resp, Func<Task<T>> del, params Type[] exceptExceptions)
    {
        try
        {
            return await del();
        }
        catch (Exception ex)
            when (exceptExceptions.Any(t => t == ex.GetType()) == false)
        {
            LogUnexpected(resp, ex);
            throw;
        }
    }


}