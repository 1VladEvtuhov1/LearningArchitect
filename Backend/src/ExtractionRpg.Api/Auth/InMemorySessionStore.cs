using System.Collections.Concurrent;

namespace ExtractionRpg.Api.Auth;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, SessionRecord> _sessions = new(StringComparer.Ordinal);

    public SessionRecord Create(string userId, TimeSpan lifetime)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var session = new SessionRecord
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_'),
            UserId = userId,
            ExpiresAt = now.Add(lifetime),
        };

        _sessions[session.Token] = session;
        return session;
    }

    public bool TryGetValid(string token, DateTimeOffset now, out SessionRecord? session)
    {
        session = null;
        if (!_sessions.TryGetValue(token, out SessionRecord? record))
            return false;

        if (record.IsExpired(now))
        {
            _sessions.TryRemove(token, out _);
            return false;
        }

        session = record;
        return true;
    }
}
