namespace SteamLib.Utility;

public readonly struct SocketsClientHandlerPair
{
    public SocketsHttpHandler Handler { get; }
    public HttpClient Client { get; }

    public SocketsClientHandlerPair(HttpClient client, SocketsHttpHandler handler)
    {
        Handler = handler;
        Client = client;
    }
    public (SocketsHttpHandler, HttpClient) Deconstruct()
    {
        return (Handler, Client);
    }
}