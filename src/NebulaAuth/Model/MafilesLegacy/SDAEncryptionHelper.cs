using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace NebulaAuth.Model.MafilesLegacy;

/// <summary>
///     Helper class for detecting and handling SDA encrypted mafiles.
///     This is used in the legacy mafile handling code to support importing SDA encrypted mafiles without requiring the
///     user to manually decrypt them first.
/// </summary>
public static class SDAEncryptionHelper
{
    private const string ManifestFileName = "manifest.json";

    public static bool LooksLikeSdaEncryptedBlob(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var trimmed = content.Trim();

        if (trimmed.StartsWith('{') || trimmed.Length < 64)
            return false;

        Span<byte> buffer = stackalloc byte[trimmed.Length];
        return Convert.TryFromBase64String(trimmed, buffer, out _);
    }

    public static Context? TryDetect(string mafilePath, SDAManifest? sdaManifest)
    {
        if (string.IsNullOrWhiteSpace(mafilePath))
            return null;

        if (sdaManifest == null)
        {
            var dir = Path.GetDirectoryName(mafilePath);
            if (string.IsNullOrWhiteSpace(dir))
                return null;

            var manifestPath = FindManifestPath(dir);
            if (manifestPath == null)
                return null;

            sdaManifest = TryReadEncryptedManifest(manifestPath);
        }

        if (sdaManifest is not {Encrypted: true} || sdaManifest.Entries.Length == 0)
            return null;

        var fileName = Path.GetFileName(mafilePath);
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var entries =
            sdaManifest.Entries.ToDictionary(x => Path.GetFileName(x.Filename), StringComparer.OrdinalIgnoreCase);
        return new Context(sdaManifest, entries, null);
    }

    private static string? FindManifestPath(string startDirectory)
    {
        var current = startDirectory;
        for (var i = 0; i < 2; i++)
        {
            var candidate = Path.Combine(current, ManifestFileName);
            if (File.Exists(candidate))
                return candidate;

            var parent = Directory.GetParent(current);
            if (parent?.FullName == null)
                break;
            current = parent.FullName;
        }

        return null;
    }

    private static SDAManifest? TryReadEncryptedManifest(string manifestPath)
    {
        try
        {
            var manifestText = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<SDAManifest>(manifestText);
            if (manifest is not {Encrypted: true} || manifest.Entries.Length == 0)
                return null;
            return manifest;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string? TryDecrypt(string content, string path, Context context)
    {
        if (string.IsNullOrWhiteSpace(context.Password))
            return null;

        // If we have password, check if we have entry
        var entry = context.GetEntry(path);
        if (entry == null)
        {
            return null;
        }

        // We have entry, try to decrypt
        return SDAEncryptor.TryDecryptData(context.Password, entry.EncryptionSalt, entry.EncryptionIv, content,
            out var decrypted)
            ? decrypted
            : null;
    }

    /// <summary>
    ///     Result of SDA encrypted mafile detection. When not null, the caller can decrypt
    ///     using SDAEncryptor.DecryptData(password, Entry.EncryptionSalt, Entry.EncryptionIv, fileContent).
    /// </summary>
    public sealed class Context
    {
        public SDAManifest SdaManifest { get; }
        public Dictionary<string, SDAManifestEntry> Entries { get; }
        public string? Password { get; }

        public Context(SDAManifest sdaManifest, Dictionary<string, SDAManifestEntry> entries, string? password)
        {
            SdaManifest = sdaManifest;
            Entries = entries;
            Password = password;
        }

        public Context WithPassword(string? password)
        {
            return new Context(SdaManifest, Entries, password);
        }

        public SDAManifestEntry? GetEntry(string path)
        {
            var fileName = Path.GetFileName(path);
            return Entries.GetValueOrDefault(fileName);
        }
    }
}