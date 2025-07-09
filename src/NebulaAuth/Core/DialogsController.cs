﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.View;
using NebulaAuth.View.Dialogs;
using NebulaAuth.ViewModel.Linker;
using NebulaAuth.ViewModel.MafileMover;
using NebulaAuth.ViewModel.Other;

namespace NebulaAuth.Core;

public static class DialogsController
{
    #region CommonDialogs

    public static async Task<bool> ShowConfirmCancelDialog(string? msg = null)
    {
        var content = msg == null ? new ConfirmCancelDialog() : new ConfirmCancelDialog(msg);

        var result = await DialogHost.Show(content);
        return result != null && (bool) result;
    }

    //public static async Task<string?> ShowTextFieldDialog(string? msg = null)
    //{
    //    var content = msg == null ? new TextFieldDialog() : new TextFieldDialog(msg);
    //    var result = await DialogHost.Show(content);
    //    return result as string;
    //}

    #endregion

    public static async Task<LoginAgainVM?> ShowLoginAgainDialog(string username, string? currentPassword = null)
    {
        var vm = new LoginAgainVM
        {
            UserName = username,
            Password = currentPassword ?? string.Empty,
            SavePassword = PHandler.IsPasswordSet
        };
        var content = new LoginAgainDialog
        {
            DataContext = vm
        };
        var result = await DialogHost.Show(content);
        if (result is true)
        {
            return vm;
        }

        return null;
    }

    public static async Task<LoginAgainOnImportVM?> ShowLoginAgainOnImportDialog(Mafile mafile,
        IEnumerable<MaProxy> proxies)
    {
        var vm = new LoginAgainOnImportVM(mafile, proxies);
        var content = new LoginAgainOnImportDialog
        {
            DataContext = vm
        };
        var result = await DialogHost.Show(content);
        if (result is true)
        {
            return vm;
        }

        return null;
    }

    public static async Task<bool> ShowProxyManager()
    {
        var vm = new ProxyManagerVM();
        var view = new ProxyManagerView
        {
            DataContext = vm
        };
        await DialogHost.Show(view);
        return vm.AnyChanges;
    }

    public static void CloseDialog()
    {
        DialogHost.Close(null);
    }

    public static async Task ShowLinkerDialog()
    {
        LinkAccountVM? vm = null;
        try
        {
            vm = new LinkAccountVM();
            var view = new LinkerView
            {
                DataContext = vm
            };
            await DialogHost.Show(view);
        }
        finally
        {
            vm?.Dispose();
        }
    }

    public static async Task ShowMafileMoverDialog()
    {
        MafileMoverVM? vm = null;
        try
        {
            vm = new MafileMoverVM();
            var view = new MafileMoverView
            {
                DataContext = vm
            };
            await DialogHost.Show(view);
        }
        finally
        {
            vm?.Dispose();
        }
    }
}