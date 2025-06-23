using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AchiesUtilities.Extensions;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Exceptions;
using SteamLib;
using SteamLib.Exceptions.Authorization;
using SteamLib.SteamMobile;

namespace NebulaAuth.Model;

public static class Storage
{
    public const string MAFILE_F = "maFiles";
    public const string REMOVED_F = "maFiles_removed";
    private static int _duplicateFound;

    public static int DuplicateFound => _duplicateFound;
    public static string MafileFolder { get; } = Path.GetFullPath(MAFILE_F);
    public static string RemovedMafileFolder { get; } = Path.GetFullPath(REMOVED_F);

    public static ObservableCollection<Mafile> MaFiles { get; private set; } = new();

    static Storage()
    {
    }

    public static async Task Initialize(int threadCount, CancellationToken token = default)
    {
        if (!Directory.Exists(MafileFolder))
            Directory.CreateDirectory(MafileFolder);

        if (!Directory.Exists(RemovedMafileFolder))
            Directory.CreateDirectory(RemovedMafileFolder);
        var comparer = new MafileNameComparer(Settings.Instance.UseAccountNameAsMafileName);
        var files = Directory
            .GetFiles(MafileFolder)
            .Where(file => Path.GetExtension(file).EqualsIgnoreCase(".mafile"))
            .Order(comparer)
            .ToList();


        var hashNames = new ConcurrentDictionary<string, byte>();
        var hashIds = new ConcurrentDictionary<SteamId, byte>();
        var localList = new ConcurrentBag<Mafile>();

        var processed = 0;

        await Task.Run(() =>
        {
            return Parallel.ForEachAsync(files,
                new ParallelOptions {CancellationToken = token, MaxDegreeOfParallelism = threadCount},
                async (file, ct) =>
                {
                    try
                    {
                        var data = await ReadMafileAsync(file);

                        if (!hashNames.TryAdd(data.AccountName, 0) ||
                            (data.SessionData != null && !hashIds.TryAdd(data.SteamId, 0)))
                        {
                            Interlocked.Increment(ref _duplicateFound);
                            Shell.Logger.Error("Duplicate mafile {file}", Path.GetFileName(file));
                        }

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

        if (overwrite == false && File.Exists(CreatePathForMafile(data)))
        {
            throw new IOException("File already exist and overwrite is False");
        }


        SaveMafile(data);
    }


    public static Mafile ReadMafile(string path)
    {
        var str = File.ReadAllText(path);
        return NebulaSerializer.Deserialize(str);
    }

    public static async Task<Mafile> ReadMafileAsync(string path)
    {
        var str = await File.ReadAllTextAsync(path);
        return NebulaSerializer.Deserialize(str);
    }

    public static void SaveMafile(Mafile data)
    {
        var path = CreatePathForMafile(data);
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
        var path = CreatePathForMafile(data);
        var str = NebulaSerializer.SerializeMafile(data);
        File.WriteAllText(Path.GetFullPath(path), str);
    }

    public static void MoveToRemoved(Mafile data)
    {
        var path = CreatePathForMafile(data);
        var copyPath = Path.Combine(REMOVED_F, data.SteamId + ".mafile");
        var copyPathCompleted = copyPath;
        var i = 0;
        while (File.Exists(copyPathCompleted))
        {
            i++;
            copyPathCompleted = copyPath + $" ({i})";
        }

        File.Copy(path, copyPathCompleted, false);
        File.Delete(path);
        MaFiles.Remove(data);
    }

    private static string CreatePathForMafile(Mafile data)
    {
        var fileName = Settings.Instance.UseAccountNameAsMafileName
            ? CreateFileNameWithAccountName(data.AccountName)
            : CreateFileNameWithSteamId(data.SteamId);

        return Path.Combine(MafileFolder, fileName);
    }

    private static string CreateFileNameWithAccountName(string accountName)
    {
        return accountName + ".mafile";
    }

    private static string CreateFileNameWithSteamId(SteamId steamId)
    {
        return steamId.Steam64.Id + ".mafile";
    }

    public static string? TryFindMafilePath(Mafile data)
    {
        var pathFileName = Path.Combine(MafileFolder, CreateFileNameWithAccountName(data.AccountName));
        string? pathSteamId = null;
        if (data.SessionData != null)
        {
            pathSteamId = Path.Combine(MafileFolder, CreateFileNameWithSteamId(data.SteamId));
        }

        var steamIdExist = pathSteamId != null && File.Exists(pathSteamId);
        var accountNameExist = File.Exists(pathFileName);

        if (steamIdExist && accountNameExist)
        {
            return Settings.Instance.UseAccountNameAsMafileName ? pathFileName : pathSteamId;
        }

        if (steamIdExist ^ accountNameExist)
        {
            return steamIdExist ? pathSteamId : pathFileName;
        }

        return null;
    }

    public static void BackupHandler(MobileDataExtended data)
    {
        if (Directory.Exists("mafiles_backup") == false)
        {
            Directory.CreateDirectory("mafiles_backup");
        }


        var json = NebulaSerializer.SerializeMafile(data, null);
        File.WriteAllText(Path.Combine("mafiles_backup", data.AccountName + ".mafile"),
            json);
    }

    public static void BackupHandlerStr(string accountName, string data)
    {
        if (Directory.Exists("mafiles_backup") == false)
        {
            Directory.CreateDirectory("mafiles_backup");
        }


        File.WriteAllText(Path.Combine("mafiles_backup", accountName + ".mafile"),
            data);
    }
}

//TODO: Refactor
//TODO: use numeric orderer when .net 10 released
internal class MafileNameComparer : IComparer<string>
{
    private const string MAF_64_START = "765";
    private static readonly IComparer<string> DefaultComparer = Comparer<string>.Default;
    public bool MafileNameMode { get; }

    public MafileNameComparer(bool mafileNameMode)
    {
        MafileNameMode = mafileNameMode;
    }


    public int Compare(string? x, string? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;


        var xisSteamId = Path.GetFileName(x).StartsWith(MAF_64_START);
        var yisSteamId = Path.GetFileName(y).StartsWith(MAF_64_START);

        if (xisSteamId ^ yisSteamId)
        {
            if (MafileNameMode)
            {
                return xisSteamId ? 1 : -1;
            }

            return yisSteamId ? 1 : -1;
        }

        return DefaultComparer.Compare(x, y);
    }
}