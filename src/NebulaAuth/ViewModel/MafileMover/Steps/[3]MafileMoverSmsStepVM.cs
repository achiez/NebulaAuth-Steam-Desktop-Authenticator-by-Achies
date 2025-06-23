using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;

namespace NebulaAuth.ViewModel.MafileMover;

public partial class MafileMoverSmsStepVM : MafileMoverStepVM
{
    public override string? Tip { get; }
    private readonly TaskCompletionSource<int> _tcs = new();
    private bool _isExecuting;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string _smsCode = string.Empty;

    public MafileMoverSmsStepVM()
    {
        var tipStr =
            LocManager.GetCodeBehindOrDefault("SmsCodeStepTip", MafileMoverVM.LOCALIZATION_KEY, "SmsCodeStepTip");
        Tip = tipStr;
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

    public override bool CancelCanExecute()
    {
        return !_isExecuting;
    }
}