using System;
using AchiesUtilities.Web.Proxy;
using NebulaAuth.Model.Comparers;

namespace NebulaAuth.Model.Entities;

public class MaProxy
{
    public int Id { get; }
    public ProxyData Data { get; }

    public MaProxy(int id, ProxyData data)
    {
        Id = id;
        Data = data;
    }

    public override bool Equals(object? obj)
    {
        return obj is MaProxy p && ProxyComparer.Equal(this, p);
    }

    protected bool Equals(MaProxy other)
    {
        return Id == other.Id && Data.Equals(other.Data);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Data);
    }
}