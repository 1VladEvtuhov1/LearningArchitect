using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Common;

namespace ExtractionRpg.Api.Lobbies;

public static class LobbyEndpoints
{
    public static RouteGroupBuilder MapLobbyEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/lobbies");
        group.MapGet("/", ListLobbies);
        group.MapPost("/", CreateLobby);
        group.MapPost("/{lobbyId}/join", JoinLobby);
        group.MapPost("/{lobbyId}/ready", SetReady);
        group.MapPost("/{lobbyId}/start", StartLobby);
        return group;
    }

    private static IResult ListLobbies(LobbyService lobbies) =>
        Results.Ok(lobbies.ListOpen());

    private static IResult CreateLobby(
        HttpRequest request,
        CreateLobbyRequest body,
        AuthService auth,
        LobbyService lobbies)
    {
        IResult? unauthorized = AuthenticatedUser.TryResolve(request, auth, out UserAccount user);
        if (unauthorized is not null)
            return unauthorized;

        return lobbies.Create(user, body.Name ?? string.Empty, body.MaxPlayers);
    }

    private static IResult JoinLobby(
        HttpRequest request,
        string lobbyId,
        AuthService auth,
        LobbyService lobbies)
    {
        IResult? unauthorized = AuthenticatedUser.TryResolve(request, auth, out UserAccount user);
        if (unauthorized is not null)
            return unauthorized;

        return lobbies.Join(user, lobbyId);
    }

    private static IResult SetReady(
        HttpRequest request,
        string lobbyId,
        SetReadyRequest body,
        AuthService auth,
        LobbyService lobbies)
    {
        IResult? unauthorized = AuthenticatedUser.TryResolve(request, auth, out UserAccount user);
        if (unauthorized is not null)
            return unauthorized;

        return lobbies.SetReady(user, lobbyId, body.IsReady);
    }

    private static IResult StartLobby(
        HttpRequest request,
        string lobbyId,
        AuthService auth,
        LobbyService lobbies)
    {
        IResult? unauthorized = AuthenticatedUser.TryResolve(request, auth, out UserAccount user);
        if (unauthorized is not null)
            return unauthorized;

        return lobbies.Start(user, lobbyId);
    }
}
