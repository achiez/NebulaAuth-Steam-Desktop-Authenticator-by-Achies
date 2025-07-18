﻿namespace SteamLib.Exceptions.Authorization;

public class SessionInvalidException : Exception
{
    public const string SESSION_NULL_MSG = "Session is null. SteamClient must be logged in before proceeding.";
    public const string GOT_401_MSG = "Steam indicates the session is invalid. Received a 401 Unauthorized response.";

    public const string HEADER_NO_STEAMID =
        "Header doesn't contain SteamId (== false). It indicated account is not logged in";

    public SessionInvalidException()
    {
    }

    public SessionInvalidException(string message) : base(message)
    {
    }

    public SessionInvalidException(string message, Exception? inner) : base(message, inner)
    {
    }
}