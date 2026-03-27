using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NebulaAuth.Core;
using NebulaAuth.Model;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.ViewModel;

public class InventoryItem : ObservableObject
{
    private string _name = string.Empty;
    private decimal _price;
    private int _quantity;
    private string _imageUrl = string.Empty;
    private string _description = string.Empty;
    private string _floatValue = string.Empty;
    private string _paintSeed = string.Empty;
    private string _origin = string.Empty;
    private decimal _steamMarketPrice;
    private int _steamMarketSales;
    private string _rarity = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public string ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string FloatValue
    {
        get => _floatValue;
        set => SetProperty(ref _floatValue, value);
    }

    public string PaintSeed
    {
        get => _paintSeed;
        set => SetProperty(ref _paintSeed, value);
    }

    public string Origin
    {
        get => _origin;
        set => SetProperty(ref _origin, value);
    }

    public decimal SteamMarketPrice
    {
        get => _steamMarketPrice;
        set => SetProperty(ref _steamMarketPrice, value);
    }

    public int SteamMarketSales
    {
        get => _steamMarketSales;
        set => SetProperty(ref _steamMarketSales, value);
    }

    public string Rarity
    {
        get => _rarity;
        set => SetProperty(ref _rarity, value);
    }

    public string AppId { get; set; } = string.Empty;
    public string ContextId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
}

public partial class InventoryVM : ObservableObject
{
    private string _accountName = "Inventory";
    private ObservableCollection<InventoryItem> _inventoryItems = new();
    private bool _isLoading;
    private int _itemsCount;
    private decimal _totalValue;
    private string _selectedSort = "Price (High to Low)";
    private string _statusMessage = "Ready";
    private InventoryItem? _selectedItem;
    private Mafile? _mafile;
    private static readonly HttpClient Client = new();

    public Mafile? Mafile
    {
        get => _mafile;
        set => SetProperty(ref _mafile, value);
    }

