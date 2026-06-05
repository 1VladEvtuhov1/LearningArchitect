using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LearningArchitect.Modules.InterviewArena.Net
{
    public sealed class UnityWebRequestBackendApiClient : IBackendApiClient
    {
        private readonly string _baseUrl;
        private readonly float _timeoutSeconds;

        public UnityWebRequestBackendApiClient(string baseUrl, float timeoutSeconds)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _timeoutSeconds = Mathf.Max(1f, timeoutSeconds);
        }

        public Task<ApiResult<LoginByNameResponseDto>> LoginByNameAsync(
            string username,
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<LoginByNameRequestDto, LoginByNameResponseDto>(
                "/api/auth/login-by-name",
                UnityWebRequest.kHttpVerbPOST,
                new LoginByNameRequestDto(username),
                sessionToken: null,
                cancellationToken);

        public Task<ApiResult<ProfileResponseDto>> GetProfileMeAsync(
            string sessionToken,
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<object, ProfileResponseDto>(
                "/api/profile/me",
                UnityWebRequest.kHttpVerbGET,
                body: null,
                sessionToken,
                cancellationToken);

        public Task<ApiResult<LobbyListResponseDto>> ListLobbiesAsync(
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<object, LobbyListResponseDto>(
                "/api/lobbies",
                UnityWebRequest.kHttpVerbGET,
                body: null,
                sessionToken: null,
                cancellationToken);

        public Task<ApiResult<LobbyDetailDto>> CreateLobbyAsync(
            string sessionToken,
            string name,
            int maxPlayers,
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<CreateLobbyRequestDto, LobbyDetailDto>(
                "/api/lobbies",
                UnityWebRequest.kHttpVerbPOST,
                new CreateLobbyRequestDto(name, maxPlayers),
                sessionToken,
                cancellationToken);

        public Task<ApiResult<LobbyDetailDto>> JoinLobbyAsync(
            string sessionToken,
            string lobbyId,
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<object, LobbyDetailDto>(
                $"/api/lobbies/{lobbyId}/join",
                UnityWebRequest.kHttpVerbPOST,
                body: null,
                sessionToken,
                cancellationToken);

        public Task<ApiResult<LobbyDetailDto>> SetReadyAsync(
            string sessionToken,
            string lobbyId,
            bool isReady,
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<SetReadyRequestDto, LobbyDetailDto>(
                $"/api/lobbies/{lobbyId}/ready",
                UnityWebRequest.kHttpVerbPOST,
                new SetReadyRequestDto(isReady),
                sessionToken,
                cancellationToken);

        public Task<ApiResult<MatchStartResponseDto>> StartLobbyAsync(
            string sessionToken,
            string lobbyId,
            CancellationToken cancellationToken = default) =>
            SendJsonAsync<object, MatchStartResponseDto>(
                $"/api/lobbies/{lobbyId}/start",
                UnityWebRequest.kHttpVerbPOST,
                body: null,
                sessionToken,
                cancellationToken);

        private async Task<ApiResult<TResponse>> SendJsonAsync<TRequest, TResponse>(
            string path,
            string method,
            TRequest body,
            string sessionToken,
            CancellationToken cancellationToken) where TResponse : class
        {
            string url = _baseUrl + path;
            byte[] payload = body != null ? Encoding.UTF8.GetBytes(BackendJsonParser.Serialize(body)) : null;

            using UnityWebRequest request = new UnityWebRequest(url, method)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = Mathf.CeilToInt(_timeoutSeconds),
            };

            if (payload != null)
            {
                request.uploadHandler = new UploadHandlerRaw(payload);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            if (!string.IsNullOrEmpty(sessionToken))
                request.SetRequestHeader("Authorization", "Bearer " + sessionToken);

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            long statusCode = request.responseCode;
            string responseText = request.downloadHandler?.text ?? string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (BackendJsonParser.TryDeserialize<ApiErrorDto>(responseText, out ApiErrorDto apiError)
                    && apiError != null)
                {
                    return ApiResult<TResponse>.Failure(
                        apiError.message ?? request.error,
                        apiError.errorCode,
                        statusCode);
                }

                return ApiResult<TResponse>.Failure(
                    string.IsNullOrWhiteSpace(responseText) ? request.error : responseText,
                    statusCode: statusCode);
            }

            if (typeof(TResponse) == typeof(object))
                return ApiResult<TResponse>.Success(default, statusCode);

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return ApiResult<TResponse>.Failure(
                    "Empty backend response.",
                    statusCode: statusCode);
            }

            if (!BackendJsonParser.TryDeserialize<TResponse>(responseText, out TResponse parsed))
            {
                return ApiResult<TResponse>.Failure(
                    "Failed to parse backend response.",
                    statusCode: statusCode);
            }

            return ApiResult<TResponse>.Success(parsed, statusCode);
        }
    }
}
