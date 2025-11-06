using System;
using System.IO;
using System.Linq;

namespace NebulaAuth.Utility;

public static class FileNameValidator
{
    public static bool IsValidFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        return !name.Any(c => invalidChars.Contains(c));
    }

    public static string GetInvalidChars(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name.Where(c => invalidChars.Contains(c)).Distinct().ToArray());
    }
}