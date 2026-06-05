using System;

namespace LearningArchitect.Modules.InterviewArena.Net
{
    [Serializable]
    public sealed class ApiErrorDto
    {
        public string errorCode;
        public string message;
    }

    [Serializable]
    public sealed class LoginByNameRequestDto
    {
        public string username;

        public LoginByNameRequestDto(string username) => this.username = username;
    }

    [Serializable]
    public sealed class LoginByNameResponseDto
    {
        public string userId;
        public string username;
        public string sessionToken;
        public string expiresAt;
    }

    [Serializable]
    public sealed class ProfileResponseDto
    {
        public string userId;
        public string username;
        public string createdAt;
        public string lastSeenAt;
    }

    [Serializable]
    public sealed class CreateLobbyRequestDto
    {
        public string name;
        public int maxPlayers;

        public CreateLobbyRequestDto(string name, int maxPlayers)
        {
            this.name = name;
            this.maxPlayers = maxPlayers;
        }
    }

    [Serializable]
    public sealed class SetReadyRequestDto
    {
        public bool isReady;

        public SetReadyRequestDto(bool isReady) => this.isReady = isReady;
    }

    [Serializable]
    public sealed class LobbyListItemDto
    {
        public string lobbyId;
        public string name;
        public int playerCount;
        public int maxPlayers;
        public string state;
    }

    [Serializable]
    public sealed class LobbyListResponseDto
    {
        public LobbyListItemDto[] items;
    }

    [Serializable]
    public sealed class LobbyPlayerDto
    {
        public string userId;
        public string username;
        public bool isReady;
        public bool isHost;
    }

    [Serializable]
    public sealed class LobbyDetailDto
    {
        public string lobbyId;
        public string name;
        public string hostUserId;
        public int maxPlayers;
        public string state;
        public LobbyPlayerDto[] players;
    }

    [Serializable]
    public sealed class MatchStartResponseDto
    {
        public string matchId;
        public string connectUrl;
    }
}
