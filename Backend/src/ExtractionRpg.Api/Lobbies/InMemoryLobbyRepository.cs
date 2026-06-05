using System.Collections.Concurrent;

namespace ExtractionRpg.Api.Lobbies;

public sealed class InMemoryLobbyRepository : ILobbyRepository
{
    private readonly ConcurrentDictionary<string, Lobby> _lobbies = new(StringComparer.Ordinal);

    public IReadOnlyList<Lobby> ListOpen() =>
        _lobbies.Values
            .Where(lobby => lobby.State == LobbyState.Preparing)
            .OrderBy(lobby => lobby.LobbyId, StringComparer.Ordinal)
            .ToList();

    public bool TryGet(string lobbyId, out Lobby? lobby)
    {
        bool found = _lobbies.TryGetValue(lobbyId, out Lobby? value);
        lobby = value;
        return found;
    }

    public Lobby Create(string name, int maxPlayers, string hostUserId, string hostUsername)
    {
        var lobby = new Lobby
        {
            LobbyId = $"lobby_{Guid.NewGuid():N}"[..18],
            Name = name,
            HostUserId = hostUserId,
            MaxPlayers = maxPlayers,
            State = LobbyState.Preparing,
        };

        lobby.Players.Add(new LobbyMember
        {
            UserId = hostUserId,
            Username = hostUsername,
            IsHost = true,
            IsReady = false,
        });

        _lobbies[lobby.LobbyId] = lobby;
        return lobby;
    }
}
