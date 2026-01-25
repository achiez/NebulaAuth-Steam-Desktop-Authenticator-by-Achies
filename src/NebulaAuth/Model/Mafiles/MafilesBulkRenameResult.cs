namespace NebulaAuth.Model.Mafiles;

public class MafilesBulkRenameResult
{
    public int Total { get; set; }
    public int Renamed { get; set; }
    public int NotRenamed => Errors + Conflict;
    public int Errors { get; set; }
    public int Conflict { get; set; }
    public string BackupFileName { get; set; }
}