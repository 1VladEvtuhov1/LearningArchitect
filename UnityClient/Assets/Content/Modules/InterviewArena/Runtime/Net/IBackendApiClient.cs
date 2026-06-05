using System.Threading;
using System.Threading.Tasks;

namespace LearningArchitect.Modules.InterviewArena.Net
{
    public interface IBackendApiClient
    {
        Task<ApiResult<LoginByNameResponseDto>> LoginByNameAsync(
            string username,
            CancellationToken cancellationToken = default);

        Task<ApiResult<ProfileResponseDto>> GetProfileMeAsync(
            string sessionToken,
            CancellationToken cancellationToken = default);

        Task<ApiResult<LobbyListResponseDto>> ListLobbiesAsync(
            CancellationToken cancellationToken = default);

        Task<ApiResult<LobbyDetailDto>> CreateLobbyAsync(
            string sessionToken,
            string name,
            int maxPlayers,
            CancellationToken cancellationToken = default);

        Task<ApiResult<LobbyDetailDto>> JoinLobbyAsync(
            string sessionToken,
            string lobbyId,
            CancellationToken cancellationToken = default);

        Task<ApiResult<LobbyDetailDto>> SetReadyAsync(
            string sessionToken,
            string lobbyId,
            bool isReady,
            CancellationToken cancellationToken = default);

        Task<ApiResult<MatchStartResponseDto>> StartLobbyAsync(
            string sessionToken,
            string lobbyId,
            CancellationToken cancellationToken = default);
    }
}
