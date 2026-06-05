using ExtractionRpg.Api.Auth;

namespace ExtractionRpg.Api.Common;

public static class AuthenticatedUser
{
    public static IResult? TryResolve(HttpRequest request, AuthService auth, out UserAccount user)
    {
        user = null!;

        if (!BearerToken.TryRead(request, out string token))
        {
            return ApiResults.Error(
                StatusCodes.Status401Unauthorized,
                ApiErrorCodes.Unauthorized,
                "Authorization header with Bearer token is required.");
        }

        if (!auth.TryResolveUserFromToken(token, out UserAccount? resolved, out bool expired) || resolved is null)
        {
            return ApiResults.Error(
                StatusCodes.Status401Unauthorized,
                expired ? ApiErrorCodes.SessionExpired : ApiErrorCodes.Unauthorized,
                expired ? "Session has expired." : "Session is invalid.");
        }

        user = resolved;
        return null;
    }
}
