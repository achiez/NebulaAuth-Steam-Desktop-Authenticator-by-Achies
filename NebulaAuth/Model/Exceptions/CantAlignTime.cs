using System;

namespace NebulaAuth.Model.Exceptions;

[Serializable]
public class CantAlignTimeException : Exception
{
    public CantAlignTimeException()
    {
    }

    public CantAlignTimeException(string message) : base(message)
    {
    }

    public CantAlignTimeException(string message, Exception inner) : base(message, inner)
    {
    }
}