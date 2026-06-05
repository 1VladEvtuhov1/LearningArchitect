using System.Collections.Concurrent;

namespace ExtractionRpg.Api.Matches;

public sealed class InMemoryMatchRepository : IMatchRepository
{
    private readonly ConcurrentDictionary<string, MatchRecord> _byMatchId = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _lobbyToMatchId = new(StringComparer.Ordinal);

    public MatchRecord Create(string matchId, string lobbyId, string hostUserId, string connectUrl)
    {
        var match = new MatchRecord
        {
            MatchId = matchId,
            LobbyId = lobbyId,
            HostUserId = hostUserId,
            ConnectUrl = connectUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _byMatchId[match.MatchId] = match;
        _lobbyToMatchId[lobbyId] = match.MatchId;
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
}
