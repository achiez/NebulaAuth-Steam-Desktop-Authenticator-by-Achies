using NebulaAuth.Core;
using NebulaAuth.Model;
using System;
using System.Collections.Specialized;

namespace NebulaAuth.Utility;

public class ClipboardHelper
{
    public static bool Set(string text)
    {
        try
        {
            System.Windows.Forms.Clipboard.SetText(text);
            return true;
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex);
            SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Error", "Error"));
        }

        return false;
    }

    public static bool SetFiles(StringCollection files)
    {
        try
        {
            System.Windows.Forms.Clipboard.SetFileDropList(files);
            return true;
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex);
            SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Error", "Error"));
        }
        return false;
    }
}