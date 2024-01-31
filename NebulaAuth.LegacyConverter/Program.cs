using Newtonsoft.Json;
using SteamLib.Utility.MaFiles;

var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
if (currentPath != null)
    Environment.CurrentDirectory = currentPath;


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
        var maf = MafileSerializer.Deserialize(text, out _);
        var legacy = MafileSerializer.SerializeLegacy(maf, Formatting.Indented);
        var name = Path.GetFileNameWithoutExtension(path);
        Write(legacy, name);

        Console.WriteLine("DONE: " + name);
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

Console.WriteLine("Press any key to exit...");
Console.ReadKey();


void Write(string maf, string name)
{

    var path = Path.Combine(name + "_legacy.mafile");
    File.WriteAllText(path, maf);
}