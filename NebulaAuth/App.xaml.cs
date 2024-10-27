using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Exceptions;
using System;
using System.Windows;

namespace NebulaAuth;


public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LocManager.Init();
        LocManager.SetApplicationLocalization(Settings.Instance.Language);
        try
        {
            Shell.Initialize();
        }
        catch (Exception ex)
        {
            var msg = ex.ToString();
            if (ex is CantAlignTimeException)
            {
                msg = LocManager.Get("CantAlignTimeError");
            }

            MessageBox.Show(msg, "Error", MessageBoxButton.OK,  MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            throw;

        }
    }
}
