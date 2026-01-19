namespace NebulaAuth.Model.MafileExport;

public class MafileExportTemplate
{
    public string Name { get; set; }
    public bool UseLoginAsMafileName { get; set; }
    public bool IncludeSharedSecret { get; set; }
    public bool IncludeIdentitySecret { get; set; }
    public bool IncludeRCode { get; set; }
    public bool IncludeSessionData { get; set; }
    public bool IncludeOtherInfo { get; set; }
    public bool IncludeNebulaProxy { get; set; }
    public bool IncludeNebulaPassword { get; set; }
    public bool IncludeNebulaGroup { get; set; }
    public string? Path { get; set; }
}