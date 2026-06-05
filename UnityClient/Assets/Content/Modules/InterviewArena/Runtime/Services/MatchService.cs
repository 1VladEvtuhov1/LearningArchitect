using System.Threading;
using System.Threading.Tasks;
using LearningArchitect.Modules.InterviewArena.Net;

namespace LearningArchitect.Modules.InterviewArena.Services
{
    public sealed class MatchService
    {
        private readonly IBackendApiClient _client;

        public MatchService(IBackendApiClient client) => _client = client;

        public Task<ApiResult<MatchResultResponseDto>> SubmitResultAsync(
            string matchId,
            string outcome,
            float elapsedSeconds,
            CancellationToken cancellationToken = default) =>
            _client.SubmitMatchResultAsync(
                SessionStorage.GetToken(),
                matchId,
                new SubmitMatchResultRequestDto(outcome, elapsedSeconds),
                cancellationToken);
    }
}
