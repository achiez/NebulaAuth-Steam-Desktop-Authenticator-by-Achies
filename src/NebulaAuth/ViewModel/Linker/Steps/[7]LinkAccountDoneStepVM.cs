using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Utility;

namespace NebulaAuth.ViewModel.Linker;

public partial class LinkAccountDoneStepVM : LinkAccountStepVM
{
    public string? InnerTip { get; }
    private readonly TaskCompletionSource _doneTcs = new();
    private readonly string _rCode;

    public LinkAccountDoneStepVM(string rCode, string fileName)
    {
        var tipStr = LocManager.GetCodeBehindOrDefault("MafileLinked", LinkAccountVM.LOCALIZATION_KEY, "MafileLinked");
        InnerTip = string.Format(tipStr, rCode, fileName);
        _rCode = rCode;
    }

    public Task GetResultAsync()
    {
        return _doneTcs.Task;
    }

    public override Task Next()
    {
        _doneTcs.TrySetResult();
        return Task.CompletedTask;
    }

    public override bool CanExecute()
    {
        return true;
    }

    public override void Cancel()
    {
        _doneTcs.TrySetResult();
    }

    [RelayCommand]
    private void CopyCode()
    {
        ClipboardHelper.Set(_rCode);
    }
}