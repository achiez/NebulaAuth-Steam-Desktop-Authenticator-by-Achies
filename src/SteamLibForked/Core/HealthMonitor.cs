using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;
using SteamLib.Exceptions.General;

namespace SteamLib.Core;

/// <summary>
///     Provides unified error handling, logging, and mapping for responses from Steam.
///     This class is used to log and analyze errors that occur due to server changes
///     or code issues, ensuring consistency in logging and exception handling.
/// </summary>
public static class SteamLibErrorMonitor
{
    /// <summary>
    ///     Logger for capturing and analyzing error-related issues.
    /// </summary>
    public static ILogger? MonitorLogger { get; set; }


    /// <summary>
    ///     Logs the response and the associated exception using the <see cref="MonitorLogger" />.
    ///     Differentiates between <see cref="UnsupportedResponseException" /> and other exceptions.
    /// </summary>
    /// <param name="response">The response string to log.</param>
    /// <param name="ex">The exception that occurred.</param>
    internal static void LogErrorResponse(string response, Exception ex)
    {
        if (ex is UnsupportedResponseException)
        {
            MonitorLogger?.LogError(ex, "Unsupported response detected");
        }
        else
        {
            MonitorLogger?.LogError(ex, "Unsupported response detected: {response}", response);
        }
    }

    /// <summary>
    ///     Maps the error to an <see cref="UnsupportedResponseException" />, logs it, and then throws the exception.
    ///     This method is used to handle unexpected responses and map them to a known exception type.
    /// </summary>
    /// <param name="response">The response that triggered the error.</param>
    /// <param name="ex">The original exception.</param>
    /// <exception cref="UnsupportedResponseException">Always throws after logging the response.</exception>
    [DoesNotReturn]
    internal static void MapAndThrowException(string response, Exception ex)
    {
        var e = new UnsupportedResponseException(response, ex);
        ExceptionDispatchInfo.SetCurrentStackTrace(e);
        LogErrorResponse(response, e);
        throw e;
    }

    [DoesNotReturn]
    internal static T MapAndThrowException<T>(string response, Exception ex)
    {
        var e = new UnsupportedResponseException(response, ex);
        ExceptionDispatchInfo.SetCurrentStackTrace(e);
        LogErrorResponse(response, e);
        throw e;
    }

    /// <summary>
    ///     Ensures the execution of a delegate and handles any exceptions by mapping them to a known type.
    ///     If an <see cref="UnsupportedResponseException" /> is caught, it is logged and rethrown.
    ///     Otherwise, any other exceptions are remapped to <see cref="UnsupportedResponseException" />.
    /// </summary>
    /// <typeparam name="T">The return type of the delegate.</typeparam>
    /// <param name="resp">The response string for logging purposes.</param>
    /// <param name="del">The delegate to execute.</param>
    /// <returns>The result of the delegate execution.</returns>
    /// <exception cref="UnsupportedResponseException">Thrown when an unsupported response is encountered.</exception>
    internal static T HandleResponse<T>(string resp, Func<T> del)
    {
        try
        {
            return del();
        }
        catch (UnsupportedResponseException ex)
        {
            LogErrorResponse(resp, ex);
            throw;
        }
        catch (Exception ex)
        {
            MapAndThrowException(resp, ex);
            return default; //Not reachable
        }
    }

    /// <summary>
    ///     Ensures the execution of a delegate and handles any exceptions by mapping them to a known type.
    ///     If an <see cref="UnsupportedResponseException" /> is caught, it is logged and rethrown.
    ///     Otherwise, any other exceptions are remapped to <see cref="UnsupportedResponseException" />.
    /// </summary>
    /// <typeparam name="T">The return type of the delegate.</typeparam>
    /// <param name="resp">The response string for logging purposes.</param>
    /// <param name="del">The delegate to execute.</param>
    /// <returns>The result of the delegate execution.</returns>
    /// <exception cref="UnsupportedResponseException">Thrown when an unsupported response is encountered.</exception>
    internal static T HandleResponse<T>(string resp, Func<string, T> del)
    {
        try
        {
            return del(resp);
        }
        catch (UnsupportedResponseException ex)
        {
            LogErrorResponse(resp, ex);
            throw;
        }
        catch (Exception ex)
        {
            MapAndThrowException(resp, ex);
            return default; //Not reachable
        }
    }

    /// <summary>
    ///     Ensures the execution of a delegate and handles any exceptions by mapping them to a known type.
    ///     If an <see cref="UnsupportedResponseException" /> is caught, it is logged and rethrown.
    ///     Otherwise, any other exceptions are remapped to <see cref="UnsupportedResponseException" />.
    /// </summary>
    /// <param name="resp">The response string for logging purposes.</param>
    /// <param name="del">The delegate to execute.</param>
    /// <returns>The result of the delegate execution.</returns>
    /// <exception cref="UnsupportedResponseException">Thrown when an unsupported response is encountered.</exception>
    internal static void HandleResponse(string resp, Action del)
    {
        try
        {
            del();
        }
        catch (UnsupportedResponseException ex)
        {
            LogErrorResponse(resp, ex);
            throw;
        }
        catch (Exception ex)
        {
            MapAndThrowException(resp, ex);
        }
    }

    /// <summary>
    ///     Ensures the execution of a delegate and handles any exceptions by mapping them to a known type.
    ///     If an <see cref="UnsupportedResponseException" /> is caught, it is logged and rethrown.
    ///     Otherwise, any other exceptions are remapped to <see cref="UnsupportedResponseException" />.
    /// </summary>
    /// <param name="resp">The response string for logging purposes.</param>
    /// <param name="del">The delegate to execute.</param>
    /// <returns>The result of the delegate execution.</returns>
    /// <exception cref="UnsupportedResponseException">Thrown when an unsupported response is encountered.</exception>
    internal static void HandleResponse(string resp, Action<string> del)
    {
        try
        {
            del(resp);
        }
        catch (UnsupportedResponseException ex)
        {
            LogErrorResponse(resp, ex);
            throw;
        }
        catch (Exception ex)
        {
            MapAndThrowException(resp, ex);
        }
    }
}