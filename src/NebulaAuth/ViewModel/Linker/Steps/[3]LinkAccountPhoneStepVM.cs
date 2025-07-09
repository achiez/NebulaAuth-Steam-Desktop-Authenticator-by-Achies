using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;

namespace NebulaAuth.ViewModel.Linker;

public partial class LinkAccountPhoneStepVM : LinkAccountStepVM
{
    public override string? Tip { get; }

    private readonly TaskCompletionSource<long?> _tcs = new();
    private bool _isExecuting;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string? _phoneNumber = string.Empty;

    public LinkAccountPhoneStepVM()
    {
        var tipStr =
            LocManager.GetCodeBehindOrDefault("EnterPhoneNumber", LinkAccountVM.LOCALIZATION_KEY, "EnterPhoneNumber");
        Tip = string.Format(tipStr, PhoneNumber);
    }


    public Task<long?> GetResultAsync()
    {
        return _tcs.Task;
    }


    public override Task Next()
    {
        if (string.IsNullOrEmpty(PhoneNumber))
        {
            _tcs.TrySetResult(null);
            return Task.CompletedTask;
        }

        var res = long.Parse(PhoneNumber);
        _tcs.TrySetResult(res);
        _isExecuting = true;
        NextCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    public override bool CanExecute()
    {
        return string.IsNullOrEmpty(PhoneNumber) ||
               (PhoneNumber.Length >= 5 && long.TryParse(PhoneNumber, out _) && !_isExecuting);
    }

    public override void Cancel()
    {
        _tcs.TrySetCanceled();
    }
}