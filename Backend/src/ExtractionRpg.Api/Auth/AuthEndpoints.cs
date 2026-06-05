using ExtractionRpg.Api.Common;

namespace ExtractionRpg.Api.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/auth");
        group.MapPost("/login-by-name", LoginByName);
        return group;
    }

    private static IResult LoginByName(LoginByNameRequest request, AuthService auth)
    {
        if (!UsernameRules.TryNormalize(request.Username, out string normalized))
        {
            return ApiResults.Error(
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.UsernameInvalid,
                UsernameRules.InvalidMessage);
        }

        LoginByNameResponse response = auth.LoginByName(normalized);
        return Results.Ok(response);
    }
}
