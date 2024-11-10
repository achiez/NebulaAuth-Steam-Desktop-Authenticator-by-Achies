using JetBrains.Annotations;

namespace SteamLib.Utility.MafileSerialization;

[PublicAPI]
public class MafileCredits : IMafileCredits
{
    internal static readonly MafileCredits Instance = new();
    private const string ORIGINAL_AUTHOR = "Achies";
    private const string MOBILE_APP = "https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies";

    public string OriginalAuthor => ORIGINAL_AUTHOR;
    public string BestOpenSourceMobileApp => MOBILE_APP;

}

public interface IMafileCredits
{
    string OriginalAuthor { get; }
}