using System;
using NebulaAuth.Model.Entities;
using SteamLib;

namespace NebulaAuth.Model.Exceptions;

public class MafileNeedReloginException : Exception
{
    public Mafile Mafile { get; }

    public MafileNeedReloginException(Mafile mafile)
    {
        Mafile = mafile;
    }
}