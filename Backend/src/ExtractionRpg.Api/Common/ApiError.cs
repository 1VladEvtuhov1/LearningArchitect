namespace ExtractionRpg.Api.Common;

public sealed record ApiError(string ErrorCode, string Message);

public static class ApiErrorCodes
{
    public const string UsernameInvalid = "USERNAME_INVALID";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string LobbyNotFound = "LOBBY_NOT_FOUND";
    public const string LobbyFull = "LOBBY_FULL";
    public const string NotLobbyMember = "NOT_LOBBY_MEMBER";
    public const string LobbyNameInvalid = "LOBBY_NAME_INVALID";
    public const string MaxPlayersInvalid = "MAX_PLAYERS_INVALID";
    public const string MatchAlreadyStarted = "MATCH_ALREADY_STARTED";
    public const string HostOnly = "HOST_ONLY";
    public const string PlayerNotReady = "PLAYER_NOT_READY";
}

public static class ApiResults
{
    public static IResult Error(int statusCode, string errorCode, string message) =>
        Results.Json(new ApiError(errorCode, message), statusCode: statusCode);
}
