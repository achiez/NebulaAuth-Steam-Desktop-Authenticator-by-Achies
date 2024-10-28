using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.View;
using NebulaAuth.ViewModel.Other;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AchiesUtilities.Extensions;
using NebulaAuth.Model.Entities;
using SteamLib.Exceptions;
using NebulaAuth.Utility;
using NebulaAuth.View.Dialogs;

namespace NebulaAuth.ViewModel;

public partial class MainVM //File //TODO: Refactor
{

    public Settings Settings => Settings.Instance;

    [RelayCommand]
    private void OpenMafileFolder()
    {
        var mafile = SelectedMafile;

        var path = Storage.MafileFolder;
        string? mafilePath = null;
        if (mafile != null)
        {
            mafilePath = Storage.TryFindMafilePath(mafile);
        }
        if (mafilePath != null)
        {
            path = $"/select, \"{mafilePath}\"";
        }

        try
        {
            var processStartInfo = new ProcessStartInfo("explorer.exe", path);
            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            SnackbarController.SendSnackbar(ex.Message);
        }
    }


    [RelayCommand]
    private Task AddMafile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Mafile|*.mafile;*.maFile",
            Multiselect = false,

        };
        var fs = openFileDialog.ShowDialog();
        if (fs != true) return Task.CompletedTask;
        var path = openFileDialog.FileName;
        return AddMafile([path]);

    }
    public async Task AddMafile(string[] path)
    {
        bool? confirmOverwrite = null;
        var added = 0;
        var notAdded = 0;
        var errors = 0;
        foreach (var str in path)
        {
            try
            {
                Storage.AddNewMafile(str, confirmOverwrite ?? false);
                added++;
            }
            catch (FormatException)
            {
                errors++;
            }
            catch (IOException)
            {
                confirmOverwrite ??= await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmMafileOverwrite"));

                if (confirmOverwrite == true)
                {
                    Storage.AddNewMafile(str, true);
                    added++;
                }
                else if (confirmOverwrite == false)
                {
                    notAdded++;
                }
            }
            catch (SessionInvalidException ex)
            {
                if (path.Length == 1 && ex.Data.Contains("mafile"))
                {
                    var mafile = (Mafile)ex.Data["mafile"]!;
                    if (await HandleAddMafileWithoutSession(mafile))
                    {
                        added++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                else
                {
                    SnackbarController.SendSnackbar($"{GetLocalization("MafileImportError")} {Path.GetFileName(str)}{GetLocalization("MissingSessionInMafile")}", TimeSpan.FromSeconds(4));
                }
            }
        }

        var msg = GetLocalization("Import");
        if (added > 0)
        {
            msg += $" {GetLocalization("ImportAdded")} {added}.";
        }
        if (notAdded > 0)
        {
            msg += $" {GetLocalization("ImportSkipped")} {notAdded}.";
        }

        if (errors > 0)
        {
            msg += $" {GetLocalization("ImportErrors")} {errors}.";
        }
        SnackbarController.SendSnackbar(msg, TimeSpan.FromSeconds(2));
    }

    private async Task<bool> HandleAddMafileWithoutSession(Mafile data)
    {
        var loginAgainVm = await DialogsController.ShowLoginAgainOnImportDialog(data, Proxies);
        if (loginAgainVm == null)
        {
            return false;
        }

        var password = loginAgainVm.Password;
        if (!loginAgainVm.UseMafileProxy)
        {
            data.Proxy = loginAgainVm.SelectedProxy;
        }
        var waitDialog = new WaitLoginDialog();
        var wait = DialogHost.Show(waitDialog);
        try
        {
            await MaClient.LoginAgain(data, password, loginAgainVm.SavePassword, waitDialog);
            SnackbarController.SendSnackbar(GetLocalization("SuccessfulLogin"));
        }
        catch (LoginException ex)
        {
            SnackbarController.SendSnackbar(ErrorTranslatorHelper.TranslateLoginError(ex.Error), TimeSpan.FromSeconds(1.5));
        }
        catch (Exception ex)
            when (ExceptionHandler.Handle(ex))
        {
            Shell.Logger.Error(ex);
        }
        finally
        {
            DialogsController.CloseDialog();
            await wait;
        }
        var result = data.SessionData != null;
        if (!result) return result;
        Storage.SaveMafile(data);
        {
            ResetQuery();
            SearchText = data.AccountName ?? string.Empty;
            SelectedMafile = data;
        } //As this operation used only for 1 mafile at time, we can safely assume that we can select it for convenience
        return result;

    }

    [RelayCommand]
    private async Task RemoveMafile()
    {
        if (SelectedMafile == null) return;
        var confirm =
            await DialogsController.ShowConfirmCancelDialog(GetLocalization("RemoveMafileConfirmation"));

        if (!confirm) return;
        try
        {
            Storage.MoveToRemoved(SelectedMafile);
        }
        catch (UnauthorizedAccessException)
        {
            SnackbarController.SendSnackbar(GetLocalization("CantRemoveAlreadyExist"));
        }
        catch (Exception ex)
        {
            SnackbarController.SendSnackbar(
                $"{GetLocalization("CantRemoveMafile")} {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenSettingsDialog()
    {
        var vm = new SettingsVM();
        var view = new SettingsView()
        {
            DataContext = vm
        };
        await DialogHost.Show(view);
    }

    [RelayCommand]
    private async Task PasteMafilesFromClipboard()
    {
        StringCollection files;
        try
        {
            files = Clipboard.GetFileDropList();
        }
        catch (Exception ex)
        {
            Shell.Logger.Error(ex);
            return;
        }

        var arr = files.Cast<string>().ToArray();
        if (arr.All(p => p.ContainsIgnoreCase("mafile") == false)) return;



        await AddMafile(arr);
    }

    [RelayCommand]
    private void CopyLogin(object? mafile)
    {
        if(mafile is not Mafile maf) return;
        var i = 0;
        while (i < 20)
        {
            try
            {
                Clipboard.SetText(maf.AccountName);
                SnackbarController.SendSnackbar(GetLocalization("LoginCopied"));
                return;
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
    }

    [RelayCommand]
    private void CopyMafile(object? mafile)
    {
        if (mafile is not Mafile maf) return;
        var i = 0;
        var path = Storage.TryFindMafilePath(maf);
        if (path == null)
        {
            SnackbarController.SendSnackbar(GetLocalization("MafileNotCopied"));
            return;
        }

        while (i < 20)
        {
            try
            {
                Clipboard.SetFileDropList([path]);
                SnackbarController.SendSnackbar(GetLocalization("MafileCopied"));
                return;
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
    }
}