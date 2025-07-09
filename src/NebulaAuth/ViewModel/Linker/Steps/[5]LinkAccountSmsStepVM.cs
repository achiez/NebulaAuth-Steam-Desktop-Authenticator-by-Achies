using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;

namespace NebulaAuth.ViewModel.Linker;

public partial class LinkAccountSmsStepVM : LinkAccountStepVM
{
    public override string? Tip { get; }
    private readonly TaskCompletionSource<int> _tcs = new();
    private bool _isExecuting;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string _smsCode = string.Empty;

    public LinkAccountSmsStepVM(string? phoneTip)
    {
        var tipStr = LocManager.GetCodeBehindOrDefault("PhoneHint", LinkAccountVM.LOCALIZATION_KEY, "PhoneHint");
        Tip = string.Format(tipStr, phoneTip);
    }

    public Task<int> GetResultAsync()
    {
        return _tcs.Task;
    }

    public override Task Next()
    {
        var res = int.Parse(SmsCode);
        _tcs.TrySetResult(res);
        _isExecuting = true;
        NextCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    public override bool CanExecute()
    {
        return !string.IsNullOrWhiteSpace(SmsCode)
               && int.TryParse(SmsCode, out var sms)
               && sms.ToString("D5").Length == 5
               && !_isExecuting;
    }

    public override void Cancel()
    {
        _tcs.TrySetCanceled();
    }
}