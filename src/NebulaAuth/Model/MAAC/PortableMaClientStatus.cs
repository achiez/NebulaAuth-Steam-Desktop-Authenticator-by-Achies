namespace NebulaAuth.Model.MAAC;

public record PortableMaClientStatus(PortableMaClientStatusType StatusType, string? Message = null)
{
    public static PortableMaClientStatus Ok()
    {
        return new PortableMaClientStatus(PortableMaClientStatusType.Ok);
    }

    public static PortableMaClientStatus Warning(string? message)
    {
        return new PortableMaClientStatus(PortableMaClientStatusType.Warning, message);
    }

    public static PortableMaClientStatus Error(string? message)
    {
        return new PortableMaClientStatus(PortableMaClientStatusType.Error, message);
    }
}

public enum PortableMaClientStatusType
{
    Ok,
    Warning,
    Error
}