    public string AccountName
    {
        get => _accountName;
        set => SetProperty(ref _accountName, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<InventoryItem> InventoryItems
    {
        get => _inventoryItems;
        set => SetProperty(ref _inventoryItems, value);
    }

    public InventoryItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int ItemsCount
    {
        get => _itemsCount;
        set => SetProperty(ref _itemsCount, value);
    }

    public decimal TotalValue
    {
        get => _totalValue;
        set => SetProperty(ref _totalValue, value);
    }

    public string SelectedSort
    {
        get => _selectedSort;
        set => SetProperty(ref _selectedSort, value);
    }

    public ObservableCollection<string> SortOptions { get; } = new()
    {
        "Price (High to Low)",
        "Price (Low to High)",
        "Name (A-Z)",
        "Name (Z-A)",
        "Quantity (Most)"
    };

    public InventoryVM()
    {
    }

    public InventoryVM(Mafile mafile)
    {
        _mafile = mafile;
        AccountName = mafile.AccountName;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        if (_mafile == null) return;
        await LoadInventoryAsync(_mafile);
    }

    public async Task LoadInventoryAsync(Mafile mafile)
    {
        IsLoading = false;
        StatusMessage = "Opening Steam browser...";
        try
        {
            Console.WriteLine("[LoadInventoryAsync] Browser window opened, no API calls needed");
            _mafile = mafile;
            AccountName = mafile.AccountName;
            
            // WebView2 authentication happens in InventoryWindow
            // No API calls or local rendering
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadInventoryAsync] Error: {ex.Message}");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task LoadAppInventory(Mafile mafile, ulong steamId, string appId, string contextId)
    {
        Console.WriteLine($"[LoadAppInventory] Starting for SteamID: {steamId}, App: {appId}, Context: {contextId}");
        
        var items = new ObservableCollection<InventoryItem>();
        
        try
        {
            Console.WriteLine("[LoadAppInventory] Calling MaClient.GetInventoryJson...");
            var content = await MaClient.GetInventoryJson(mafile, steamId, appId, contextId);
            
            Console.WriteLine($"[LoadAppInventory] Content length: {content.Length} bytes");
            
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("[LoadAppInventory] ERROR: Response is empty");
                SnackbarController.SendSnackbar("Inventory is empty or not accessible");
                return;
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            Console.WriteLine($"[LoadAppInventory] JSON parsed successfully");

            if (!root.TryGetProperty("success", out var successProp) || !successProp.GetBoolean())
            {
                Console.WriteLine("[LoadAppInventory] ERROR: success is false");
                SnackbarController.SendSnackbar("Failed to fetch inventory");
                return;
            }

            if (!root.TryGetProperty("rgInventory", out var inventoryElement))
            {
                Console.WriteLine("[LoadAppInventory] ERROR: No 'rgInventory' property");
                SnackbarController.SendSnackbar("No inventory items found");
                return;
            }

            if (!root.TryGetProperty("rgDescriptions", out var descriptionsElement))
            {
                Console.WriteLine("[LoadAppInventory] ERROR: No 'rgDescriptions' property");
                return;
            }

            // Parse descriptions: keys are "classid_instanceid", values contain market_name and icon_url
            var descriptions = new Dictionary<string, (string Name, string ImageUrl, string Description, string Rarity)>();
            
            foreach (var desc in descriptionsElement.EnumerateObject())
            {
                var descObj = desc.Value;
                
                if (descObj.TryGetProperty("market_name", out var marketName) &&
                    descObj.TryGetProperty("icon_url", out var iconUrl))
                {
                    var key = desc.Name; // This is "classid_instanceid"
                    var name = marketName.GetString() ?? "Unknown Item";
                    var image = iconUrl.GetString() ?? "";
                    
                    // Try to get description from type or name
                    var itemDescription = "";
                    if (descObj.TryGetProperty("type", out var typeField))
                    {
                        itemDescription = typeField.GetString() ?? "";
                    }
                    
                    // Get rarity (quality_color or type)
                    var rarity = "";
                    if (descObj.TryGetProperty("type", out var rarityField))
                    {
                        rarity = rarityField.GetString() ?? "";
                    }
                    
                    if (!string.IsNullOrEmpty(key))
                    {
                        descriptions[key] = (
                            name, 
                            $"https://community.cloudflare.steamstatic.com/economy/image/{image}",
                            itemDescription,
                            rarity
                        );
                        Console.WriteLine($"[LoadAppInventory] Parsed description: {key} = {name}");
                    }
                }
            }

            Console.WriteLine($"[LoadAppInventory] Found {descriptions.Count} descriptions");

            // Parse inventory items
            var itemsDict = new Dictionary<string, InventoryItem>();
            var itemCount = 0;
            
            foreach (var item in inventoryElement.EnumerateObject())
            {
                itemCount++;
                var itemObj = item.Value;
                
                if (itemObj.TryGetProperty("classid", out var classId) &&
                    itemObj.TryGetProperty("instanceid", out var instanceId) &&
                    itemObj.TryGetProperty("amount", out var amount))
                {
                    var classIdStr = classId.GetString() ?? "";
                    var instanceIdStr = instanceId.GetString() ?? "";
                    var key = $"{classIdStr}_{instanceIdStr}"; // Match the description key format
                    
                    Console.WriteLine($"[LoadAppInventory] Processing item: {key}");
                    
                    if (descriptions.TryGetValue(key, out var desc))
                    {
                        var amountStr = amount.GetString() ?? "1";
                        var amountInt = int.Parse(amountStr);
                        
                        // Try to extract float and paint seed from itemObj
                        var floatValue = "";
                        var paintSeed = "";
                        var origin = "";
                        
                        // These might be in additional fields - let's check
                        foreach (var prop in itemObj.EnumerateObject())
                        {
                            var propName = prop.Name.ToLower();
                            if (propName.Contains("float"))
                            {
                                floatValue = prop.Value.GetString() ?? "";
                            }
                            if (propName.Contains("seed") || propName.Contains("paintseed"))
                            {
                                paintSeed = prop.Value.GetString() ?? "";
                            }
                        }
                        
                        if (itemsDict.TryGetValue(key, out var existingItem))
                        {
                            existingItem.Quantity += amountInt;
                            Console.WriteLine($"[LoadAppInventory] Updated quantity for {key}: {existingItem.Quantity}");
                        }
                        else
                        {
                            itemsDict[key] = new InventoryItem
                            {
                                Name = desc.Name,
                                ImageUrl = desc.ImageUrl,
                                Quantity = amountInt,
                                Description = desc.Description,
                                Rarity = desc.Rarity,
                                FloatValue = floatValue,
                                PaintSeed = paintSeed,
                                Origin = origin,
                                AppId = appId,
                                ContextId = contextId,
                                ClassId = classIdStr,
                                InstanceId = instanceIdStr,
                                AssetId = item.Name
                            };
                            Console.WriteLine($"[LoadAppInventory] Added new item: {desc.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[LoadAppInventory] WARNING: No description found for key {key}");
                    }
                }
            }

            Console.WriteLine($"[LoadAppInventory] Processed {itemCount} inventory items, created {itemsDict.Count} unique items");

            foreach (var item in itemsDict.Values)
            {
                items.Add(item);
            }

            InventoryItems = items;
            UpdateStatistics();
            
            Console.WriteLine($"[LoadAppInventory] SUCCESS: Loaded {items.Count} items");
            
            if (items.Count == 0)
            {
                SnackbarController.SendSnackbar("No items found in inventory");
            }
            else
            {
                SnackbarController.SendSnackbar($"Loaded {items.Count} items");
                
                // Fetch market prices in background (fast, non-blocking)
                _ = FetchMarketPricesAsync(items);
            }
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"[LoadAppInventory] JSON ERROR: {jsonEx.Message}");
            Console.WriteLine($"[LoadAppInventory] StackTrace: {jsonEx.StackTrace}");
            SnackbarController.SendSnackbar($"Failed to parse inventory data");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"[LoadAppInventory] HTTP ERROR: {httpEx.Message}");
            Console.WriteLine($"[LoadAppInventory] StackTrace: {httpEx.StackTrace}");
            SnackbarController.SendSnackbar($"Network error: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadAppInventory] EXCEPTION: {ex.GetType().Name}");
            Console.WriteLine($"[LoadAppInventory] Message: {ex.Message}");
            Console.WriteLine($"[LoadAppInventory] StackTrace: {ex.StackTrace}");
            SnackbarController.SendSnackbar($"Error loading inventory: {ex.Message}");
        }
    }

    private async Task FetchMarketPricesAsync(ObservableCollection<InventoryItem> items)
    {
        Console.WriteLine($"[FetchMarketPricesAsync] Starting price fetch for {items.Count} items");
        
        // Get unique item names to avoid duplicate requests
        var uniqueNames = items.Select(i => i.Name).Distinct().ToList();
        Console.WriteLine($"[FetchMarketPricesAsync] Found {uniqueNames.Count} unique items");
        
        // Queue of items to fetch
        var toFetch = new Queue<string>(uniqueNames);
        var failedItems = new List<(string itemName, int attemptCount)>();
        int totalSuccess = 0;
        int totalFailed = 0;
        
        // First pass: Sequential requests with reasonable delays
        // This is slower but won't trigger rate limiting
        const int delayBetweenRequests = 800; // 800ms between requests = ~270 items/minute
        
        Console.WriteLine($"[FetchMarketPricesAsync] Starting first pass - sequential fetch with {delayBetweenRequests}ms delay");
        
        while (toFetch.Count > 0)
        {
            var itemName = toFetch.Dequeue();
            
            try
            {
                var success = await FetchSinglePriceAsync(itemName, items);
                
                if (success)
                {
                    totalSuccess++;
                }
                else
                {
                    // Add to failed list for retry
                    failedItems.Add((itemName, 0));
                    totalFailed++;
                    Console.WriteLine($"[FetchMarketPricesAsync] Added to retry queue: {itemName}");
                }
                
                // Delay between requests
                await Task.Delay(delayBetweenRequests);
            }
            catch (Exception ex)
            {
                failedItems.Add((itemName, 0));
                totalFailed++;
                Console.WriteLine($"[FetchMarketPricesAsync] Exception added to retry: {itemName} - {ex.Message}");
                
                // If we hit rate limit, add extra delay before continuing
                if (ex.Message.Contains("429") || ex.Message.Contains("Rate limited"))
                {
                    Console.WriteLine($"[FetchMarketPricesAsync] Rate limit detected, adding 3 second delay");
                    await Task.Delay(3000);
                }
            }
        }
        
        Console.WriteLine($"[FetchMarketPricesAsync] First pass done: {totalSuccess} success, {totalFailed} failed");
        
        // Retry failed items with exponential backoff
        if (failedItems.Count > 0)
        {
            Console.WriteLine($"[FetchMarketPricesAsync] Starting retry phase for {failedItems.Count} items");
            int retryAttempts = 0;
            const int maxRetries = 2;
            
            while (failedItems.Count > 0 && retryAttempts < maxRetries)
            {
                retryAttempts++;
                var itemsToRetry = failedItems.ToList();
                failedItems.Clear();
                
                // Exponential backoff: 2 seconds for first retry, 4 seconds for second
                int delayMs = 2000 * (int)Math.Pow(2, retryAttempts - 1);
                
                Console.WriteLine($"[FetchMarketPricesAsync] Retry attempt {retryAttempts}/{maxRetries} with {delayMs}ms delay between items");
                
                foreach (var (itemName, attemptCount) in itemsToRetry)
                {
                    try
                    {
                        await Task.Delay(delayMs);
                        var success = await FetchSinglePriceAsync(itemName, items);
                        
                        if (success)
                        {
                            totalSuccess++;
                            Console.WriteLine($"[FetchMarketPricesAsync] Retry successful: {itemName}");
                        }
                        else
                        {
                            failedItems.Add((itemName, attemptCount + 1));
                            Console.WriteLine($"[FetchMarketPricesAsync] Retry failed, will retry again: {itemName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedItems.Add((itemName, attemptCount + 1));
                        Console.WriteLine($"[FetchMarketPricesAsync] Retry exception: {itemName} - {ex.Message}");
                        
                        // Extra delay on rate limit
                        if (ex.Message.Contains("429") || ex.Message.Contains("Rate limited"))
                        {
                            Console.WriteLine($"[FetchMarketPricesAsync] Rate limit on retry, adding 5 second delay");
                            await Task.Delay(5000);
                        }
                    }
                }
            }
            
            totalFailed = failedItems.Count;
            if (totalFailed > 0)
            {
                Console.WriteLine($"[FetchMarketPricesAsync] {totalFailed} items still failing after retries");
            }
        }
        
        UpdateStatistics();
        Console.WriteLine($"[FetchMarketPricesAsync] Completed - {totalSuccess}/{uniqueNames.Count} items fetched, {totalFailed} failed. Total inventory value: ${TotalValue:F2}");
    }

    private async Task<bool> FetchSinglePriceAsync(string itemName, ObservableCollection<InventoryItem> items)
    {
        try
        {
            var (price, sales) = await MaClient.GetItemMarketPrice(itemName);
            
            // Update all items with this name
            foreach (var item in items.Where(i => i.Name == itemName))
            {
                item.SteamMarketPrice = price;
                item.SteamMarketSales = sales;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FetchSinglePriceAsync] Exception for {itemName}: {ex.Message}");
            // Don't swallow the exception - let caller know there was an issue
            throw;
        }
    }

    public void UpdateStatistics()
    {
        ItemsCount = InventoryItems.Count;
        TotalValue = 0;
        foreach (var item in InventoryItems)
        {
            TotalValue += item.SteamMarketPrice * item.Quantity;
        }
    }
}
