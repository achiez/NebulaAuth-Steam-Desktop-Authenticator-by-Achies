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

namespace NebulaAuth.ViewModel;

public partial class MainVM //File //TODO: Refactor
{

    public Settings Settings => Settings.Instance;

    [RelayCommand]
    private void OpenMafileFolder()
    { var mafile = SelectedMafile;
     
        var path = Storage.MafileFolder;
        if (mafile?.SessionData != null)
        {
            var mafPath = Path.Combine(Storage.MafileFolder, mafile.SessionData.SteamId.Steam64 + ".maFile");
            if (File.Exists(mafPath))
            {
                path = $"/e, /select, \"{mafPath}\""; ;
            }
        }

        Process.Start("explorer", path);
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
        return AddMafile(new[] { path });

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
                confirmOverwrite ??= await DialogsController.ShowConfirmCancelDialog(GetLocalizationOrDefault("ConfirmMafileOverwrite"));

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
                    SnackbarController.SendSnackbar($"{GetLocalizationOrDefault("MafileImportError")} {Path.GetFileName(str)}{GetLocalizationOrDefault("MissingSessionInMafile")}", TimeSpan.FromSeconds(4));
                }
            }
        }

        var msg = GetLocalizationOrDefault("Import");
        if (added > 0)
        {
            msg += $" {GetLocalizationOrDefault("ImportAdded")} {added}.";
        }
        if (notAdded > 0)
        {
            msg += $" {GetLocalizationOrDefault("ImportSkipped")} {notAdded}.";
        }

        if (errors > 0)
        {
            msg += $" {GetLocalizationOrDefault("ImportErrors")} {errors}.";
        }
        SnackbarController.SendSnackbar(msg, TimeSpan.FromSeconds(2));
    }

    private async Task<bool> HandleAddMafileWithoutSession(Mafile data)
    {
        var oldMafile = SelectedMafile;
        SelectedMafile = data;
        await LoginAgain();
        var result = data.SessionData != null;
        if (result)
        {
            var existed = MaFiles.FirstOrDefault(m => m.AccountName == data.AccountName); //TODO: more elegant way to handle overwrite
            if (existed != null)
            {
                MaFiles.Remove(existed);
            }
            MaFiles.Add(data);
            SelectedMafile = data;
        }
        else
        {
            SelectedMafile = oldMafile;
        }
        return result;

    }

    [RelayCommand]
    private async Task RemoveMafile()
    {
        if (SelectedMafile == null) return;
        var confirm =
            await DialogsController.ShowConfirmCancelDialog(GetLocalizationOrDefault("RemoveMafileConfirmation"));

        if (!confirm) return;
        try
        {
            Storage.MoveToRemoved(SelectedMafile);
        }
        catch (UnauthorizedAccessException)
        {
            SnackbarController.SendSnackbar(GetLocalizationOrDefault("CantRemoveAlreadyExist"));
        }
        catch (Exception ex)
        {
            SnackbarController.SendSnackbar(
                $"{GetLocalizationOrDefault("CantRemoveMafile")} {ex.Message}");
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
    private async Task CopyMafileFromBuffer()
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
}