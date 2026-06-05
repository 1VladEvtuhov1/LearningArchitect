using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Common;

namespace ExtractionRpg.Api.Matches;

public static class MatchEndpoints
{
    public static RouteGroupBuilder MapMatchEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/matches");
        group.MapPost("/{matchId}/result", SubmitResult);
        return group;
    }

    private static IResult SubmitResult(
        HttpRequest request,
        string matchId,
        SubmitMatchResultRequest body,
        AuthService auth,
        MatchService matches)
    {
        IResult? unauthorized = AuthenticatedUser.TryResolve(request, auth, out UserAccount user);
        if (unauthorized is not null)
            return unauthorized;

        return matches.SubmitResult(user, matchId, body);
    }
}
