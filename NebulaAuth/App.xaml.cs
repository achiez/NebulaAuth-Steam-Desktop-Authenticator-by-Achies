using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Exceptions;
using System;
using System.Windows;
using CodingSeb.Localization;

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
                msg = Loc.Tr(LocManager.GetCodeBehind("CantAlignTimeError"));
            }

            MessageBox.Show(msg);
            throw;
        }
    }
}
