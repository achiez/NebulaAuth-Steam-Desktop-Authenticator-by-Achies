using System;
using System.Runtime.Serialization;

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

    protected CantAlignTimeException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}