﻿namespace SteamLib.Utility.MafileSerialization;

public class MafileSerializerSettings
{
    public MafileDeserializationOptions DeserializationOptions { get; set; } = new();

    [Obsolete("Currently not used")]
    public MafileDeserializationOptions SerializationOptions { get; set; } = new();
}

public class MafileDeserializationOptions
{
    public bool AllowDeviceIdGeneration { get; set; }
    public bool AllowSessionIdGeneration { get; set; }

    /// <summary>
    /// Throws if the <see cref="MobileDataExtended.SerialNumber"/> is 0 or invalid. Otherwise, SerialNumber will be set to 0.
    /// </summary>
    public bool ThrowIfInvalidSerialNumber { get; set; }

    /// <summary>
    /// Restricts recovering an invalid <see cref="MobileDataExtended.SerialNumber"/> if the value is written as a negative number. 
    /// This can occur when an incompatible type is used, one that does not support large proto fixed64 values.
    /// <br/> Returns 0 if <see langword="true"/>, instead of attempting to repair the value.
    /// </summary>
    public bool RestrictOverflowSerialNumberRecovery { get; set; }
    public bool ThrowIfInvalidSteamId { get; set; }

}

public class MafileSerializationOptions
{

}