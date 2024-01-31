namespace SteamLib.Utility.MaFiles;

public class MafileCredits : IMafileCredits
{
    internal static readonly MafileCredits Instance = new();
    private const string ORIGINAL_AUTHOR = "Achies";
    private const string MOBILE_APP = "https://github.com/achiez/NebulaAuth";

    public string OriginalAuthor => ORIGINAL_AUTHOR;
    public string BestOpenSourceMobileApp => MOBILE_APP;

}

public interface IMafileCredits
{
    string OriginalAuthor { get; }
}