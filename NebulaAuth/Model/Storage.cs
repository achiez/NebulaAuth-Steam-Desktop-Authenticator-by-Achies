﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AchiesUtilities.Extensions;
using NebulaAuth.Model.Entities;
using NebulaAuth.Model.Exceptions;
using SteamLib.Core.Models;
using SteamLib.Exceptions;
using SteamLib.SteamMobile;

namespace NebulaAuth.Model;

//RETHINK
public static class Storage
{
    public const string MAFILE_F = "maFiles";
    public const string REMOVED_F = "maFiles_removed";

    public static readonly int DuplicateFound;

    public static string MafileFolder { get; } = Path.GetFullPath(MAFILE_F);
    public static string RemovedMafileFolder { get; } = Path.GetFullPath(REMOVED_F);

    public static ObservableCollection<Mafile> MaFiles { get; } = new();

    static Storage()
    {
        if (Directory.Exists(MafileFolder) == false)
        {
            Directory.CreateDirectory(MafileFolder);
        }

        if (Directory.Exists(RemovedMafileFolder) == false)
        {
            Directory.CreateDirectory(RemovedMafileFolder);
        }

        var files = Directory.GetFiles(MafileFolder);
        var hashNames = new HashSet<string>();
        var hashIds = new HashSet<SteamId>();
        var comparer = new MafileNameComparer(Settings.Instance.UseAccountNameAsMafileName);
        var ordered = files.Order(comparer).ToList();
        foreach (var file in ordered.Where(file => Path.GetExtension(file).EqualsIgnoreCase(".mafile")))
        {
            try
            {
                var data = ReadMafile(file);

                if (hashNames.Contains(data.AccountName) || hashIds.Contains(data.SteamId))
                {
                    DuplicateFound++;
                    Shell.Logger.Error("Duplicate mafile {file}", Path.GetFileName(file));
                    continue;
                }

                hashNames.Add(data.AccountName);
                if (data.SessionData != null) hashIds.Add(data.SteamId);
                MaFiles.Add(data);
            }
            catch (Exception ex)
            {
                Shell.Logger.Error(ex, "Can't load mafile {file}", Path.GetFileName(file));
            }
        }

        MaFiles = new ObservableCollection<Mafile>(MaFiles.OrderBy(m => m.AccountName));
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
}

//TODO: Refactor
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