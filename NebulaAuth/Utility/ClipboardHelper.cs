using NebulaAuth.Core;
using NebulaAuth.Model;
using System;
using System.Collections.Specialized;
using System.Windows;

namespace NebulaAuth.Utility;

public class ClipboardHelper
{
    public static bool Set(string text)
    {
        var i = 0;
        while (i < 20)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (Exception ex)
            {
                if (i == 19)
                {
                    Shell.Logger.Error(ex);
                    SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Error", "Error"));
                }

            }
            i++;
        }

        return false;
    }

    public static bool SetFiles(StringCollection files)
    {
        var i = 0;
        while (i < 20)
        {
            try
            {
                Clipboard.SetFileDropList(files);
                return true;
            }
            catch (Exception ex)
            {
                if (i == 19)
                {
                    Shell.Logger.Error(ex);
                    SnackbarController.SendSnackbar(LocManager.GetCommonOrDefault("Error", "Error"));
                }

            }
            i++;
        }

        return false;
    }
}