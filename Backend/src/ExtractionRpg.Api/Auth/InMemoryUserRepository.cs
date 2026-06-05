using System.Collections.Concurrent;

namespace ExtractionRpg.Api.Auth;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, UserAccount> _byUsername = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, UserAccount> _byUserId = new(StringComparer.Ordinal);

    public UserAccount GetOrCreate(string username)
    {
        return _byUsername.AddOrUpdate(
            username,
            _ => CreateUser(username),
            (_, existing) =>
            {
                existing.LastSeenAt = DateTimeOffset.UtcNow;
                return existing;
            });
    }

    public bool TryGetById(string userId, out UserAccount? user)
    {
        bool found = _byUserId.TryGetValue(userId, out UserAccount? account);
        user = account;
        return found;
    }

    private UserAccount CreateUser(string username)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var account = new UserAccount
        {
            UserId = $"u_{Guid.NewGuid():N}"[..14],
            Username = username,
            CreatedAt = now,
            LastSeenAt = now,
        };

        _byUserId[account.UserId] = account;
        return account;
    }
}
