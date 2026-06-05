using System.Text.RegularExpressions;

namespace ExtractionRpg.Api.Lobbies;

public static partial class LobbyNameRules
{
    public const int MinLength = 3;
    public const int MaxLength = 32;
    public const int MinMaxPlayers = 2;
    public const int MaxMaxPlayers = 8;

    public const string InvalidNameMessage =
        "Lobby name must be 3-32 characters and contain only letters, numbers, spaces or underscore.";

    public const string InvalidMaxPlayersMessage =
        "maxPlayers must be between 2 and 8.";

    [GeneratedRegex("^[a-zA-Z0-9_ ]{3,32}$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    public static bool TryNormalizeName(string? name, out string normalized)
    {
        normalized = name?.Trim() ?? string.Empty;
        if (normalized.Length < MinLength || normalized.Length > MaxLength)
            return false;

        return NamePattern().IsMatch(normalized);
    }

    public static bool IsValidMaxPlayers(int maxPlayers) =>
        maxPlayers is >= MinMaxPlayers and <= MaxMaxPlayers;
}
