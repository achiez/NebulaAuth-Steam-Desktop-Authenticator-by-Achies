using NebulaAuth.Model.Entities;
using System.IO;

namespace NebulaAuth.Model.Mafiles;

internal static class MafilesStorageHelper
{
    /// <summary>
    ///     Returns <see cref="Mafile.Filename" /> or creates a new one and updates the property. <see cref="Mafile.Filename"/> is always not <see langword="null"/> after this method call.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GetOrUpdateMafilePath(Mafile data)
    {
        if (data.Filename != null)
        {
            return Path.Combine(Storage.MafilesDirectory, data.Filename);
        }

        var fileName = CreateMafileFileName(data, Settings.Instance.UseAccountNameAsMafileName);
        data.Filename = fileName;
        return Path.Combine(Storage.MafilesDirectory, fileName);
    }

    /// <summary>
    ///     Creates mafile file name according to the current settings.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="useAccountName"></param>
    /// <returns></returns>
    public static string CreateMafileFileName(Mafile data, bool useAccountName)
    {
        return useAccountName
            ? MafileNamingStrategy.Login.GetMafileName(data)
            : MafileNamingStrategy.SteamId.GetMafileName(data);
    }
}