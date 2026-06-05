namespace ExtractionRpg.Api.Auth;

public sealed class UserAccount
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastSeenAt { get; set; }
}
