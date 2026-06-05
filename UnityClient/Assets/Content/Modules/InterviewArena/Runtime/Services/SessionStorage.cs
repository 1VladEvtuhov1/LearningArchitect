namespace LearningArchitect.Modules.InterviewArena.Services
{
    public static class SessionStorage
    {
        private const string TokenKey = "ExtractionRpg.SessionToken";
        private const string UserIdKey = "ExtractionRpg.UserId";
        private const string UsernameKey = "ExtractionRpg.Username";
        private const string LobbyIdKey = "ExtractionRpg.LobbyId";

        public static bool HasSession => !string.IsNullOrEmpty(GetToken());

        public static string GetToken() => UnityEngine.PlayerPrefs.GetString(TokenKey, string.Empty);

        public static string GetUserId() => UnityEngine.PlayerPrefs.GetString(UserIdKey, string.Empty);

        public static string GetUsername() => UnityEngine.PlayerPrefs.GetString(UsernameKey, string.Empty);

        public static string GetLobbyId() => UnityEngine.PlayerPrefs.GetString(LobbyIdKey, string.Empty);

        public static void SaveLogin(string token, string userId, string username)
        {
            UnityEngine.PlayerPrefs.SetString(TokenKey, token ?? string.Empty);
            UnityEngine.PlayerPrefs.SetString(UserIdKey, userId ?? string.Empty);
            UnityEngine.PlayerPrefs.SetString(UsernameKey, username ?? string.Empty);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void SaveLobbyId(string lobbyId)
        {
            UnityEngine.PlayerPrefs.SetString(LobbyIdKey, lobbyId ?? string.Empty);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void ClearLobby()
        {
            UnityEngine.PlayerPrefs.DeleteKey(LobbyIdKey);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void ClearAll()
        {
            UnityEngine.PlayerPrefs.DeleteKey(TokenKey);
            UnityEngine.PlayerPrefs.DeleteKey(UserIdKey);
            UnityEngine.PlayerPrefs.DeleteKey(UsernameKey);
            UnityEngine.PlayerPrefs.DeleteKey(LobbyIdKey);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
