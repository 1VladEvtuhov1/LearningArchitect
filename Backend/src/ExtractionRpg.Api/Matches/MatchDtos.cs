namespace ExtractionRpg.Api.Matches;

public sealed class MatchConnectOptions
{
    public const string SectionName = "Match";

    public string ConnectUrlTemplate { get; init; } =
        "wss://localhost:5001/matches/{matchId}";
}

public sealed record MatchStartResponse(string MatchId, string ConnectUrl);

public sealed record SubmitMatchResultRequest(string Outcome, float ElapsedSeconds);

public sealed record MatchResultResponse(
    string MatchId,
    string UserId,
    string Outcome,
    float ElapsedSeconds,
    DateTimeOffset SubmittedAt);

public static class MatchConnectUrlBuilder
{
    public static string Build(string template, string matchId) =>
        template.Replace("{matchId}", matchId, StringComparison.Ordinal);
}
