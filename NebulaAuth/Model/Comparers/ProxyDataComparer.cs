using System;
using System.Collections.Generic;
using AchiesUtilities.Web.Proxy;

namespace NebulaAuth.Model.Comparers;

public class ProxyDataComparer : IEqualityComparer<ProxyData>
{
    public bool Equals(ProxyData? x, ProxyData? y)
    {
        return Equal(x, y);

    }

    public static bool Equal(ProxyData? x, ProxyData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.Address == y.Address
               && x.Port == y.Port
               && x.Username == y.Username
               && x.Password == y.Password;

    }

    public int GetHashCode(ProxyData obj)
    {
        return HashCode.Combine(obj.Address, obj.Port, obj.Username, obj.Password);
    }
}