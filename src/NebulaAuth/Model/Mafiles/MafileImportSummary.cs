using System;

namespace NebulaAuth.Model.Mafiles;

public class MafileImportSummary
{
    public int Added { get; private set; }
    public int NotAdded { get; private set; }
    public int Errors { get; private set; }

    public void AddedOne()
    {
        Added++;
    }

    public void SkippedOne()
    {
        NotAdded++;
    }

    public void ErrorOne()
    {
        Errors++;
    }

    public void Add(int added = 0, int notAdded = 0, int errors = 0)
    {
        Added += added;
        NotAdded += notAdded;
        Errors += errors;
    }

    public void Apply(AddMafileResult result)
    {
        switch (result)
        {
            case AddMafileResult.Added:
                AddedOne();
                break;
            case AddMafileResult.AlreadyExist:
                SkippedOne();
                break;
            case AddMafileResult.Error:
                ErrorOne();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}