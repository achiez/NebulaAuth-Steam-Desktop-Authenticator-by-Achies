using System;
using MaterialDesignThemes.Wpf;

namespace NebulaAuth.Core;

public class SnackbarController
{

    public static SnackbarMessageQueue MessageQueue { get; } = new() { DiscardDuplicates = true};
    private const int MIN_SNACKBAR_TIME = 1200;

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
    /// <param name="action"></param>
    /// <param name="duration">Default duration is 1 second</param>
    /// <param name="actionText">Default: 'OK'</param>
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
        var duration = str.Length / 0.03;
        if (duration < MIN_SNACKBAR_TIME)
        {
           duration = MIN_SNACKBAR_TIME;
        }
        return TimeSpan.FromMilliseconds(duration);

    }

}