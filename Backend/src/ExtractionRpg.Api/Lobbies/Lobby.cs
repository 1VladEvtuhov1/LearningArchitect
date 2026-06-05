namespace ExtractionRpg.Api.Lobbies;

public sealed class LobbyMember
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public bool IsHost { get; init; }
    public bool IsReady { get; set; }
}

public sealed class Lobby
{
    public required string LobbyId { get; init; }
    public required string Name { get; init; }
    public required string HostUserId { get; init; }
    public int MaxPlayers { get; init; }
    public LobbyState State { get; set; }
    public List<LobbyMember> Players { get; } = new();
}
