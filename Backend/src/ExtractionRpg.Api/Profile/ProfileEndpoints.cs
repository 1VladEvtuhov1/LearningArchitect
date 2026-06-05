using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Common;

namespace ExtractionRpg.Api.Profile;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/profile");
        group.MapGet("/me", GetMe);
        return group;
    }

    private static IResult GetMe(HttpRequest request, AuthService auth)
    {
        IResult? unauthorized = AuthenticatedUser.TryResolve(request, auth, out UserAccount user);
        if (unauthorized is not null)
            return unauthorized;

        return Results.Ok(new ProfileResponse(
            user.UserId,
            user.Username,
            user.CreatedAt,
            user.LastSeenAt));
    }
}
