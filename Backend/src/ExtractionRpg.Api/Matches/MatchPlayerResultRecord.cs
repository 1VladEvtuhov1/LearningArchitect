namespace ExtractionRpg.Api.Matches;

public sealed class MatchPlayerResultRecord
{
    public required string MatchId { get; init; }
    public required string UserId { get; init; }
    public required string Outcome { get; init; }
    public float ElapsedSeconds { get; init; }
    public DateTimeOffset SubmittedAt { get; init; }
}
