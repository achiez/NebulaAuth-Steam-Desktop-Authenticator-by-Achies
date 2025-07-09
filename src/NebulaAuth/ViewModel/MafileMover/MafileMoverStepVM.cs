using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NebulaAuth.ViewModel.MafileMover;

public abstract partial class MafileMoverStepVM : ObservableObject
{
    public virtual string? Tip => null;

    [RelayCommand(CanExecute = nameof(CanExecute))]
    public abstract Task Next();

    public abstract bool CanExecute();

    [RelayCommand(CanExecute = nameof(CancelCanExecute))]
    public abstract void Cancel();

    public virtual bool CancelCanExecute()
    {
        return true;
    }
}