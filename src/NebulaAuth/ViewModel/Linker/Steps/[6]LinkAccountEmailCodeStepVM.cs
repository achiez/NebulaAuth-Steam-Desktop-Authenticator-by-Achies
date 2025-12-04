using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Services;

namespace NebulaAuth.ViewModel.Linker;

public partial class LinkAccountEmailCodeStepVM : LinkAccountStepVM
{
    public override string? Tip { get; }

    private readonly TaskCompletionSource<string> _tcs = new();
    private readonly string? _email;
    private CancellationTokenSource? _autoFetchCts;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string _emailCode = string.Empty;

    [ObservableProperty]
    private bool _isAutoFetching;

    [ObservableProperty]
    private string? _autoFetchStatus;

    private bool _executing;

    public LinkAccountEmailCodeStepVM(string? email = null)
    {
        _email = email;
        Tip = LocManager.GetCodeBehindOrDefault("EnterEmailCode", LinkAccountVM.LOCALIZATION_KEY, "EnterEmailCode");
        
        if (!string.IsNullOrWhiteSpace(_email))
        {
            _ = Task.Run(async () => await TryAutoFetchCodeAsync());
        }
    }

    private async Task TryAutoFetchCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(_email))
            return;

        var emailAccount = EmailStorage.GetEmailAccount(_email);
        if (emailAccount == null)
            return;

        _autoFetchCts = new CancellationTokenSource();
        IsAutoFetching = true;
        AutoFetchStatus = LocManager.GetCodeBehindOrDefault("FetchingCodeFromEmail", LinkAccountVM.LOCALIZATION_KEY, "Fetching code from email...");

        try
        {
            var code = await EmailCodeParser.GetSteamCodeFromEmailAsync(emailAccount, 60, _autoFetchCts.Token);
            if (!string.IsNullOrEmpty(code))
            {
                EmailCode = code;
                AutoFetchStatus = LocManager.GetCodeBehindOrDefault("CodeFetched", LinkAccountVM.LOCALIZATION_KEY, "Code fetched!");
                await Task.Delay(500, _autoFetchCts.Token);
                Next();
            }
            else
            {
                AutoFetchStatus = LocManager.GetCodeBehindOrDefault("CodeNotFound", LinkAccountVM.LOCALIZATION_KEY, "Code not found. Please enter manually.");
            }
        }
        catch (TaskCanceledException)
        {
            AutoFetchStatus = LocManager.GetCodeBehindOrDefault("AutoFetchCancelled", LinkAccountVM.LOCALIZATION_KEY, "Auto-fetch cancelled.");
        }
        catch (System.Exception ex)
        {
            Shell.Logger.Error(ex, "Error auto-fetching email code");
            AutoFetchStatus = LocManager.GetCodeBehindOrDefault("AutoFetchError", LinkAccountVM.LOCALIZATION_KEY, "Error fetching code. Please enter manually.");
        }
        finally
        {
            IsAutoFetching = false;
        }
    }

    public Task<string> GetResultAsync()
    {
        return _tcs.Task;
    }

    public override Task Next()
    {
        _autoFetchCts?.Cancel();
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
        _autoFetchCts?.Cancel();
        _tcs.TrySetCanceled();
    }
}