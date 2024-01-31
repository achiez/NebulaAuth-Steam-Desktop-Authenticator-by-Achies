using System;
using MaterialDesignThemes.Wpf;

namespace NebulaAuth.Core;

public class SnackbarController
{

    public static SnackbarMessageQueue MessageQueue { get; } = new() { DiscardDuplicates = true};
    private const int MIN_SNACKBAR_TIME = 1000;
    public SnackbarController()
    {

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="duration">Default duration is 1 second</param>
    public static void SendSnackbar(string text, TimeSpan? duration = null)
    {
        duration ??= GetSnackbarTime(text);
        MessageQueue.Enqueue(text, null, null, null, false, false, duration.Value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="duration">Default duration is 1 second</param>
    public static void SendSnackbarWithButton(string text, string actionText = "OK", Action? action = null, TimeSpan? duration = null)
    {
        duration ??= GetSnackbarTime(text);
        Action<object?> argAction;
        if (action == null)
        {
            argAction = _ => { };
        }
        else
        {
            argAction = _ => action();
        }

        MessageQueue.Enqueue(text, actionText, argAction, null, false, false, duration);
    }

    private static TimeSpan GetSnackbarTime(string str)
    {
        if (str.Length <= 100) return TimeSpan.FromMilliseconds(MIN_SNACKBAR_TIME);
                
        var length = str.Length / 0.07;
        return TimeSpan.FromMilliseconds(length);

    }

}