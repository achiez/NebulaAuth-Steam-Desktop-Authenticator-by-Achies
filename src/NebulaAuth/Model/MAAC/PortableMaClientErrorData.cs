using System;
using System.Collections.Generic;
using System.Linq;

namespace NebulaAuth.Model.MAAC;

public sealed class PortableMaClientErrorData
{
    public SortedDictionary<DateTime, Exception> Values { get; } = new();
    public bool NoErrors => Values.Count == 0;
    private readonly object _lock = new();


    public DateTime? GetOldestErrorTime()
    {
        if (Values.Count == 0) return null;
        return Values.Keys.First();
    }

    public void AddEntry(Exception ex)
    {
        lock (_lock)
            Values[DateTime.UtcNow] = ex;
    }

    public void Clear()
    {
        lock (_lock)
            Values.Clear();
    }

    public TimeSpan? GetTimeFromLastError()
    {
        if (Values.Count == 0) return null;
        var lastEntry = Values.Keys.Last();
        return DateTime.UtcNow - lastEntry;
    }
}