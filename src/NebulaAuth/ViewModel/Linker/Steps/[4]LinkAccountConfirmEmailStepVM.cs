using System.Threading.Tasks;
using NebulaAuth.Core;

namespace NebulaAuth.ViewModel.Linker;

public class LinkAccountConfirmEmailStepVM : LinkAccountStepVM
{
    public override string? Tip { get; }
    private readonly TaskCompletionSource _tcs = new();
    private bool _isExecuting;


    public LinkAccountConfirmEmailStepVM(bool retry)
    {
        if (retry)
        {
            Tip = LocManager.GetCodeBehindOrDefault("ConfirmEmailLink", LinkAccountVM.LOCALIZATION_KEY,
                "ClickOnEmailLinkRetry");
        }
        else
        {
            Tip = LocManager.GetCodeBehindOrDefault("ConfirmEmailLink", LinkAccountVM.LOCALIZATION_KEY,
                "ClickOnEmailLink");
        }
    }

    public Task GetResultAsync()
    {
        return _tcs.Task;
    }


    public override Task Next()
    {
        _tcs.TrySetResult();
        _isExecuting = true;
        NextCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    public override bool CanExecute()
    {
        return !_isExecuting;
    }

    public override void Cancel()
    {
        _tcs.TrySetCanceled();
    }
}