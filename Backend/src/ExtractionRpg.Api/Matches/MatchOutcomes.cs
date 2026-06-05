namespace ExtractionRpg.Api.Matches;

public static class MatchOutcomes
{
    public const string Extracted = "Extracted";
    public const string Death = "Death";

    public static bool IsValid(string? outcome) =>
        string.Equals(outcome, Extracted, StringComparison.Ordinal)
        || string.Equals(outcome, Death, StringComparison.Ordinal);
}
