using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;
using ProtoBuf;
using SteamLib.Api.Services;
using SteamLib.Authentication;
using SteamLib.Exceptions.Authorization;
using SteamLib.ProtoCore.Services;

namespace NebulaAuth.Helpers;

/// <summary>
/// Automatically approves Steam QR-code login inside the embedded WebView2.
///
/// Flow:
///   1. The login page JS calls Steam's BeginAuthSessionViaQR API.
///   2. We intercept the HTTP response in C# via WebResourceResponseReceived
///      and deserialize the protobuf to extract the clientId.
///   3. With the clientId + mafile's SharedSecret + SteamId we call
///      UpdateAuthSessionWithMobileConfirmation — exactly what the Steam Mobile App does.
///   4. The browser detects the server-side approval and redirects to the inventory.
/// </summary>
public class SteamQRCodeAuthenticator
{
    private CoreWebView2? _coreWebView;
    private Mafile? _mafile;
    private CancellationTokenSource? _cts;
    private bool _approvalInProgress;
    private bool _approved;
    private bool _redirected;

    public event EventHandler<AuthStatusEventArgs>? StatusChanged;

    public Task InitializeAsync(CoreWebView2 coreWebView, Mafile mafile)
    {
        _coreWebView = coreWebView;
        _mafile = mafile;
        _cts = new CancellationTokenSource();

        _coreWebView.WebResourceResponseReceived += OnWebResourceResponseReceived;
        _coreWebView.NavigationCompleted += OnNavigationCompleted;

        var url = "https://steamcommunity.com/login/home/?goto=%2Fmy%2Finventory%2F";
        _coreWebView.Navigate(url);

        StatusChanged?.Invoke(this, new AuthStatusEventArgs
        {
            Status = "Waiting for Steam login page...",
            IsLoading = true
        });

        return Task.CompletedTask;
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess || _coreWebView == null) return;

        var uri = _coreWebView.Source;

        if (uri.Contains("/inventory", StringComparison.OrdinalIgnoreCase) &&
            !uri.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            StatusChanged?.Invoke(this, new AuthStatusEventArgs
            {
                Status = "Login successful! Viewing inventory.",
                IsLoading = false
            });
        }
        else if (uri.Contains("login", StringComparison.OrdinalIgnoreCase) && !_approved)
        {
            StatusChanged?.Invoke(this, new AuthStatusEventArgs
            {
                Status = "Steam login page loaded. Waiting for QR session...",
                IsLoading = true
            });
        }
    }

    private async void OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        try
        {
            var requestUri = e.Request.Uri;

            if (requestUri.Contains("BeginAuthSessionViaQR", StringComparison.OrdinalIgnoreCase))
            {
                await HandleQRSessionResponseAsync(e);
            }
            // After approval, detect when cookies are set so we can navigate immediately
            else if (_approved && !_redirected && requestUri.Contains("finalizelogin", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Response.StatusCode == 200)
                {
                    StatusChanged?.Invoke(this, new AuthStatusEventArgs
                    {
                        Status = "Session established! Loading inventory...",
                        IsLoading = true
                    });
                }
            }
            else if (_approved && !_redirected && requestUri.Contains("/settoken", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Response.StatusCode == 200)
                {
                    _redirected = true;
                    // Small delay to let all cookie transfers complete
                    await Task.Delay(500);
                    _coreWebView?.Navigate("https://steamcommunity.com/my/inventory/");
                }
            }
        }
        catch (Exception)
        {
            // Response interception error handled silently
        }
    }

    private async Task HandleQRSessionResponseAsync(CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        if (_approvalInProgress || _approved) return;

        var statusCode = e.Response.StatusCode;

        if (statusCode != 200)
        {
            return;
        }

        try
        {
            using var stream = await e.Response.GetContentAsync();
            if (stream == null)
            {
                return;
            }

            // Copy to MemoryStream for reliable deserialization
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            if (ms.Length == 0)
            {
                return;
            }

            ms.Position = 0;

            var qrResponse = Serializer.Deserialize<BeginAuthSessionViaQR_Response>(ms);
            if (qrResponse == null || qrResponse.ClientId == 0)
            {
                return;
            }

            _approvalInProgress = true;
            StatusChanged?.Invoke(this, new AuthStatusEventArgs
            {
                Status = "QR session captured! Approving with authenticator...",
                IsLoading = true
            });

            _ = ApproveQRSessionAsync(qrResponse.ClientId);
        }
        catch (Exception)
        {
            // Error parsing QR protobuf handled silently
        }
    }

    private async Task ApproveQRSessionAsync(ulong clientId)
    {
        if (_mafile?.SharedSecret == null || _mafile.SessionData == null)
        {
            ReportError("Authenticator data missing. Ensure the account is set up in the main app.");
            _approvalInProgress = false;
            return;
        }

        try
        {
            var token = _mafile.SessionData.GetMobileToken();
            if (token == null || token.Value.IsExpired)
            {
                ReportError("Mobile session expired. Please refresh session in main app.");
                _approvalInProgress = false;
                return;
            }

            var accessToken = token.Value.Token;
            var steamId = _mafile.SessionData.SteamId;

            StatusChanged?.Invoke(this, new AuthStatusEventArgs
            {
                Status = "Verifying login session...",
                IsLoading = true
            });

            var httpClient = MaClient.GetHttpClientHandlerPair(_mafile).Client;

            var sessionInfo = await AuthenticationServiceApi.GetAuthSessionInfo(
                httpClient, accessToken, clientId);

            var confirmReq = AuthRequestHelper.CreateMobileConfirmationRequest(
                1, clientId, steamId.Steam64, _mafile.SharedSecret);

            StatusChanged?.Invoke(this, new AuthStatusEventArgs
            {
                Status = $"Approving login from {sessionInfo.Country} ({sessionInfo.IP})...",
                IsLoading = true
            });

            await AuthenticationServiceApi.UpdateAuthSessionWithMobileConfirmation(
                httpClient, accessToken, confirmReq);

            _approved = true;
            StatusChanged?.Invoke(this, new AuthStatusEventArgs
            {
                Status = $"Login approved! ({sessionInfo.Country}, {sessionInfo.IP}) Waiting for redirect...",
                IsLoading = true
            });

            // Do NOT navigate away — the browser's own JS polling loop will:
            //   1. Call PollAuthSessionStatus and receive refresh_token + access_token
            //   2. Call finalizelogin to set session cookies
            //   3. Redirect to the inventory page
            // Forcing navigation kills that flow since the browser has no cookies yet.
        }
        catch (SessionPermanentlyExpiredException)
        {
            ReportError("Session expired. Please re-authenticate in the main app.");
        }
        catch (SessionInvalidException)
        {
            ReportError("Session invalid. Please log in again in the main app.");
        }
        catch (Exception ex)
        {
            ReportError($"Error: {ex.Message}");
        }
        finally
        {
            if (!_approved)
                _approvalInProgress = false;
        }
    }

    private void ReportError(string message)
    {
        StatusChanged?.Invoke(this, new AuthStatusEventArgs
        {
            Status = message,
            IsLoading = false,
            IsError = true
        });
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        if (_coreWebView != null)
        {
            _coreWebView.WebResourceResponseReceived -= OnWebResourceResponseReceived;
            _coreWebView.NavigationCompleted -= OnNavigationCompleted;
        }
    }
}

public class AuthStatusEventArgs : EventArgs
{
    public string Status { get; set; } = string.Empty;
    public bool IsLoading { get; set; }
    public bool IsError { get; set; }
}
