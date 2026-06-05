using Microsoft.Extensions.Options;

namespace ExtractionRpg.Api.Auth;

public sealed record LoginByNameRequest(string? Username);

public sealed record LoginByNameResponse(
    string UserId,
    string Username,
    string SessionToken,
    DateTimeOffset ExpiresAt);

public sealed record ProfileResponse(
    string UserId,
    string Username,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt);

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly ISessionStore _sessions;
    private readonly TimeSpan _sessionLifetime;

    public AuthService(IUserRepository users, ISessionStore sessions, IOptions<AuthSessionOptions> options)
    {
        _users = users;
        _sessions = sessions;
        _sessionLifetime = TimeSpan.FromHours(Math.Clamp(options.Value.LifetimeHours, 1, 720));
    }

    public LoginByNameResponse LoginByName(string username)
    {
        UserAccount user = _users.GetOrCreate(username);
        SessionRecord session = _sessions.Create(user.UserId, _sessionLifetime);

        return new LoginByNameResponse(
            user.UserId,
            user.Username,
            session.Token,
            session.ExpiresAt);
    }

    public bool TryResolveUserFromToken(string token, out UserAccount? user, out bool expired)
    {
        user = null;
        expired = false;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (!_sessions.TryGetValid(token, now, out SessionRecord? session) || session is null)
        {
            expired = true;
            return false;
        }

        if (!_users.TryGetById(session.UserId, out user) || user is null)
            return false;

        user.LastSeenAt = now;
        return true;
    }
}
