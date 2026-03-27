using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AchiesUtilities.Web.Models;
using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Entities;
using SteamLib.Api.Mobile;
using SteamLib.Api.Services;
using SteamLib.Api.Trade;
using SteamLib.Authentication;
using SteamLib.Exceptions.Authorization;
using SteamLib.ProtoCore.Services;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.Web;

namespace NebulaAuth.Model;

public static class MaClient
{
    private static HttpClientHandler ClientHandler { get; }
    private static HttpClient Client { get; }
    private static DynamicProxy Proxy { get; }
    public static ProxyData? DefaultProxy { get; set; }


    static MaClient()
    {
        Proxy = new DynamicProxy();
        var pair = ClientBuilder.BuildMobileClient(Proxy, null);
        Client = pair.Client;
        ClientHandler = pair.Handler;
    }


    public static void SetAccount(Mafile? account)
    {
        ClientHandler.CookieContainer.ClearAllCookies();
        if (account == null) return;
        ClientHandler.CookieContainer.SetSteamMobileCookiesWithMobileToken(account.SessionData);
        Proxy.SetData(account.Proxy?.Data);
    }

    public static Task<IEnumerable<Confirmation>> GetConfirmations(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        return SteamMobileConfirmationsApi.GetConfirmations(Client, mafile, mafile.SessionData!.SteamId);
    }

    public static Task LoginAgain(Mafile mafile, string password, bool savePassword)
    {
        SetProxy(mafile);
        return SessionHandler.LoginAgain(new HttpClientHandlerPair(Client, ClientHandler), mafile, password,
            savePassword);
    }


    public static Task RefreshSession(Mafile mafile)
    {
        ValidateMafile(mafile, true);
        SetProxy(mafile);
        return SessionHandler.RefreshMobileToken(new HttpClientHandlerPair(Client, ClientHandler), mafile);
    }

    public static async Task<bool> SendConfirmation(Mafile mafile, Confirmation confirmation, bool confirm)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        var res = await SteamMobileConfirmationsApi.SendConfirmation(Client, confirmation, mafile.SessionData!.SteamId,
            mafile,
            confirm);

        if (!res && confirmation.ConfType == ConfirmationType.Trade)
        {
            Shell.Logger.Warn("Failed to send trade confirmation for {accountName}. Sending ack", mafile.AccountName);
            await SteamTradeApi.Acknowledge(Client, mafile.SessionData.SessionId);
            await Task.Delay(10);
        }
        else
        {
            return res;
        }

