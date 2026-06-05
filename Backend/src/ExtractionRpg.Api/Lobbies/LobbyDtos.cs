namespace ExtractionRpg.Api.Lobbies;

public sealed record CreateLobbyRequest(string? Name, int MaxPlayers = 4);

public sealed record SetReadyRequest(bool IsReady);

public sealed record LobbyListItemDto(
    string LobbyId,
    string Name,
    int PlayerCount,
    int MaxPlayers,
    string State);

public sealed record LobbyListResponse(IReadOnlyList<LobbyListItemDto> Items);

public sealed record LobbyPlayerDto(
    string UserId,
    string Username,
    bool IsReady,
    bool IsHost);

public sealed record LobbyDetailDto(
    string LobbyId,
    string Name,
    string HostUserId,
    int MaxPlayers,
    string State,
    IReadOnlyList<LobbyPlayerDto> Players);

public static class LobbyMapper
{
    public static LobbyListItemDto ToListItem(Lobby lobby) =>
        new(
            lobby.LobbyId,
            lobby.Name,
            lobby.Players.Count,
            lobby.MaxPlayers,
            lobby.State.ToString());

    public static LobbyDetailDto ToDetail(Lobby lobby) =>
        new(
            lobby.LobbyId,
            lobby.Name,
            lobby.HostUserId,
            lobby.MaxPlayers,
            lobby.State.ToString(),
            lobby.Players.Select(ToPlayer).ToList());

    public static LobbyPlayerDto ToPlayer(LobbyMember member) =>
        new(member.UserId, member.Username, member.IsReady, member.IsHost);
}
