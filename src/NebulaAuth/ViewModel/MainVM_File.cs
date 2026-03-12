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
using NebulaAuth.Model.Mafiles;
using NebulaAuth.Model.MafilesLegacy;
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
        var dialog = new OpenFileDialog
        {
            Filter = "Mafile|*.mafile;*.maFile",
            Multiselect = false
        };

        return dialog.ShowDialog() == true
            ? AddMafile([dialog.FileName])
            : Task.CompletedTask;
    }

    public async Task AddMafile(string[] path)
    {
        bool? confirmOverwrite = null;
        var summary = new MafileImportSummary();
        SDAEncryptionHelper.Context? sdaContext = null;
        var sdaPasswordPrompted = false;
        foreach (var str in path)
        {
            try
            {
                var (mafile, passwordPrompted, context) = await TryReadMafile(str, sdaContext, sdaPasswordPrompted);
                sdaContext = context;
                sdaPasswordPrompted = passwordPrompted;
                if (mafile == null)
                {
                    summary.ErrorOne();
                    continue;
                }

                var overwrite = confirmOverwrite ?? false;
                var res = await Storage.AddNewMafileFromData(mafile, overwrite);
                if (res == AddMafileResult.AlreadyExist && confirmOverwrite == null)
                {
                    confirmOverwrite =
                        await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmMafileOverwrite"));
                    res = await Storage.AddNewMafileFromData(mafile, confirmOverwrite.Value);
                }

                summary.Apply(res);
            }
            catch (FormatException)
            {
                summary.ErrorOne();
            }
            catch (MafileNeedReloginException ex)
            {
                if (path.Length == 1 && ex.Mafile != null)
                {
                    var mafile = ex.Mafile;
                    if (await HandleAddMafileWithoutSession(mafile))
                    {
                        summary.AddedOne();
                    }
                    else
                    {
                        summary.ErrorOne();
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

        ShowImportSummary(summary);
    }


    private async Task<MafileReadResult> TryReadMafile(string path, SDAEncryptionHelper.Context? sdaContext,
        bool sdaPasswordPrompted)
    {
        try
        {
            var content = await File.ReadAllTextAsync(path);
            Mafile mafile;
            if (!SDAEncryptionHelper.LooksLikeSdaEncryptedBlob(content))
            {
                mafile = NebulaSerializer.Deserialize(content, path);
                return new MafileReadResult(mafile, sdaPasswordPrompted, sdaContext);
            }

            // Looks like encrypted, let's see if we have manifest
            sdaContext ??= SDAEncryptionHelper.TryDetect(path, sdaContext?.SdaManifest);
            if (sdaContext != null)
            {
                // We have manifest, but we must ensure we have password
                if (sdaContext.Password == null && !sdaPasswordPrompted)
                {
                    sdaPasswordPrompted = true;
                    var password = await DialogsController.ShowSdaPasswordDialog();
                    sdaContext = sdaContext.WithPassword(password);
                }

                var decrypted = SDAEncryptionHelper.TryDecrypt(content, path, sdaContext);
                if (decrypted == null) return new MafileReadResult(null, sdaPasswordPrompted, sdaContext);
                content = decrypted;
            }

            // If we are here, it means that either file is not encrypted, or we successfully decrypted it
            mafile = NebulaSerializer.Deserialize(content, path);
            return new MafileReadResult(mafile, sdaPasswordPrompted, sdaContext);
        }
        catch (Exception ex)
            when (ex is not MafileNeedReloginException)
        {
            Shell.Logger.Warn(ex, "Failed to import mafile");
            throw new FormatException($"Failed to read mafile {Path.GetFileName(path)}", ex);
        }
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

    private static void ShowImportSummary(MafileImportSummary summary)
    {
        var msg = GetLocalization("Import");
        if (summary.Added > 0)
        {
            msg += $" {GetLocalization("ImportAdded")} {summary.Added}.";
        }

        if (summary.NotAdded > 0)
        {
            msg += $" {GetLocalization("ImportSkipped")} {summary.NotAdded}.";
        }

        if (summary.Errors > 0)
        {
            msg += $" {GetLocalization("ImportErrors")} {summary.Errors}.";
        }

        SnackbarController.SendSnackbar(msg, TimeSpan.FromSeconds(2));
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
        if (arr.All(p => !p.ContainsIgnoreCase("mafile"))) return;


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

    private record MafileReadResult(
        Mafile? Mafile,
        bool SdaPasswordPrompted,
        SDAEncryptionHelper.Context? SdaContext);
}