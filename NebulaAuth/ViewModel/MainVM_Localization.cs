using NebulaAuth.Core;

namespace NebulaAuth.ViewModel;

public partial class MainVM
{
    private const string LOC_PATH = "MainVM";
    private static string? GetLocalization(string key)
    {
        return LocManager.GetCodeBehind(LOC_PATH, key);
    }

    private static string GetLocalizationOrDefault(string key)
    {
        return LocManager.GetCodeBehindOrDefault(key, LOC_PATH, key);
    }
}