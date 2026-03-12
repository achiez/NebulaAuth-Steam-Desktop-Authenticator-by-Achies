using System;
using System.Collections.Generic;
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

    public async Task AddMafile(string[] path)
    {
        bool? confirmOverwrite = null;
        var added = 0;
        var notAdded = 0;
        var errors = 0;
        var sdaPasswordByDirectory = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var str in path)
        {
            try
            {
                await Storage.AddNewMafile(str, confirmOverwrite ?? false);
                added++;
            }
            catch (FormatException)
            {
                var sdaResult = await TryImportSdaEncryptedMafile(str, sdaPasswordByDirectory, confirmOverwrite);
                if (!sdaResult.Handled)
                {
                    errors++;
                }
                else
                {
                    if (sdaResult.ConfirmOverwrite != null)
                        confirmOverwrite = sdaResult.ConfirmOverwrite;
                    added += sdaResult.Added;
                    notAdded += sdaResult.NotAdded;
                    errors += sdaResult.Errors;
                }
            }
            catch (IOException)
            {
                confirmOverwrite ??=
                    await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmMafileOverwrite"));

                if (confirmOverwrite == true)
                {
                    await Storage.AddNewMafile(str, true);
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

    /// <summary>
    /// Result of trying to import an SDA-encrypted mafile. When Handled is true, apply the counts and optional ConfirmOverwrite to the caller's state.
    /// </summary>
    private sealed class SdaImportResult
    {
        public bool Handled { get; init; }
        public bool? ConfirmOverwrite { get; init; }
        public int Added { get; init; }
        public int NotAdded { get; init; }
        public int Errors { get; init; }
    }

    /// <summary>
    /// Tries to detect SDA-encrypted mafile, prompt for password if needed, decrypt and import.
    /// </summary>
    /// <returns>Result with Handled=true if SDA-encrypted and processed; apply Added/NotAdded/Errors and ConfirmOverwrite (if set) to caller state. Handled=false if not SDA-encrypted.</returns>
    private async Task<SdaImportResult> TryImportSdaEncryptedMafile(
        string mafilePath,
        Dictionary<string, string?> sdaPasswordByDirectory,
        bool? confirmOverwrite)
    {
        var sdaResult = SdaEncryptedMafileDetector.TryDetect(mafilePath);
        if (sdaResult == null)
        {
            // If the user only has an encrypted .mafile (common), ask them to locate SDA manifest.json.
            // Without it, SDA decryption is cryptographically impossible (salt + IV are stored there).
            string encryptedBlob;
            try
            {
                encryptedBlob = await File.ReadAllTextAsync(mafilePath);
            }
            catch
            {
                return new SdaImportResult { Handled = false };
            }

            if (LooksLikeSdaEncryptedBlob(encryptedBlob))
            {
                var proceed = await DialogsController.ShowConfirmCancelDialog(GetLocalization("SdaManifestMissing"));
                if (!proceed)
                    return new SdaImportResult { Handled = true, NotAdded = 1 };

                var manifestPath = DialogsController.PickSdaManifestPath();
                if (string.IsNullOrWhiteSpace(manifestPath))
                    return new SdaImportResult { Handled = true, NotAdded = 1 };

                var manifest = SdaEncryptedMafileDetector.TryReadEncryptedManifestFromPath(manifestPath);
                if (manifest == null)
                    return new SdaImportResult { Handled = true, Errors = 1 };

                sdaResult = SdaEncryptedMafileDetector.TryDetect(mafilePath, manifest);
                if (sdaResult == null)
                {
                    SnackbarController.SendSnackbar(GetLocalization("SdaManifestEntryNotFound"),
                        TimeSpan.FromSeconds(5));
                    return new SdaImportResult { Handled = true, Errors = 1 };
                }
            }
            else
            {
                return new SdaImportResult { Handled = false };
            }
        }

        var dir = Path.GetDirectoryName(mafilePath);
        if (string.IsNullOrEmpty(dir))
            return new SdaImportResult { Handled = false };

        if (!sdaPasswordByDirectory.TryGetValue(dir, out var password))
        {
            password = await DialogsController.ShowSdaPasswordDialog();
            if (string.IsNullOrEmpty(password))
            {
                // User cancelled password prompt -> treat as skipped, not an error.
                return new SdaImportResult { Handled = true, NotAdded = 1 };
            }

            sdaPasswordByDirectory[dir] = password;
        }

        string fileContent;
        try
        {
            fileContent = await File.ReadAllTextAsync(mafilePath);
        }
        catch (Exception ex)
        {
            Shell.Logger.Warn(ex, "Could not read mafile for SDA decrypt");
            SnackbarController.SendSnackbar($"{GetLocalization("MafileImportError")} {Path.GetFileName(mafilePath)}",
                TimeSpan.FromSeconds(3));
            return new SdaImportResult { Handled = true, Errors = 1 };
        }

        var plainText = SDAEncryptor.DecryptData(password, sdaResult.SdaManifestEntry.EncryptionSalt, sdaResult.SdaManifestEntry.EncryptionIv, fileContent);
        if (string.IsNullOrEmpty(plainText))
        {
            SnackbarController.SendSnackbar(GetLocalization("SdaWrongPassword"), TimeSpan.FromSeconds(3));
            sdaPasswordByDirectory.Remove(dir);
            return new SdaImportResult { Handled = true, Errors = 1 };
        }

        Mafile data;
        try
        {
            data = NebulaSerializer.Deserialize(plainText, mafilePath);
        }
        catch (MafileNeedReloginException ex) when (ex.Mafile != null)
        {
            data = ex.Mafile;
        }
        catch (Exception ex)
        {
            Shell.Logger.Warn(ex, "Could not deserialize decrypted SDA mafile");
            SnackbarController.SendSnackbar($"{GetLocalization("MafileImportError")} {Path.GetFileName(mafilePath)}",
                TimeSpan.FromSeconds(3));
            return new SdaImportResult { Handled = true, Errors = 1 };
        }

        try
        {
            await Storage.AddNewMafileFromData(data, confirmOverwrite ?? false);
            return new SdaImportResult { Handled = true, Added = 1 };
        }
        catch (IOException)
        {
            var newOverwrite = await DialogsController.ShowConfirmCancelDialog(GetLocalization("ConfirmMafileOverwrite"));
            if (newOverwrite == true)
            {
                try
                {
                    await Storage.AddNewMafileFromData(data, true);
                    return new SdaImportResult { Handled = true, ConfirmOverwrite = true, Added = 1 };
                }
                catch (Exception ex)
                {
                    Shell.Logger.Warn(ex, "Could not save decrypted mafile");
                    SnackbarController.SendSnackbar($"{GetLocalization("MafileImportError")} {Path.GetFileName(mafilePath)}",
                        TimeSpan.FromSeconds(3));
                    return new SdaImportResult { Handled = true, Errors = 1 };
                }
            }

            if (newOverwrite == false)
                return new SdaImportResult { Handled = true, ConfirmOverwrite = false, NotAdded = 1 };
            return new SdaImportResult { Handled = true, ConfirmOverwrite = newOverwrite };
        }
        catch (Exception ex)
        {
            Shell.Logger.Warn(ex, "Could not save decrypted mafile");
            SnackbarController.SendSnackbar($"{GetLocalization("MafileImportError")} {Path.GetFileName(mafilePath)}",
                TimeSpan.FromSeconds(3));
            return new SdaImportResult { Handled = true, Errors = 1 };
        }
    }

    private static bool LooksLikeSdaEncryptedBlob(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var trimmed = content.Trim();
        // Plain mafile JSON starts with {, SDA encrypted mafile is base64 blob text.
        if (trimmed.StartsWith("{"))
            return false;

        // Cheap heuristic: mostly base64 chars and long enough.
        if (trimmed.Length < 64)
            return false;

        for (var i = 0; i < trimmed.Length; i++)
        {
            var c = trimmed[i];
            var isBase64 =
                (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                (c >= '0' && c <= '9') ||
                c == '+' || c == '/' || c == '=' ||
                c == '\r' || c == '\n';
            if (!isBase64)
                return false;
        }

        return true;
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
}