        return await SteamMobileConfirmationsApi.SendConfirmation(Client, confirmation, mafile.SessionData!.SteamId,
            mafile,
            confirm);
    }

    public static async Task<bool> SendMultipleConfirmation(Mafile mafile, IEnumerable<Confirmation> confirmations,
        bool confirm)
    {
        var enumerable = confirmations.ToList();
        if (enumerable.Count == 0)
        {
            return false;
        }

        ValidateMafile(mafile);
        SetProxy(mafile);

        var res = await SteamMobileConfirmationsApi.SendMultipleConfirmations(Client, enumerable,
            mafile.SessionData!.SteamId,
            mafile, confirm);
        if (!res && enumerable.Any(c => c.ConfType == ConfirmationType.Trade))
        {
            Shell.Logger.Warn("Failed to send trade confirmations for {accountName}. Sending ack", mafile.AccountName);
            await SteamTradeApi.Acknowledge(Client, mafile.SessionData.SessionId);
            await Task.Delay(10);
        }
        else
        {
            return res;
        }

        return await SteamMobileConfirmationsApi.SendMultipleConfirmations(Client, enumerable,
            mafile.SessionData!.SteamId,
            mafile, confirm);
    }

    public static Task<RemoveAuthenticator_Response> RemoveAuthenticator(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        if (mafile.RevocationCode == null)
        {
            throw new InvalidOperationException("This mafile does not have R-Code");
        }

        var token = mafile.SessionData!.GetMobileToken()!;
        return SteamMobileApi.RemoveAuthenticator(Client, token.Value.Token, mafile.RevocationCode);
    }

    private static void SetProxy(Mafile mafile)
    {
        Proxy.SetData(mafile.Proxy?.Data ?? DefaultProxy);
    }

    private static void ValidateMafile(Mafile mafile, bool ignoreAccessToken = false)
    {
        if (mafile.SessionData == null) throw new SessionInvalidException();
        if (mafile.SessionData.RefreshToken.IsExpired)
            throw new SessionPermanentlyExpiredException();

        if (!ignoreAccessToken)
        {
            var access = mafile.SessionData.GetMobileToken();
            if (access == null || access.Value.IsExpired)
                throw new SessionPermanentlyExpiredException();
        }
    }

    public static async Task<LoginConfirmationResult> ConfirmLoginRequest(Mafile mafile)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);
        var token = mafile.SessionData!.GetMobileToken()!.Value;
        var sessions = await AuthenticationServiceApi.GetAuthSessionsForAccount(Client, token.Token);

        if (sessions.ClientIds.Count == 0)
        {
            return new LoginConfirmationResult
            {
                Error = LoginConfirmationError.NoRequests
            };
        }

        if (sessions.ClientIds.Count > 1)
        {
            return new LoginConfirmationResult
            {
                Error = LoginConfirmationError.MoreThanOneRequest
            };
        }

        var clientId = sessions.ClientIds.Single();
        var clientInfo = await AuthenticationServiceApi.GetAuthSessionInfo(Client, token.Token, clientId);
        var updateReq =
            AuthRequestHelper.CreateMobileConfirmationRequest(1, clientId, mafile.SessionData.SteamId.Steam64,
                mafile.SharedSecret);
        await AuthenticationServiceApi.UpdateAuthSessionWithMobileConfirmation(Client, token.Token, updateReq);
        return new LoginConfirmationResult
        {
            Country = clientInfo.Country,
            IP = clientInfo.IP,
            Success = true
        };
    }

    public static HttpClientHandlerPair GetHttpClientHandlerPair(Mafile mafile)
    {
        SetProxy(mafile);
        return new HttpClientHandlerPair(Client, ClientHandler);
    }

    public static async Task<string> GetInventoryJson(Mafile mafile, ulong steamId, string appId, string contextId)
    {
        ValidateMafile(mafile);
        SetProxy(mafile);

        // Попробуем несколько вариантов URL
        var urls = new[]
        {
            $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count=5000",
            $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?count=5000",
            $"https://steamcommunity.com/profiles/{steamId}/inventory/json/{appId}/{contextId}",
            $"https://api.steampowered.com/IEconService/GetInventoryItemsWithDescriptions/v1/?format=json&appid={appId}&steamid={steamId}"
        };

        Console.WriteLine($"[MaClient.GetInventoryJson] Trying {urls.Length} different URLs...");
        
        foreach (var url in urls)
        {
            Console.WriteLine($"[MaClient.GetInventoryJson] Attempt: {url}");
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                request.Headers.Add("Referer", $"https://steamcommunity.com/profiles/{steamId}/inventory/");

                var response = await Client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[MaClient.GetInventoryJson] Status: {response.StatusCode}, Length: {content.Length}");
                Console.WriteLine($"[MaClient.GetInventoryJson] Preview: {content.Substring(0, Math.Min(300, content.Length))}");

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content) && content != "null")
                {
                    Console.WriteLine($"[MaClient.GetInventoryJson] SUCCESS with URL: {url}");
                    return content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MaClient.GetInventoryJson] URL failed: {ex.Message}");
            }
        }

        throw new Exception("All inventory API URLs failed. Inventory may be private or inaccessible.");
    }

    public static async Task<(decimal Price, int Sales)> GetItemMarketPrice(string marketHashName, string appId = "730")
    {
        try
        {
            Console.WriteLine($"[MaClient.GetItemMarketPrice] Fetching price for: {marketHashName}");
            
            var url = $"https://steamcommunity.com/market/priceoverview/?appid={appId}&market_hash_name={Uri.EscapeDataString(marketHashName)}&country=US";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json");

            var response = await Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Throw on rate limit so retry logic can handle it
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"[MaClient.GetItemMarketPrice] Rate limited (429) for {marketHashName}");
                throw new HttpRequestException("Rate limited by Steam API (429)");
            }

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(content))
            {
                Console.WriteLine($"[MaClient.GetItemMarketPrice] Failed to get price, status: {response.StatusCode}");
                return (0, 0);
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (!root.TryGetProperty("success", out var success) || !success.GetBoolean())
            {
                Console.WriteLine($"[MaClient.GetItemMarketPrice] API returned success=false for {marketHashName}");
                return (0, 0);
            }

            decimal price = 0;
            int sales = 0;

            if (root.TryGetProperty("lowest_price", out var lowestPrice))
            {
                var priceStr = lowestPrice.GetString() ?? "";
                // Price format: "$X.XX" or "X.XX", need to parse number
                priceStr = priceStr.Replace("$", "").Trim();
                if (decimal.TryParse(priceStr, out var parsedPrice))
                {
                    price = parsedPrice;
                }
            }

            if (root.TryGetProperty("volume", out var volumeElem))
            {
                var volumeStr = volumeElem.GetString() ?? "";
                // Volume format: "123" or "1,234", remove commas
                volumeStr = volumeStr.Replace(",", "");
                if (int.TryParse(volumeStr, out var parsedVolume))
                {
                    sales = parsedVolume;
                }
            }

            Console.WriteLine($"[MaClient.GetItemMarketPrice] Got price: ${price}, sales: {sales}");
            return (price, sales);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            // Re-throw rate limit errors so retry logic handles them
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaClient.GetItemMarketPrice] Exception: {ex.Message}");
            return (0, 0);
        }
    }
}