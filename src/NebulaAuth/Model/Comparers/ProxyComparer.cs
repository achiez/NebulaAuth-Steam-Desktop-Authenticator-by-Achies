using System;
using System.Collections.Generic;
using NebulaAuth.Model.Entities;

namespace NebulaAuth.Model.Comparers;

public class ProxyComparer : IEqualityComparer<MaProxy>
{
    public bool Equals(MaProxy? x, MaProxy? y)
    {
        return Equal(x, y);
    }

    public static bool Equal(MaProxy? x, MaProxy? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.Id == y.Id && x.Data.Equals(y.Data);
    }

    public int GetHashCode(MaProxy obj)
    {
        return HashCode.Combine(obj.Id, obj.Data);
    }
}