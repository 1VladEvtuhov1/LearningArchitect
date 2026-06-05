namespace ExtractionRpg.Api.Matches;

public interface IMatchRepository
{
    MatchRecord Create(string matchId, string lobbyId, string hostUserId, string connectUrl);
    bool TryGet(string matchId, out MatchRecord? match);
    bool TryGetByLobbyId(string lobbyId, out MatchRecord? match);
}
