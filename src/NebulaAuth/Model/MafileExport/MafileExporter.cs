using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AchiesUtilities.Extensions;
using NebulaAuth.Core;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Mafiles;
using SteamLibForked.Models.SteamIds;

namespace NebulaAuth.Model.MafileExport;

public static class MafileExporter
{
    public static async Task<ExportResult> ExportMafiles(IEnumerable<string> keys, MafileExportTemplate template)
    {
        if (template.Path == null || !PathIsValid(template.Path))
        {
            return new ExportResult(LocManager.GetCodeBehindOrDefault("InvalidPath", "MafileExporter.InvalidPath"));
        }

        if (!Directory.Exists(template.Path))
        {
            Directory.CreateDirectory(template.Path);
        }

        if (IsNebulaUtilizedDirectory(template.Path))
        {
            return new ExportResult(LocManager.GetCodeBehindOrDefault("NebulaUtilizedDirectory",
                "MafileExporter.NebulaUtilizedDirectory"));
        }

        if (!IsAllowedToWrite(template.Path))
        {
            return new ExportResult(LocManager.GetCodeBehindOrDefault("AccessDenied", "MafileExporter.AccessDenied"));
        }


        var exported = new Dictionary<Mafile, string>();
        var notFound = new List<string>();
        var conflict = new List<string>();

        foreach (var key in keys)
        {
            SteamId? steamId = null;
            if (SteamId64.TryParse(key, out var id64))
            {
                steamId = id64;
            }

            var maf = Storage.MaFiles.FirstOrDefault(m => m.AccountName.EqualsIgnoreCase(key) || m.SteamId == steamId);
            if (maf != null)
            {
                var fileName = await ExportMafile(template.Path, template, maf);
                if (fileName == null)
                {
                    conflict.Add(key);
                    continue;
                }

                exported[maf] = fileName;
            }
            else
            {
                notFound.Add(key);
            }
        }

        return new ExportResult(exported, notFound, conflict);
    }

    private static async Task<string?> ExportMafile(string path, MafileExportTemplate template, Mafile mafile)
    {
        var serializeMaf = new Mafile
        {
            SharedSecret = template.IncludeSharedSecret ? mafile.SharedSecret : null!,
            IdentitySecret = template.IncludeIdentitySecret ? mafile.IdentitySecret : null!,
            DeviceId = template.IncludeIdentitySecret ? mafile.DeviceId : null!,
            RevocationCode = template.IncludeRCode ? mafile.RevocationCode : null!,
            AccountName = mafile.AccountName,
            SessionData = template.IncludeSessionData ? mafile.SessionData : null!,
            SteamId = mafile.SteamId,
            ServerTime = template.IncludeOtherInfo ? mafile.ServerTime : 0,
            SerialNumber = template.IncludeOtherInfo ? mafile.SerialNumber : 0,
            Uri = template.IncludeOtherInfo ? mafile.Uri : null!,
            TokenGid = template.IncludeOtherInfo ? mafile.TokenGid : null!,
            Secret1 = template.IncludeOtherInfo ? mafile.Secret1 : null!,
            Proxy = template.IncludeNebulaProxy ? mafile.Proxy : null!,
            Group = template.IncludeNebulaGroup ? mafile.Group : null!,
            Password = template.IncludeNebulaPassword ? mafile.Password : null!,
            LinkedClient = null,
            Filename = null
        };

        var serialized = NebulaSerializer.SerializeMafile(serializeMaf);
        var strategy = template.UseLoginAsMafileName ? MafileNamingStrategy.Login : MafileNamingStrategy.SteamId;
        var fileName = strategy.GetMafileName(serializeMaf);
        var fullPath = Path.Combine(path, fileName);
        if (File.Exists(fullPath))
        {
            return null;
        }

        await File.WriteAllTextAsync(fullPath, serialized);
        return fullPath;
    }

    private static bool PathIsValid(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        try
        {
            var full = Path.GetFullPath(path);

            var invalidPathChars = Path.GetInvalidPathChars();
            if (full.IndexOfAny(invalidPathChars) >= 0) return false;

            var invalidFileChars = Path.GetInvalidFileNameChars();

            foreach (var part in full.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (part is [_, ':']) continue;

                if (part.IndexOfAny(invalidFileChars) >= 0) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAllowedToWrite(string path)
    {
        try
        {
            var testFilePath = Path.Combine(path, "test.tmp");
            using (var fs = File.Create(testFilePath))
            {
            }

            File.Delete(testFilePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsNebulaUtilizedDirectory(string path)
    {
        return Storage.MafilesDirectories.Any(x => Path.GetFullPath(path).EqualsIgnoreCase(x));
    }
}