namespace ExtractionRpg.Api.Lobbies;

public interface ILobbyRepository
{
    IReadOnlyList<Lobby> ListOpen();
    bool TryGet(string lobbyId, out Lobby? lobby);
    Lobby Create(string name, int maxPlayers, string hostUserId, string hostUsername);
}
