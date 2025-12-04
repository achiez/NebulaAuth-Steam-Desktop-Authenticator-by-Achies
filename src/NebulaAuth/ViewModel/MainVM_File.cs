using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AchiesUtilities.Extensions;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Exceptions;
using NebulaAuth.Utility;
using NebulaAuth.View;
using NebulaAuth.View.Dialogs;
using NebulaAuth.ViewModel.Other;
using SteamLibForked.Exceptions.Authorization;

namespace NebulaAuth.ViewModel;

public partial class MainVM //File //TODO: Refactor
{
    public Settings Settings => Settings.Instance;

    [RelayCommand]
    private void OpenMafileFolder()
    {
        var mafile = SelectedMafile;

        var path = Storage.MafilesDirectory;
        string? mafilePath = null;
        if (mafile != null)
        {
            mafilePath = Storage.TryGetMafilePath(mafile);
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
            Multiselect = false
        };
        var fs = openFileDialog.ShowDialog();
        if (fs != true) return Task.CompletedTask;
        var path = openFileDialog.FileName;
        return AddMafile([path]);
    }

    [RelayCommand]
    private Task AddMafilesBatch()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Mafile|*.mafile;*.maFile",
            Multiselect = true
        };
        var fs = openFileDialog.ShowDialog();
        if (fs != true) return Task.CompletedTask;
        var paths = openFileDialog.FileNames;
        return AddMafilesBatchSequential(paths);
    }

    private async Task AddMafilesBatchSequential(string[] paths)
    {
        bool? confirmOverwrite = null;
        var added = 0;
        var notAdded = 0;
        var errors = 0;
        
        foreach (var str in paths)
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
                confirmOverwrite ??=
                    await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmMafileOverwrite"));

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
            catch (MafileNeedReloginException ex)
            {
                if (paths.Length == 1 && ex.Mafile != null)
                {
                    var mafile = ex.Mafile;
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
                    SnackbarController.SendSnackbar(
                        $"{GetLocalization("MafileImportError")} {Path.GetFileName(str)}{GetLocalization("MissingSessionInMafile")}",
                        TimeSpan.FromSeconds(4));
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
                confirmOverwrite ??=
                    await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmMafileOverwrite"));

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
            catch (MafileNeedReloginException ex)
            {
                if (path.Length == 1 && ex.Mafile != null)
                {
                    var mafile = ex.Mafile;
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
                    SnackbarController.SendSnackbar(
                        $"{GetLocalization("MafileImportError")} {Path.GetFileName(str)}{GetLocalization("MissingSessionInMafile")}",
                        TimeSpan.FromSeconds(4));
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
            await MaClient.LoginAgain(data, password, loginAgainVm.SavePassword);
            SnackbarController.SendSnackbar(GetLocalization("SuccessfulLogin"));
        }
        catch (LoginException ex)
        {
            SnackbarController.SendSnackbar(ErrorTranslatorHelper.TranslateLoginError(ex.Error),
                TimeSpan.FromSeconds(1.5));
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
        await Storage.SaveMafileAsync(data);
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
        var view = new SettingsView(CurrentDialogHost)
        {
            DataContext = vm
        };
        await DialogHost.Show(view);
    }

    [RelayCommand]
    private async Task OpenEmailManagerDialog()
    {
        await DialogsController.ShowEmailManagerDialog();
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
        if (mafile is not Mafile maf) return;
        if (ClipboardHelper.Set(maf.AccountName))
            SnackbarController.SendSnackbar(GetLocalization("LoginCopied"));
    }

    [RelayCommand]
    private void CopySteamId(object? mafile)
    {
        if (mafile is not Mafile maf) return;

        if (ClipboardHelper.Set(maf.SteamId.ToString()))
            SnackbarController.SendSnackbar(GetLocalization("SteamIdCopied"));
    }

    [RelayCommand]
    private void CopyMafile(object? mafile)
    {
        if (mafile is not Mafile maf) return;
        var path = Storage.TryGetMafilePath(maf);
        if (ClipboardHelper.SetFiles([path]))
            SnackbarController.SendSnackbar(GetLocalization("MafileCopied"));
    }

    [RelayCommand(CanExecute = nameof(CanCopyPassword))]
    private void CopyPassword(object? mafile)
    {
        if (mafile is not Mafile maf) return;
        if (maf.Password == null) return;
        try
        {
            var pass = PHandler.Decrypt(maf.Password);
            if (ClipboardHelper.Set(pass))
                SnackbarController.SendSnackbar(GetLocalization("PasswordCopied"));
        }
        catch
        {
            SnackbarController.SendSnackbar(GetLocalization("CantDecryptPassword"));
        }
    }

    private bool CanCopyPassword(object? mafile)
    {
        if (mafile is not Mafile maf) return false;
        return maf.Password != null && PHandler.IsPasswordSet;
    }
}