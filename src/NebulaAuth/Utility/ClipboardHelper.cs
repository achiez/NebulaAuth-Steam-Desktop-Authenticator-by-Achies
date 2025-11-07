using System;
using System.Collections.Specialized;
using System.Windows.Forms;
using NebulaAuth.Core;
using NebulaAuth.Model;

namespace NebulaAuth.Utility;

public class ClipboardHelper
{
    public static bool Set(string text)
    {
        try
        {
            Clipboard.SetText(text);
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
            Clipboard.SetFileDropList(files);
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