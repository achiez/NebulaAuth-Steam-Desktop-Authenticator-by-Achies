﻿using System.Reflection;
using AchiesUtilities.Extensions;
using NebulaAuth.LegacyConverter;
using Newtonsoft.Json;
using SteamLib.Utility.MafileSerialization;

try
{
    var mafileSerializer = new MafileSerializer(new MafileSerializerSettings
    {
        DeserializationOptions =
        {
            AllowDeviceIdGeneration = true,
            AllowSessionIdGeneration = true
        }
    });
    var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (currentPath != null)
        Environment.CurrentDirectory = currentPath;
    const string toStoreFolder = "ConvertedMafiles";

    if (Directory.Exists(toStoreFolder) == false)
    {
        Directory.CreateDirectory(toStoreFolder);
    }
    else
    {
        var isEmpty = Directory.GetFiles(toStoreFolder).Length == 0;
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (!isEmpty)
        {
            Console.WriteLine(
                "WARNING! 'ConverterdMafiles' folder is not empty. Please backup data from it and then continue.");
            Console.ResetColor();
            Console.WriteLine("Press Y to continue");
            while (Console.ReadKey(true).Key != ConsoleKey.Y)
            {
            }
        }
    }

    var decryptMode = false;
    while (true)
    {
        Console.WriteLine("Press 'D' to select decrypt mode. Press 'C' to convert mode. Press ESC to exit");
        var key = Console.ReadKey(true);
        switch (key.Key)
        {
            case ConsoleKey.D:
                decryptMode = true;
                break;
            case ConsoleKey.C:
                break;
            case ConsoleKey.Escape:
                return;
            default:
                continue;
        }

        break;
    }


    Manifest? manifest = null;
    string? password = null;
    if (decryptMode)
    {
        var files = Directory.GetFiles(currentPath, "manifest.json");
        var manifestPath = files.FirstOrDefault();
        if (manifestPath == null)
        {
            Console.WriteLine("No manifest.json found in current directory");
            return;
        }

        var manifestText = File.ReadAllText(manifestPath);
        try
        {
            manifest = JsonConvert.DeserializeObject<Manifest>(manifestText)!;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Can't read manifest: " + ex);
            return;
        }

        if (manifest.Encrypted == false)
        {
            Console.WriteLine("Manifest is not encrypted");
            return;
        }

        while (true)
        {
            Console.WriteLine("Please enter your encryption password: ");
            password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password))
                continue;

            break;
        }
    }


    Console.WriteLine(currentPath);


    foreach (var path in args)
    {
        if (Path.Exists(path) == false)
        {
            Console.WriteLine($"NOT VALID PATH: '{path}'");
            continue;
        }

        Console.WriteLine("Reading: " + path);
        try
        {
            var text = File.ReadAllText(path);
            if (decryptMode)
            {
                var fileName = Path.GetFileName(path);
                text = DecryptMafile(fileName, text);
                if (text == null)
                {
                    Console.WriteLine(path + " not found in manifest. Skipped");
                    continue;
                }
            }

            var data = mafileSerializer.Deserialize(text);
            var maf = data.Data;
            var legacy = MafileSerializer.SerializeLegacy(maf, Formatting.Indented);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            Write(legacy, fileNameWithoutExtension);

            Console.WriteLine("DONE: " + fileNameWithoutExtension);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex);
        }
        finally
        {
            Console.WriteLine("-----------------------------------------");
        }
    }

    //Local Functions

    void Write(string maf, string name)
    {
        var path = Path.Combine(toStoreFolder, name + "_legacy.mafile");
        File.WriteAllText(path, maf);
    }

    string? DecryptMafile(string fileName, string cipherText)
    {
        if (password == null) return null;
        var entry = manifest?.Entries.FirstOrDefault(x => x.Filename.EqualsIgnoreCase(fileName));
        if (entry == null)
        {
            return null;
        }

        var iv = entry.EncryptionIv;
        var salt = entry.EncryptionSalt;
        return SDAEncryptor.DecryptData(password, salt, iv, cipherText);
    }
}
finally
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}