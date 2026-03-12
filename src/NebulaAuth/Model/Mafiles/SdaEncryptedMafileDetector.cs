using System;
using System.IO;
using AchiesUtilities.Extensions;
using Newtonsoft.Json;
using NebulaAuth.LegacyConverter;

namespace NebulaAuth.Model.Mafiles;

/// <summary>
/// Detects SDA-style encrypted mafiles by checking for manifest.json in the same directory
/// and whether the file is listed in the manifest entries.
/// </summary>
public static class SdaEncryptedMafileDetector
{
    private const string ManifestFileName = "manifest.json";

    /// <summary>
    /// Result of SDA encrypted mafile detection. When not null, the caller can decrypt
    /// using SDAEncryptor.DecryptData(password, Entry.EncryptionSalt, Entry.EncryptionIv, fileContent).
    /// </summary>
    public sealed class SdaEncryptedResult
    {
        public Manifest Manifest { get; }
        public Entry Entry { get; }

        public SdaEncryptedResult(Manifest manifest, Entry entry)
        {
            Manifest = manifest;
            Entry = entry;
        }
    }

    /// <summary>
    /// Checks if the file at <paramref name="mafilePath"/> is an SDA-encrypted mafile by looking for
    /// manifest.json in the same directory. Does not validate file content.
    /// </summary>
    /// <param name="mafilePath">Full path to the .mafile file.</param>
    /// <returns>Manifest and entry for decryption, or null if not SDA encrypted.</returns>
    public static SdaEncryptedResult? TryDetect(string mafilePath)
    {
        if (string.IsNullOrWhiteSpace(mafilePath))
            return null;

        var dir = Path.GetDirectoryName(mafilePath);
        if (string.IsNullOrWhiteSpace(dir))
            return null;

        var fileName = Path.GetFileName(mafilePath);
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        // SDA typically keeps manifest.json in the same folder as mafiles, but depending on how users copy data
        // it may end up one directory above (e.g. selecting a file from a subfolder).
        var manifestPath = FindManifestPath(dir);
        if (manifestPath == null)
            return null;

        var manifest = TryReadEncryptedManifest(manifestPath);
        if (manifest == null)
            return null;

        return TryDetect(mafilePath, manifest);
    }

    public static SdaEncryptedResult? TryDetect(string mafilePath, Manifest manifest)
    {
        if (string.IsNullOrWhiteSpace(mafilePath))
            return null;
        if (manifest == null || !manifest.Encrypted || manifest.Entries == null || manifest.Entries.Length == 0)
            return null;

        var fileName = Path.GetFileName(mafilePath);
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var entry = FindEntry(manifest, fileName);
        if (entry == null)
            return null;

        return new SdaEncryptedResult(manifest, entry);
    }

    public static Manifest? TryReadEncryptedManifestFromPath(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            return null;
        return TryReadEncryptedManifest(manifestPath);
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

    private static Entry? FindEntry(Manifest manifest, string fileName)
    {
        return Array.Find(manifest.Entries, e =>
        {
            if (string.IsNullOrWhiteSpace(e.Filename))
                return false;

            // SDA usually stores just "123.mafile" but some versions/setups can store a relative path
            // like "maFiles\\123.mafile". Match by both raw value and basename.
            if (e.Filename.EqualsIgnoreCase(fileName))
                return true;

            try
            {
                var entryBaseName = Path.GetFileName(e.Filename);
                return !string.IsNullOrWhiteSpace(entryBaseName) && entryBaseName.EqualsIgnoreCase(fileName);
            }
            catch
            {
                return false;
            }
        });
    }

    private static Manifest? TryReadEncryptedManifest(string manifestPath)
    {
        try
        {
            var manifestText = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<Manifest>(manifestText);
            if (manifest == null || !manifest.Encrypted || manifest.Entries == null || manifest.Entries.Length == 0)
                return null;
            return manifest;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
