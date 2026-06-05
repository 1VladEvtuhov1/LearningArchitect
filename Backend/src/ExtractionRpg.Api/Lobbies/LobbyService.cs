using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Common;
using ExtractionRpg.Api.Matches;
using Microsoft.Extensions.Options;

namespace ExtractionRpg.Api.Lobbies;

public sealed class LobbyService
{
    private readonly ILobbyRepository _lobbies;
    private readonly IMatchRepository _matches;
    private readonly MatchConnectOptions _matchConnect;

    public LobbyService(
        ILobbyRepository lobbies,
        IMatchRepository matches,
        IOptions<MatchConnectOptions> matchConnect)
    {
        _lobbies = lobbies;
        _matches = matches;
        _matchConnect = matchConnect.Value;
    }

    public LobbyListResponse ListOpen()
    {
        IReadOnlyList<LobbyListItemDto> items = _lobbies.ListOpen()
            .Select(LobbyMapper.ToListItem)
            .ToList();

        return new LobbyListResponse(items);
    }

    public IResult Create(UserAccount user, string name, int maxPlayers)
    {
        if (!LobbyNameRules.TryNormalizeName(name, out string normalizedName))
        {
            return ApiResults.Error(
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.LobbyNameInvalid,
                LobbyNameRules.InvalidNameMessage);
        }

        if (!LobbyNameRules.IsValidMaxPlayers(maxPlayers))
        {
            return ApiResults.Error(
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.MaxPlayersInvalid,
                LobbyNameRules.InvalidMaxPlayersMessage);
        }

        Lobby lobby = _lobbies.Create(normalizedName, maxPlayers, user.UserId, user.Username);
        return Results.Ok(LobbyMapper.ToDetail(lobby));
    }

    public IResult Join(UserAccount user, string lobbyId)
    {
        if (!_lobbies.TryGet(lobbyId, out Lobby? lobby) || lobby is null)
        {
            return ApiResults.Error(
                StatusCodes.Status404NotFound,
                ApiErrorCodes.LobbyNotFound,
                "Lobby was not found.");
        }

        if (lobby.State != LobbyState.Preparing)
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.MatchAlreadyStarted,
                "Lobby is no longer accepting players.");
        }

        LobbyMember? existing = lobby.Players.FirstOrDefault(player => player.UserId == user.UserId);
        if (existing is not null)
            return Results.Ok(LobbyMapper.ToDetail(lobby));

        if (lobby.Players.Count >= lobby.MaxPlayers)
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.LobbyFull,
                "Lobby is full.");
        }

        lobby.Players.Add(new LobbyMember
        {
            UserId = user.UserId,
            Username = user.Username,
            IsHost = false,
            IsReady = false,
        });

        return Results.Ok(LobbyMapper.ToDetail(lobby));
    }

    public IResult SetReady(UserAccount user, string lobbyId, bool isReady)
    {
        if (!_lobbies.TryGet(lobbyId, out Lobby? lobby) || lobby is null)
        {
            return ApiResults.Error(
                StatusCodes.Status404NotFound,
                ApiErrorCodes.LobbyNotFound,
                "Lobby was not found.");
        }

        if (lobby.State != LobbyState.Preparing)
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.MatchAlreadyStarted,
                "Lobby is no longer in preparing state.");
        }

        LobbyMember? member = lobby.Players.FirstOrDefault(player => player.UserId == user.UserId);
        if (member is null)
        {
            return ApiResults.Error(
                StatusCodes.Status403Forbidden,
                ApiErrorCodes.NotLobbyMember,
                "You are not a member of this lobby.");
        }

        member.IsReady = isReady;
        return Results.Ok(LobbyMapper.ToDetail(lobby));
    }

    public IResult Start(UserAccount user, string lobbyId)
    {
        if (!_lobbies.TryGet(lobbyId, out Lobby? lobby) || lobby is null)
        {
            return ApiResults.Error(
                StatusCodes.Status404NotFound,
                ApiErrorCodes.LobbyNotFound,
                "Lobby was not found.");
        }

        if (lobby.State != LobbyState.Preparing)
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.MatchAlreadyStarted,
                "Match has already been started for this lobby.");
        }

        LobbyMember? member = lobby.Players.FirstOrDefault(player => player.UserId == user.UserId);
        if (member is null)
        {
            return ApiResults.Error(
                StatusCodes.Status403Forbidden,
                ApiErrorCodes.NotLobbyMember,
                "You are not a member of this lobby.");
        }

        if (!member.IsHost)
        {
            return ApiResults.Error(
                StatusCodes.Status403Forbidden,
                ApiErrorCodes.HostOnly,
                "Only the lobby host can start the match.");
        }

        if (lobby.Players.Any(player => !player.IsReady))
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.PlayerNotReady,
                "All lobby members must be ready before starting.");
        }

        if (_matches.TryGetByLobbyId(lobbyId, out _))
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.MatchAlreadyStarted,
                "Match has already been started for this lobby.");
        }

        string template = string.IsNullOrWhiteSpace(_matchConnect.ConnectUrlTemplate)
            ? "wss://localhost:5001/matches/{matchId}"
            : _matchConnect.ConnectUrlTemplate;

        string matchId = $"match_{Guid.NewGuid():N}"[..19];
        string connectUrl = MatchConnectUrlBuilder.Build(template, matchId);
        IReadOnlyList<string> participantUserIds = lobby.Players
            .Select(player => player.UserId)
            .ToList();

        MatchRecord match = _matches.Create(matchId, lobbyId, user.UserId, connectUrl, participantUserIds);
        lobby.State = LobbyState.InMatch;

        return Results.Ok(new MatchStartResponse(match.MatchId, match.ConnectUrl));
    }
}
