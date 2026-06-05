using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Common;

namespace ExtractionRpg.Api.Matches;

public sealed class MatchService
{
    private readonly IMatchRepository _matches;

    public MatchService(IMatchRepository matches) => _matches = matches;

    public IResult SubmitResult(UserAccount user, string matchId, SubmitMatchResultRequest body)
    {
        if (string.IsNullOrWhiteSpace(matchId))
        {
            return ApiResults.Error(
                StatusCodes.Status404NotFound,
                ApiErrorCodes.MatchNotFound,
                "Match was not found.");
        }

        if (!_matches.TryGet(matchId, out MatchRecord? match) || match is null)
        {
            return ApiResults.Error(
                StatusCodes.Status404NotFound,
                ApiErrorCodes.MatchNotFound,
                "Match was not found.");
        }

        if (!match.ParticipantUserIds.Contains(user.UserId, StringComparer.Ordinal))
        {
            return ApiResults.Error(
                StatusCodes.Status403Forbidden,
                ApiErrorCodes.NotMatchParticipant,
                "You are not a participant in this match.");
        }

        if (_matches.TryGetPlayerResult(matchId, user.UserId, out _))
        {
            return ApiResults.Error(
                StatusCodes.Status409Conflict,
                ApiErrorCodes.ResultAlreadySubmitted,
                "Result has already been submitted for this match.");
        }

        if (!MatchOutcomes.IsValid(body.Outcome))
        {
            return ApiResults.Error(
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.MatchOutcomeInvalid,
                "Outcome must be Extracted or Death.");
        }

        if (body.ElapsedSeconds < 0f || body.ElapsedSeconds > 86_400f)
        {
            return ApiResults.Error(
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.ElapsedSecondsInvalid,
                "ElapsedSeconds must be between 0 and 86400.");
        }

        MatchPlayerResultRecord record = _matches.SavePlayerResult(
            matchId,
            user.UserId,
            body.Outcome!,
            body.ElapsedSeconds);

        return Results.Ok(new MatchResultResponse(
            record.MatchId,
            record.UserId,
            record.Outcome,
            record.ElapsedSeconds,
            record.SubmittedAt));
    }
}
