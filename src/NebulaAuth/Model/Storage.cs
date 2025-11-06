using AchiesUtilities.Extensions;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Exceptions;
using NebulaAuth.Model.Mafiles;
using SteamLib;
using SteamLib.Exceptions.Authorization;
using SteamLib.SteamMobile;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NebulaAuth.Model;

public static class Storage
{
    public const string DIR_MAFILES = "maFiles";
    public const string DIR_REMOVED_MAFILES = "maFiles_removed";
    public const string DIR_BACKUP_MAFILES = "maFiles_backup";
    public static string MafilesDirectory { get; } = Path.GetFullPath(DIR_MAFILES);
    public static string RemovedMafilesDirectory { get; } = Path.GetFullPath(DIR_REMOVED_MAFILES);
    public static string BackupMafilesDirectory { get; } = Path.GetFullPath(DIR_BACKUP_MAFILES);

    public static ObservableCollection<Mafile> MaFiles { get; private set; } = [];

    public static async Task Initialize(int threadCount, CancellationToken token = default)
    {
        Directory.CreateDirectory(MafilesDirectory);
        Directory.CreateDirectory(RemovedMafilesDirectory);
        Directory.CreateDirectory(BackupMafilesDirectory);

        var files = Directory
            .GetFiles(MafilesDirectory)
            .Where(file => Path.GetExtension(file).EqualsIgnoreCase(".mafile"))
            .ToList();


        var localList = new ConcurrentBag<Mafile>();
        await Task.Run(() =>
        {
            return Parallel.ForEachAsync(files,
                new ParallelOptions { CancellationToken = token, MaxDegreeOfParallelism = threadCount },
                async (file, ct) =>
                {
                    try
                    {
                        var data = await ReadMafileAsync(file);
                        localList.Add(data);
                    }
                    catch (Exception ex)
                    {
                        Shell.Logger.Error(ex, "Can't load mafile {file}", Path.GetFileName(file));
                    }
                });
        }, token);

        MaFiles = new ObservableCollection<Mafile>(localList.OrderBy(m => m.AccountName));
    }

    /// <summary>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="overwrite"></param>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="SessionInvalidException"></exception>
    /// <exception cref="IOException"></exception>
    public static void AddNewMafile(string path, bool overwrite)
    {
        Mafile data;
        try
        {
            data = ReadMafile(path);
        }
        catch (Exception ex)
            when (ex is not MafileNeedReloginException)
        {
            Shell.Logger.Warn(ex, "Can't load mafile");
            throw new FormatException("File data is not valid", ex);
        }

        if (string.IsNullOrWhiteSpace(data.AccountName))
            throw new FormatException("File data is not valid. Missing AccountName");

        try
        {
            _ = SteamGuardCodeGenerator.GenerateCode(data.SharedSecret);
        }
        catch (Exception ex)
        {
            throw new FormatException("Can't generate code on this mafile", ex);
        }

        if (overwrite == false && File.Exists(GetOrCreateMafilePath(data)))
        {
            throw new IOException("File already exist and overwrite is False"); //TODO: Custom Exception
        }


        SaveMafile(data);
    }
    public static Mafile ReadMafile(string path)
    {
        var str = File.ReadAllText(path);
        return NebulaSerializer.Deserialize(str, path);
    }
    public static async Task<Mafile> ReadMafileAsync(string path)
    {
        var str = await File.ReadAllTextAsync(path);
        return NebulaSerializer.Deserialize(str, path);
    }

    public static void SaveMafile(Mafile data)
    {
        var path = GetOrCreateMafilePath(data);
        var str = NebulaSerializer.SerializeMafile(data);
        File.WriteAllText(Path.GetFullPath(path), str);

        var existed = MaFiles.SingleOrDefault(m => m.AccountName == data.AccountName);
        if (existed != null)
        {
            var index = MaFiles.IndexOf(existed);
            MaFiles[index] = data;
        }
        else
        {
            MaFiles.Add(data);
        }
    }
    public static void UpdateMafile(Mafile data)
    {
        var path = GetOrCreateMafilePath(data);
        var str = NebulaSerializer.SerializeMafile(data);
        File.WriteAllText(Path.GetFullPath(path), str);
    }
    public static void MoveToRemoved(Mafile data)
    {
        var sourcePath = GetOrCreateMafilePath(data);
        var destinationPath = Path.Combine(DIR_REMOVED_MAFILES, data.Filename + ".mafile");
        var destinationPathFinal = destinationPath;
        var i = 0;
        while (File.Exists(destinationPathFinal))
        {
            i++;
            destinationPathFinal = destinationPath + $" ({i})";
        }

        File.Copy(sourcePath, destinationPathFinal, false);
        File.Delete(sourcePath);
        MaFiles.Remove(data);
    }
    private static string GetOrCreateMafilePath(Mafile data)
    {
        if (data.Filename != null)
        {
            return Path.Combine(MafilesDirectory, data.Filename);
        }

        var fileName = CreateMafileFileName(data, Settings.Instance.UseAccountNameAsMafileName);
        data.Filename = fileName;
        return Path.Combine(MafilesDirectory, fileName);
    }

