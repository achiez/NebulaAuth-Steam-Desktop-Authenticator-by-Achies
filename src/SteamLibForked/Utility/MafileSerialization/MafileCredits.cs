namespace SteamLib.Utility.MafileSerialization;

public class MafileCredits : IMafileCredits
{
    private const string ORIGINAL_AUTHOR = "Achies";
    private const string MOBILE_APP = "https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies";
    internal static readonly MafileCredits Instance = new();

    public string OriginalAuthor => ORIGINAL_AUTHOR;
    public string BestOpenSourceMobileApp => MOBILE_APP;
}

public interface IMafileCredits
{
    string OriginalAuthor { get; }
}