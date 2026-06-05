namespace ExtractionRpg.Api.Matches;

public interface IMatchRepository
{
    MatchRecord Create(
        string matchId,
        string lobbyId,
        string hostUserId,
        string connectUrl,
        IReadOnlyList<string> participantUserIds);

    bool TryGet(string matchId, out MatchRecord? match);

    bool TryGetByLobbyId(string lobbyId, out MatchRecord? match);

    bool TryGetPlayerResult(string matchId, string userId, out MatchPlayerResultRecord? result);

    MatchPlayerResultRecord SavePlayerResult(
        string matchId,
        string userId,
        string outcome,
        float elapsedSeconds);
}
