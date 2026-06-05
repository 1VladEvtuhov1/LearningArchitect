using System.Collections.Concurrent;

namespace ExtractionRpg.Api.Matches;

public sealed class InMemoryMatchRepository : IMatchRepository
{
    private readonly ConcurrentDictionary<string, MatchRecord> _byMatchId = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _lobbyToMatchId = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, MatchPlayerResultRecord>> _resultsByMatch =
        new(StringComparer.Ordinal);

    public MatchRecord Create(
        string matchId,
        string lobbyId,
        string hostUserId,
        string connectUrl,
        IReadOnlyList<string> participantUserIds)
    {
        var match = new MatchRecord
        {
            MatchId = matchId,
            LobbyId = lobbyId,
            HostUserId = hostUserId,
            ConnectUrl = connectUrl,
            ParticipantUserIds = participantUserIds,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _byMatchId[match.MatchId] = match;
        _lobbyToMatchId[lobbyId] = match.MatchId;
        _resultsByMatch[match.MatchId] = new ConcurrentDictionary<string, MatchPlayerResultRecord>(StringComparer.Ordinal);
        return match;
    }

    public bool TryGet(string matchId, out MatchRecord? match)
    {
        bool found = _byMatchId.TryGetValue(matchId, out MatchRecord? record);
        match = record;
        return found;
    }

    public bool TryGetByLobbyId(string lobbyId, out MatchRecord? match)
    {
        match = null;
        if (!_lobbyToMatchId.TryGetValue(lobbyId, out string? matchId))
            return false;

        return TryGet(matchId, out match);
    }

    public bool TryGetPlayerResult(string matchId, string userId, out MatchPlayerResultRecord? result)
    {
        result = null;
        if (!_resultsByMatch.TryGetValue(matchId, out ConcurrentDictionary<string, MatchPlayerResultRecord>? byUser))
            return false;

        return byUser.TryGetValue(userId, out result);
    }

    public MatchPlayerResultRecord SavePlayerResult(
        string matchId,
        string userId,
        string outcome,
        float elapsedSeconds)
    {
        if (!_resultsByMatch.TryGetValue(matchId, out ConcurrentDictionary<string, MatchPlayerResultRecord>? byUser))
        {
            byUser = new ConcurrentDictionary<string, MatchPlayerResultRecord>(StringComparer.Ordinal);
            _resultsByMatch[matchId] = byUser;
        }

        var record = new MatchPlayerResultRecord
        {
            MatchId = matchId,
            UserId = userId,
            Outcome = outcome,
            ElapsedSeconds = elapsedSeconds,
            SubmittedAt = DateTimeOffset.UtcNow,
        };

        byUser[userId] = record;
        return record;
    }
}
