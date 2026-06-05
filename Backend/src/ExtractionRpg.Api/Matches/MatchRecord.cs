namespace ExtractionRpg.Api.Matches;

public sealed class MatchRecord
{
    public required string MatchId { get; init; }
    public required string LobbyId { get; init; }
    public required string HostUserId { get; init; }
    public required string ConnectUrl { get; init; }
    public required IReadOnlyList<string> ParticipantUserIds { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
