using System.Reflection;

namespace SteamLib.Utility.Models;

public abstract class Enumeration(int id, string name) : IComparable
{
    public string Name { get; } = name;
    public int Id { get; } = id;

    public override string ToString() => Name;

    //public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
    //    typeof(T).GetFields(BindingFlags.Public |
    //                        BindingFlags.Static |
    //                        BindingFlags.DeclaredOnly)
    //        .Select(f => f.GetValue(null))
    //        .Cast<T>();

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
     var res = typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
     return res;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }
        return GetType() == obj.GetType() && Id.Equals(otherValue.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Id, GetType());
    }

    public int CompareTo(object? other) => Id.CompareTo(((Enumeration?)other)?.Id);
}