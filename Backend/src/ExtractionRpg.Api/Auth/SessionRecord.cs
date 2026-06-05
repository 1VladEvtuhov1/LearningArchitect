namespace ExtractionRpg.Api.Auth;

public sealed class SessionRecord
{
    public required string Token { get; init; }
    public required string UserId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
}