    private static string CreateMafileFileName(Mafile data, bool useAccountName)
    {
        return useAccountName
            ? MafileNamingStrategy.Login.GetMafileName(data)
            : MafileNamingStrategy.SteamId.GetMafileName(data);
    }

    public static string? TryGetMafilePath(Mafile data)
    {
        if (data.Filename == null) return null;
        return Path.Combine(MafilesDirectory, data.Filename);
    }

    public static void WriteBackup(MobileDataExtended data)
    {
        var json = NebulaSerializer.SerializeMafile(data, null);
        WriteBackup(data.AccountName, json);
    }
    public static void WriteBackup(string accountName, string data)
    {
        Directory.CreateDirectory(DIR_BACKUP_MAFILES);
        File.WriteAllText(Path.Combine(DIR_BACKUP_MAFILES, accountName + MafileNamingStrategy.DEF_EXTENSION), data);
    }

    public static async Task<MafileRenameResult> RenameMafiles(bool loginAsFileName, IProgress<double>? progress = null)
    {
        if (MaFiles.Count == 0) return new MafileRenameResult();
        var now = DateTime.Now;
        var backupFileName = $"rename_backup_{now:yyyy-MM-dd_HH-mm-ss}.zip";
        var zipPath = Path.Combine(BackupMafilesDirectory, backupFileName);
        await using (var zipStream = new FileStream(zipPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false);
            var files = Directory
                .EnumerateFiles(MafilesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => Path.GetExtension(f).Equals(".mafile", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var counter = 0;
            foreach (var file in files)
            {
                counter++;
                var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel.Optimal);

                await using var entryStream = entry.Open();
                await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                    useAsync: true);
                await fileStream.CopyToAsync(entryStream);
                
                if (counter % 5 == 0)
                {
                    progress?.Report(counter / (double)files.Count);
                    await Task.Delay(10);
                }
            }
        }

        var mafiles = MaFiles.ToList();
        var res = new MafileRenameResult
        {
            Total = mafiles.Count,
            BackupFileName = backupFileName
        };

        foreach (var mafile in mafiles)
        {
            try
            {
                var targetFileName = CreateMafileFileName(mafile, loginAsFileName);
                if (mafile.Filename == targetFileName || mafile.Filename == null)
                {
                    res.Renamed += 1;
                    continue;
                }

                var fullTargetPath = Path.Combine(MafilesDirectory, targetFileName);
                var fullSourcePath = Path.Combine(MafilesDirectory, mafile.Filename);
                if (!File.Exists(fullSourcePath))
                {
                    Shell.Logger.Warn("Can't rename mafile {old} to {new} because source file not found",
                        mafile.Filename, targetFileName);
                    IncErrors();
                    continue;
                }

                if (File.Exists(fullTargetPath))
                {
                    Shell.Logger.Warn("Can't rename mafile {old} to {new} because target file already exist",
                        mafile.Filename, targetFileName);
                    res.AlreadyExist += 1;
                    continue;
                }

                File.Move(fullSourcePath, fullTargetPath);
                res.Renamed += 1;
                mafile.Filename = targetFileName;

            }
            catch (Exception ex)
            {
                Shell.Logger.Error(ex, "Error renaming mafile {file} {accountName}", mafile.Filename,
                    mafile.AccountName);
                IncErrors();
            }

        }
        return res;

        void IncErrors() => res.Errors += 1;
    }

    public class MafileRenameResult
    {
        public int Total { get; set; }
        public int Renamed { get; set; }
        public int NotRenamed => Errors + AlreadyExist;
        public int Errors { get; set; }
        public int AlreadyExist { get; set; }
        public string BackupFileName { get; set; }
    }
}