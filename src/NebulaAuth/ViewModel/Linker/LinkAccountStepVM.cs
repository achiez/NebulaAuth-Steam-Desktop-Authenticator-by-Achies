using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NebulaAuth.ViewModel.Linker;

public abstract partial class LinkAccountStepVM : ObservableObject
{
    public virtual string? Tip => null;

    [RelayCommand(CanExecute = nameof(CanExecute))]
    public abstract Task Next();

    public abstract bool CanExecute();

    [RelayCommand]
    public abstract void Cancel();
}