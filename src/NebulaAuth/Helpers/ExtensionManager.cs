using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace NebulaAuth.Helpers;

/// <summary>
/// Manages browser extension downloading, extraction, and loading for WebView2.
/// Extensions are stored in %LocalAppData%/NebulaAuth/Extensions/ so they persist
/// across app updates. CRX files from Chrome Web Store are unpacked into folders
/// that WebView2's AddBrowserExtensionAsync can load.
/// </summary>
public static class ExtensionManager
{
    private const string SteamInventoryHelperChromeId = "cmeakgjggjdlcpncigglobpjbkabhmjl";
    private const string SteamInventoryHelperFolder = "SteamInventoryHelper";

    private static readonly string ExtensionsBaseDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NebulaAuth", "Extensions");

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    /// <summary>
    /// Ensures Steam Inventory Helper is downloaded and extracted, then loads
    /// all extensions from the shared extensions directory into the given profile.
    /// </summary>
    public static async Task EnsureAndLoadExtensionsAsync(CoreWebView2 coreWebView, Action<string>? onStatus = null)
    {
        // Download Steam Inventory Helper if not present
        await EnsureExtensionDownloadedAsync(
            SteamInventoryHelperChromeId,
            SteamInventoryHelperFolder,
            "Steam Inventory Helper",
            onStatus);

        // Load all extensions into the profile
        await LoadAllExtensionsAsync(coreWebView, onStatus);
    }

    private static async Task EnsureExtensionDownloadedAsync(
        string chromeExtensionId, string folderName, string displayName, Action<string>? onStatus)
    {
        var extensionDir = Path.Combine(ExtensionsBaseDir, folderName);
        var manifestPath = Path.Combine(extensionDir, "manifest.json");

        if (File.Exists(manifestPath))
        {
            return;
        }

        onStatus?.Invoke($"Downloading {displayName}...");

        try
        {
            var crxBytes = await DownloadCrxFromChromeWebStoreAsync(chromeExtensionId);

            ExtractCrxToDirectory(crxBytes, extensionDir);
        }
        catch (Exception)
        {
            // Non-fatal — the browser will work without the extension
        }
    }

    private static async Task<byte[]> DownloadCrxFromChromeWebStoreAsync(string extensionId)
    {
        // Chrome Web Store CRX download URL pattern
        var url = "https://clients2.google.com/service/update2/crx" +
                  "?response=redirect" +
                  "&prodversion=130.0" +
                  "&acceptformat=crx2,crx3" +
                  $"&x=id%3D{extensionId}%26uc";

        using var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Extracts a CRX2/CRX3 file (which wraps a ZIP) into the target directory.
    /// </summary>
    private static void ExtractCrxToDirectory(byte[] crxData, string outputDir)
    {
        if (crxData.Length < 16)
            throw new InvalidDataException("CRX data too short");

        // Verify CRX magic: "Cr24"
        if (crxData[0] != 'C' || crxData[1] != 'r' || crxData[2] != '2' || crxData[3] != '4')
            throw new InvalidDataException("Not a valid CRX file (missing Cr24 magic)");

        var version = BitConverter.ToUInt32(crxData, 4);
        int zipOffset;

        if (version == 3)
        {
            // CRX3: magic(4) + version(4) + headerLen(4) + header(headerLen) + ZIP
            var headerLen = BitConverter.ToUInt32(crxData, 8);
            zipOffset = 12 + (int)headerLen;
        }
        else if (version == 2)
        {
            // CRX2: magic(4) + version(4) + pubKeyLen(4) + sigLen(4) + pubKey + sig + ZIP
            var pubKeyLen = BitConverter.ToUInt32(crxData, 8);
            var sigLen = BitConverter.ToUInt32(crxData, 12);
            zipOffset = 16 + (int)pubKeyLen + (int)sigLen;
        }
        else
        {
            throw new InvalidDataException($"Unsupported CRX version: {version}");
        }

        if (zipOffset >= crxData.Length)
            throw new InvalidDataException("Invalid CRX header — ZIP offset exceeds data length");

        // Clean and recreate output directory
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        using var zipStream = new MemoryStream(crxData, zipOffset, crxData.Length - zipOffset);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(outputDir);
    }

    private static async Task LoadAllExtensionsAsync(CoreWebView2 coreWebView, Action<string>? onStatus)
    {
        if (!Directory.Exists(ExtensionsBaseDir))
        {
            return;
        }

        // Check which extensions are already installed in this profile
        var installedExtensions = await coreWebView.Profile.GetBrowserExtensionsAsync();
        var installedIds = new HashSet<string>(
            installedExtensions.Select(e => e.Id), StringComparer.OrdinalIgnoreCase);

        foreach (var extensionFolder in Directory.GetDirectories(ExtensionsBaseDir))
        {
            var manifestPath = Path.Combine(extensionFolder, "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            try
            {
                var ext = await coreWebView.Profile.AddBrowserExtensionAsync(extensionFolder);

                if (!installedIds.Contains(ext.Id))
                {
                    onStatus?.Invoke($"Extension loaded: {ext.Name}");
                }
            }
            catch (Exception)
            {
                // Extension loading failed silently
            }
        }

        // Also load from app-local Extensions/ folder (for user-provided extensions)
        var appExtensionsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");
        if (Directory.Exists(appExtensionsDir))
        {
            foreach (var appExtFolder in Directory.GetDirectories(appExtensionsDir))
            {
                var manifestPath = Path.Combine(appExtFolder, "manifest.json");
                if (!File.Exists(manifestPath)) continue;

                try
                {
                    var ext = await coreWebView.Profile.AddBrowserExtensionAsync(appExtFolder);
                }
                catch (Exception)
                {
                    // Extension loading failed silently
                }
            }
        }
    }
}
