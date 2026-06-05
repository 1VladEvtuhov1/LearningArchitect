namespace LearningArchitect.Modules.InterviewArena.Services
{
    public static class SessionStorage
    {
        private const string TokenKey = "ExtractionRpg.SessionToken";
        private const string UserIdKey = "ExtractionRpg.UserId";
        private const string UsernameKey = "ExtractionRpg.Username";
        private const string LobbyIdKey = "ExtractionRpg.LobbyId";
        private const string MatchIdKey = "ExtractionRpg.MatchId";
        private const string MatchStartKey = "ExtractionRpg.MatchStartUtcTicks";

        public static bool HasSession => !string.IsNullOrEmpty(GetToken());

        public static string GetToken() => UnityEngine.PlayerPrefs.GetString(TokenKey, string.Empty);

        public static string GetUserId() => UnityEngine.PlayerPrefs.GetString(UserIdKey, string.Empty);

        public static string GetUsername() => UnityEngine.PlayerPrefs.GetString(UsernameKey, string.Empty);

        public static string GetLobbyId() => UnityEngine.PlayerPrefs.GetString(LobbyIdKey, string.Empty);

        public static string GetMatchId() => UnityEngine.PlayerPrefs.GetString(MatchIdKey, string.Empty);

        public static bool HasActiveMatch => !string.IsNullOrEmpty(GetMatchId());

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

        public static void SaveMatch(string matchId)
        {
            UnityEngine.PlayerPrefs.SetString(MatchIdKey, matchId ?? string.Empty);
            UnityEngine.PlayerPrefs.SetString(MatchStartKey, System.DateTime.UtcNow.Ticks.ToString());
            UnityEngine.PlayerPrefs.Save();
        }

        public static float GetMatchElapsedSeconds()
        {
            string ticksText = UnityEngine.PlayerPrefs.GetString(MatchStartKey, string.Empty);
            if (!long.TryParse(ticksText, out long ticks) || ticks <= 0)
                return 0f;

            System.TimeSpan elapsed = System.DateTime.UtcNow - new System.DateTime(ticks, System.DateTimeKind.Utc);
            return (float)elapsed.TotalSeconds;
        }

        public static void ClearLobby()
        {
            UnityEngine.PlayerPrefs.DeleteKey(LobbyIdKey);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void ClearMatch()
        {
            UnityEngine.PlayerPrefs.DeleteKey(MatchIdKey);
            UnityEngine.PlayerPrefs.DeleteKey(MatchStartKey);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void ClearAll()
        {
            UnityEngine.PlayerPrefs.DeleteKey(TokenKey);
            UnityEngine.PlayerPrefs.DeleteKey(UserIdKey);
            UnityEngine.PlayerPrefs.DeleteKey(UsernameKey);
            UnityEngine.PlayerPrefs.DeleteKey(LobbyIdKey);
            UnityEngine.PlayerPrefs.DeleteKey(MatchIdKey);
            UnityEngine.PlayerPrefs.DeleteKey(MatchStartKey);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
