namespace ExtractionRpg.Api.Auth;

public sealed class AuthSessionOptions
{
    public const string SectionName = "Session";

    public int LifetimeHours { get; init; } = 24;
}
