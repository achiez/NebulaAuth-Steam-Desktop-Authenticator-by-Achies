using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;

namespace NebulaAuth.ViewModel.Linker;

public partial class LinkAccountEmailAuthStepVM : LinkAccountStepVM
{
    public override string? Tip { get; }

    private readonly TaskCompletionSource<string> _tcs = new();

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string _emailCode = string.Empty;

    private bool _executing;

    public LinkAccountEmailAuthStepVM()
    {
        Tip = LocManager.GetCodeBehindOrDefault("EnterEmailCode", LinkAccountVM.LOCALIZATION_KEY, "EnterEmailCode");
    }

    public Task<string> GetResultAsync()
    {
        return _tcs.Task;
    }

    public override Task Next()
    {
        _tcs.TrySetResult(EmailCode);
        _executing = true;
        NextCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    public override bool CanExecute()
    {
        return !string.IsNullOrWhiteSpace(EmailCode) && EmailCode.Length == 5 && !_executing;
    }

    public override void Cancel()
    {
        _tcs.TrySetCanceled();
    }
}