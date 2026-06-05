namespace ExtractionRpg.Api.Common;

public static class BearerToken
{
    private const string Prefix = "Bearer ";

    public static bool TryRead(HttpRequest request, out string token)
    {
        token = string.Empty;
        if (!request.Headers.TryGetValue("Authorization", out var values))
            return false;

        string? header = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        token = header[Prefix.Length..].Trim();
        return token.Length > 0;
    }
}
