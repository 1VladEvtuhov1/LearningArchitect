using System.Threading;
using System.Threading.Tasks;
using LearningArchitect.Modules.InterviewArena.Net;

namespace LearningArchitect.Modules.InterviewArena.Services
{
    public sealed class LobbyService
    {
        private readonly IBackendApiClient _client;

        public LobbyService(IBackendApiClient client) => _client = client;

        public Task<ApiResult<LobbyListResponseDto>> ListOpenAsync(CancellationToken cancellationToken = default) =>
            _client.ListLobbiesAsync(cancellationToken);

        public async Task<ApiResult<LobbyDetailDto>> CreateAsync(
            string name,
            int maxPlayers,
            CancellationToken cancellationToken = default)
        {
            ApiResult<LobbyDetailDto> result = await _client.CreateLobbyAsync(
                SessionStorage.GetToken(),
                name,
                maxPlayers,
                cancellationToken);

            if (result.Succeeded && result.Value != null)
                SessionStorage.SaveLobbyId(result.Value.lobbyId);

            return result;
        }

        public async Task<ApiResult<LobbyDetailDto>> JoinAsync(
            string lobbyId,
            CancellationToken cancellationToken = default)
        {
            ApiResult<LobbyDetailDto> result = await _client.JoinLobbyAsync(
                SessionStorage.GetToken(),
                lobbyId,
                cancellationToken);

            if (result.Succeeded && result.Value != null)
                SessionStorage.SaveLobbyId(result.Value.lobbyId);

            return result;
        }

        public Task<ApiResult<LobbyDetailDto>> SetReadyAsync(
            string lobbyId,
            bool isReady,
            CancellationToken cancellationToken = default) =>
            _client.SetReadyAsync(SessionStorage.GetToken(), lobbyId, isReady, cancellationToken);

        public Task<ApiResult<MatchStartResponseDto>> StartAsync(
            string lobbyId,
            CancellationToken cancellationToken = default) =>
            _client.StartLobbyAsync(SessionStorage.GetToken(), lobbyId, cancellationToken);
    }
}
