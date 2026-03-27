using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.View;
using NebulaAuth.Utility;

namespace NebulaAuth.ViewModel;

public partial class MainVM //Inventory
{
    [RelayCommand]
    private async Task OpenInventory(object? parameter)
    {
        var mafile = parameter as Mafile;
        
        if (mafile == null)
        {
            SnackbarController.SendSnackbar("Please select an account");
            return;
        }

        try
        {
            var inventoryVm = new InventoryVM(mafile);
            var inventoryWindow = new InventoryWindow(inventoryVm);
            
            // Set as owned window to keep it on top of main window
            inventoryWindow.Owner = Application.Current.MainWindow;
            
            inventoryWindow.Show();

            await inventoryVm.LoadInventoryAsync(mafile);
        }
        catch (Exception ex)
        {
            if (ExceptionHandler.Handle(ex))
            {
                SnackbarController.SendSnackbar($"Failed to open inventory: {ex.Message}");
            }
        }
    }
}
