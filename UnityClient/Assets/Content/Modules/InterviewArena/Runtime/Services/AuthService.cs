using System.Threading;
using System.Threading.Tasks;
using LearningArchitect.Modules.InterviewArena.Net;
using LearningArchitect.Shared.Settings;

namespace LearningArchitect.Modules.InterviewArena.Services
{
    public sealed class AuthService
    {
        private readonly IBackendApiClient _client;

        public AuthService(IBackendApiClient client) => _client = client;

        public async Task<ApiResult<LoginByNameResponseDto>> LoginByNameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            ApiResult<LoginByNameResponseDto> result = await _client.LoginByNameAsync(username, cancellationToken);
            if (!result.Succeeded || result.Value == null)
                return result;

            SessionStorage.SaveLogin(
                result.Value.sessionToken,
                result.Value.userId,
                result.Value.username);
            GameSettings.SetLastUsername(result.Value.username);

            return result;
        }

        public void Logout() => SessionStorage.ClearAll();
    }
}
