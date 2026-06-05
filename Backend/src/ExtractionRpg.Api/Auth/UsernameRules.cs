using System.Text.RegularExpressions;

namespace ExtractionRpg.Api.Auth;

public static partial class UsernameRules
{
    [GeneratedRegex("^[a-zA-Z0-9_]{3,16}$", RegexOptions.CultureInvariant)]
    private static partial Regex UsernamePattern();

    public const string InvalidMessage =
        "Username must be 3-16 characters and contain only letters, numbers or underscore.";

    public static bool TryNormalize(string? username, out string normalized)
    {
        normalized = username?.Trim() ?? string.Empty;
        return normalized.Length > 0 && UsernamePattern().IsMatch(normalized);
    }
}
