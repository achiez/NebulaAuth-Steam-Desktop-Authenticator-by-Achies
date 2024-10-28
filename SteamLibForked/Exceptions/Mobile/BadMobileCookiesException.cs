namespace SteamLib.Exceptions.Mobile;

[Serializable]
public class BadMobileCookiesException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public BadMobileCookiesException() : base("You are using default HttpClient without mobile specific cookies. Login can't be proceeded with these cookies")
    {
    }

    public BadMobileCookiesException(string message) : base(message)
    {
    }

    public BadMobileCookiesException(string message, Exception inner) : base(message, inner)
    {
    }
}