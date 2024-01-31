using NebulaAuth.Model.Entities;
using Newtonsoft.Json.Linq;
using SteamLib;
using SteamLib.SteamMobile;
using SteamLib.Utility.MaFiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SteamLib.Exceptions;

namespace NebulaAuth.Model;

public static class Storage
{
    public const string MAFILE_F = "maFiles";
    public const string REMOVED_F = "maFiles_removed";

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
        foreach (var file in files)
        {
            if (Path.GetExtension(file).Equals(".mafile", StringComparison.InvariantCultureIgnoreCase) == false) continue;
            try
            {
                var data = ReadMafile(file);
                MaFiles.Add(data);
            }
            catch (Exception ex)
            {
                Shell.Logger.Error(ex, "Can't load mafile {file}", Path.GetFileName(file));
            }
        }

        MaFiles = new ObservableCollection<Mafile>(MaFiles.OrderBy(m => m.AccountName));
    }


    public static void AddNewMafile(string path, bool overwrite)
    {
        Mafile data;
        try
        {
            data = ReadMafile(path);
        }
        catch (Exception ex)
        {
            Shell.Logger.Warn(ex, "Can't load mafile");
            throw new FormatException("File data is not valid", ex);
        }

        if (string.IsNullOrWhiteSpace(data.AccountName))
            throw new FormatException("File data is not valid. Missing AccountName");

        try
        {
            var code = SteamGuardCodeGenerator.GenerateCode(data.SharedSecret);
        }
        catch (Exception ex)
        {
            throw new FormatException("Can't generate code on this mafile", ex);
        }


        if (data.SessionData == null)
            throw new SessionInvalidException("File data is not valid. Missing SessionData")
            {
                Data = { { "mafile", data } }
            };
    

        if (overwrite == false && File.Exists(GetMafilePath(data)))
        {
            throw new IOException("File already exist and overwrite is False");
        }

      
        SaveMafile(data);
    }


    public static Mafile ReadMafile(string path)
    {
        var str = File.ReadAllText(path);
        var mafile = MafileSerializer.Deserialize(str, out var mafileData);
        if (mafileData.IsExtended == false)
            throw new FormatException("Mafile is not extended data");


        var props = mafileData.UnusedProperties ?? new Dictionary<string, JProperty>();
        var proxy = GetPropertyValue<MaProxy>("Proxy", props);
        var group = GetPropertyValue<string>("Group", props);
        var password = GetPropertyValue<string>("Password", props);
        return Mafile.FromMobileDataExtended((MobileDataExtended)mafile, proxy, group, password);
    }

    public static string SerializeMafile(Mafile data)
    {
        if (Settings.Instance.LegacyMode)
        {
            var props = new Dictionary<string, object?>
            {
                {nameof(Mafile.Proxy), data.Proxy},
                {nameof(Mafile.Group), data.Group},
                {nameof(Mafile.Password), data.Password}
            };
            return MafileSerializer.SerializeLegacy(data, Formatting.Indented, props);

        }
        else
        {
           return MafileSerializer.Serialize(data);
        }
    }


    private static T? GetPropertyValue<T>(string name, Dictionary<string, JProperty> dictionary)
    {
        if (dictionary.TryGetValue(name, out var prop) == false) return default;
        var value = prop.Value;
        try
        {
            return value.ToObject<T>();
        }
        catch (Exception ex)
        {
            Shell.Logger.Warn(ex, "Can't deserialize property {name}", name);
            return default;
        }
    }

    public static void SaveMafile(Mafile data)
    {

        var path = GetMafilePath(data);
        var str = SerializeMafile(data);
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
        var path = GetMafilePath(data);
        var str = SerializeMafile(data);
        File.WriteAllText(Path.GetFullPath(path), str);
    }

    public static void RemoveMafile(Mafile data)
    {
        var path = GetMafilePath(data);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        MaFiles.Remove(data);
    }
    public static void MoveToRemoved(Mafile data)
    {
        var path = GetMafilePath(data);
        var copyPath = Path.Combine(REMOVED_F, data.SessionData!.SteamId + ".mafile");
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
    private static string GetMafilePath(Mafile data)
    {
        if (data.SessionData == null)
            throw new NullReferenceException("SessionData was null can't retrieve SteamId");
        var fileName = data.SessionData.SteamId + ".mafile";
        return Path.Combine(MafileFolder, fileName);
    }



}