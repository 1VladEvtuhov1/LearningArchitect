using System.Text;
using LearningArchitect.Modules.InterviewArena.Net;
using LearningArchitect.Modules.InterviewArena.Services;
using LearningArchitect.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Online flow overlay: login → lobby browser → lobby room. Hides gameplay until <see cref="GameState.LocalArena"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterviewArenaOnlineFlowController : MonoBehaviour
    {
        [SerializeField] private InterviewArenaRuntimeContext context;
        [SerializeField] private BackendApiConfig backendConfig;
        [SerializeField] private RectTransform panelRoot;

        private AuthService _authService;
        private LobbyService _lobbyService;
        private GameStateMachine _stateMachine;
        private bool _gameplayWired;

        private GameObject _loginPanel;
        private GameObject _browserPanel;
        private GameObject _roomPanel;
        private TMP_InputField _usernameInput;
        private TMP_InputField _lobbyNameInput;
        private TMP_Text _statusLabel;
        private TMP_Text _roomDetailsLabel;
        private Toggle _readyToggle;
        private Button _startButton;
        private RectTransform _lobbyListRoot;

        private LobbyDetailDto _currentLobby;

        private void Awake()
        {
            context = context != null ? context : FindAnyObjectByType<InterviewArenaRuntimeContext>();
            if (context == null)
                return;

            _stateMachine = context.StateMachine;
            _stateMachine.StateChanged += HandleStateChanged;
        }

        private void Start()
        {
            if (context == null || backendConfig == null || !backendConfig.UseOnlineFlow)
            {
                EnterLocalArenaImmediate();
                return;
            }

            IBackendApiClient client = new UnityWebRequestBackendApiClient(
                backendConfig.BaseUrl,
                backendConfig.TimeoutSeconds);

            _authService = new AuthService(client);
            _lobbyService = new LobbyService(client);

            EnsurePanelRoot();
            BuildPanels();
            SetGameplayActive(false);
            _stateMachine.Enter(GameState.Login);
            ShowPanel(_loginPanel);
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.LocalArena)
                return;

            SetGameplayActive(true);
            WireGameplayIfNeeded();
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(false);
        }

        private void EnterLocalArenaImmediate()
        {
            _stateMachine?.Enter(GameState.LocalArena);
        }

        private void EnsurePanelRoot()
        {
            if (panelRoot != null)
                return;

            Canvas canvas = FindAnyObjectByType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;
            GameObject root = new GameObject("Container - OnlineFlow", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            panelRoot = root.GetComponent<RectTransform>();
            Stretch(panelRoot);
        }

        private void BuildPanels()
        {
            _statusLabel = CreateLabel(panelRoot, "Text - OnlineStatus", 14, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(900f, 48f));
            _statusLabel.color = ShowcasePalette.TextSecondary;

            _loginPanel = CreatePanel("Panel - Login");
            _usernameInput = CreateInput(_loginPanel.transform, "Input - Username", new Vector2(0f, 40f));
            CreateButton(_loginPanel.transform, "Button - Login", new Vector2(0f, -20f), OnLoginClicked);
            CreateButton(_loginPanel.transform, "Button - SkipOffline", new Vector2(0f, -70f), () => _stateMachine.Enter(GameState.LocalArena));

            _browserPanel = CreatePanel("Panel - LobbyBrowser");
            _lobbyNameInput = CreateInput(_browserPanel.transform, "Input - LobbyName", new Vector2(0f, 120f));
            _lobbyNameInput.text = "Forest Run";
            CreateButton(_browserPanel.transform, "Button - Refresh", new Vector2(-120f, 60f), () => _ = RefreshLobbiesAsync());
            CreateButton(_browserPanel.transform, "Button - Create", new Vector2(120f, 60f), () => _ = CreateLobbyAsync());
            CreateButton(_browserPanel.transform, "Button - Logout", new Vector2(0f, -160f), OnLogoutClicked);

            GameObject listHost = new GameObject("Container - LobbyList", typeof(RectTransform));
            listHost.transform.SetParent(_browserPanel.transform, false);
            _lobbyListRoot = listHost.GetComponent<RectTransform>();
            _lobbyListRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _lobbyListRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _lobbyListRoot.pivot = new Vector2(0.5f, 0.5f);
            _lobbyListRoot.anchoredPosition = new Vector2(0f, -20f);
            _lobbyListRoot.sizeDelta = new Vector2(520f, 220f);
            listHost.AddComponent<VerticalLayoutGroup>().spacing = 6f;

            _roomPanel = CreatePanel("Panel - LobbyRoom");
            _roomDetailsLabel = CreateLabel(_roomPanel.transform, "Text - RoomDetails", 16, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(560f, 220f));
            _roomDetailsLabel.alignment = TextAlignmentOptions.TopLeft;

            GameObject readyHost = new GameObject("Toggle - Ready", typeof(RectTransform), typeof(Toggle), typeof(Image));
            readyHost.transform.SetParent(_roomPanel.transform, false);
            RectTransform readyRect = readyHost.GetComponent<RectTransform>();
            readyRect.anchorMin = new Vector2(0.5f, 0.5f);
            readyRect.anchorMax = new Vector2(0.5f, 0.5f);
            readyRect.anchoredPosition = new Vector2(-140f, -120f);
            readyRect.sizeDelta = new Vector2(28f, 28f);
            readyHost.GetComponent<Image>().color = ShowcasePalette.AccentSoft(0.9f);
            _readyToggle = readyHost.GetComponent<Toggle>();
            _readyToggle.onValueChanged.AddListener(value => _ = SetReadyAsync(value));
            CreateLabel(_roomPanel.transform, "Text - ReadyLabel", 14, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-80f, -120f), new Vector2(120f, 28f)).text = "Ready";

            _startButton = CreateButton(_roomPanel.transform, "Button - StartMatch", new Vector2(120f, -120f), () => _ = StartMatchAsync());
            CreateButton(_roomPanel.transform, "Button - RefreshRoom", new Vector2(0f, -170f), () => _ = RefreshRoomAsync());
            CreateButton(_roomPanel.transform, "Button - LeaveRoom", new Vector2(0f, -220f), OnLeaveRoomClicked);
        }

        private async void OnLoginClicked()
        {
            string username = _usernameInput != null ? _usernameInput.text : string.Empty;
            SetStatus("Logging in...");
            ApiResult<LoginByNameResponseDto> result = await _authService.LoginByNameAsync(username);
            if (!result.Succeeded || result.Value == null)
            {
                SetStatus(FormatError(result));
                return;
            }

            SetStatus("Logged in as " + result.Value.username);
            _stateMachine.Enter(GameState.LobbyBrowser);
            ShowPanel(_browserPanel);
            await RefreshLobbiesAsync();
        }

        private async System.Threading.Tasks.Task RefreshLobbiesAsync()
        {
            SetStatus("Loading lobbies...");
            ApiResult<LobbyListResponseDto> result = await _lobbyService.ListOpenAsync();
            if (!result.Succeeded)
            {
                SetStatus(FormatError(result));
                return;
            }

            ClearLobbyList();
            LobbyListItemDto[] items = result.Value?.items ?? System.Array.Empty<LobbyListItemDto>();
            for (int i = 0; i < items.Length; i++)
            {
                LobbyListItemDto item = items[i];
                string lobbyId = item.lobbyId;
                CreateButton(
                    _lobbyListRoot,
                    "Button - Join_" + lobbyId,
                    Vector2.zero,
                    () => _ = JoinLobbyAsync(lobbyId),
                    $"{item.name} ({item.playerCount}/{item.maxPlayers})");
            }

            SetStatus(items.Length + " open lobby(ies).");
        }

        private async System.Threading.Tasks.Task CreateLobbyAsync()
        {
            string name = _lobbyNameInput != null ? _lobbyNameInput.text : "Forest Run";
            SetStatus("Creating lobby...");
            ApiResult<LobbyDetailDto> result = await _lobbyService.CreateAsync(name, 4);
            if (!result.Succeeded || result.Value == null)
            {
                SetStatus(FormatError(result));
                return;
            }

            EnterLobbyRoom(result.Value);
        }

        private async System.Threading.Tasks.Task JoinLobbyAsync(string lobbyId)
        {
            SetStatus("Joining lobby...");
            ApiResult<LobbyDetailDto> result = await _lobbyService.JoinAsync(lobbyId);
            if (!result.Succeeded || result.Value == null)
            {
                SetStatus(FormatError(result));
                return;
            }

            EnterLobbyRoom(result.Value);
        }

        private void EnterLobbyRoom(LobbyDetailDto lobby)
        {
            _currentLobby = lobby;
            SessionStorage.SaveLobbyId(lobby.lobbyId);
            _stateMachine.Enter(GameState.LobbyRoom);
            ShowPanel(_roomPanel);
            RefreshRoomView();
        }

        private async System.Threading.Tasks.Task RefreshRoomAsync()
        {
            if (_currentLobby == null)
                return;

            ApiResult<LobbyDetailDto> joinResult = await _lobbyService.JoinAsync(_currentLobby.lobbyId);
            if (joinResult.Succeeded && joinResult.Value != null)
                _currentLobby = joinResult.Value;

            RefreshRoomView();
        }

        private async System.Threading.Tasks.Task SetReadyAsync(bool isReady)
        {
            if (_currentLobby == null)
                return;

            ApiResult<LobbyDetailDto> result = await _lobbyService.SetReadyAsync(_currentLobby.lobbyId, isReady);
            if (!result.Succeeded || result.Value == null)
            {
                SetStatus(FormatError(result));
                return;
            }

            _currentLobby = result.Value;
            RefreshRoomView();
        }

        private async System.Threading.Tasks.Task StartMatchAsync()
        {
            if (_currentLobby == null)
                return;

            SetStatus("Starting match...");
            ApiResult<MatchStartResponseDto> result = await _lobbyService.StartAsync(_currentLobby.lobbyId);
            if (!result.Succeeded || result.Value == null)
            {
                SetStatus(FormatError(result));
                return;
            }

            SetStatus("Match " + result.Value.matchId + " started.");
            _stateMachine.Enter(GameState.LoadingMatch);
            _stateMachine.Enter(GameState.LocalArena);
        }

        private void RefreshRoomView()
        {
            if (_currentLobby == null || _roomDetailsLabel == null)
                return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(_currentLobby.name + "  [" + _currentLobby.state + "]");
            builder.AppendLine("LobbyId: " + _currentLobby.lobbyId);
            LobbyPlayerDto[] players = _currentLobby.players ?? System.Array.Empty<LobbyPlayerDto>();
            for (int i = 0; i < players.Length; i++)
            {
                LobbyPlayerDto player = players[i];
                builder.Append("- ");
                builder.Append(player.username);
                builder.Append(player.isHost ? " (host)" : string.Empty);
                builder.Append(player.isReady ? " [ready]" : string.Empty);
                builder.AppendLine();
            }

            _roomDetailsLabel.text = builder.ToString();

            string userId = SessionStorage.GetUserId();
            bool isHost = _currentLobby.hostUserId == userId;
            if (_startButton != null)
                _startButton.gameObject.SetActive(isHost);

            LobbyPlayerDto self = FindSelf(players, userId);
            if (_readyToggle != null && self != null)
                _readyToggle.SetIsOnWithoutNotify(self.isReady);
        }

        private static LobbyPlayerDto FindSelf(LobbyPlayerDto[] players, string userId)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].userId == userId)
                    return players[i];
            }

            return null;
        }

        private void OnLogoutClicked()
        {
            _authService?.Logout();
            _currentLobby = null;
            _stateMachine.Enter(GameState.Login);
            ShowPanel(_loginPanel);
            SetStatus("Logged out.");
        }

        private void OnLeaveRoomClicked()
        {
            SessionStorage.ClearLobby();
            _currentLobby = null;
            _stateMachine.Enter(GameState.LobbyBrowser);
            ShowPanel(_browserPanel);
            _ = RefreshLobbiesAsync();
        }

        private void ShowPanel(GameObject panel)
        {
            if (_loginPanel != null)
                _loginPanel.SetActive(panel == _loginPanel);
            if (_browserPanel != null)
                _browserPanel.SetActive(panel == _browserPanel);
            if (_roomPanel != null)
                _roomPanel.SetActive(panel == _roomPanel);
        }

        private void ClearLobbyList()
        {
            if (_lobbyListRoot == null)
                return;

            for (int i = _lobbyListRoot.childCount - 1; i >= 0; i--)
                Destroy(_lobbyListRoot.GetChild(i).gameObject);
        }

        private void SetGameplayActive(bool active)
        {
            if (context != null && context.Player != null)
                context.Player.gameObject.SetActive(active);

            if (context != null && context.ActorsRoot != null)
                context.ActorsRoot.gameObject.SetActive(active);
        }

        private void WireGameplayIfNeeded()
        {
            if (_gameplayWired || context == null)
                return;

            Result wireResult = context.TryWirePlayerAndCamera();
            if (!wireResult.Succeeded)
                Debug.LogError("[InterviewArena] " + wireResult.Error, context);

            _gameplayWired = true;
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message ?? string.Empty;
        }

        private static string FormatError<T>(ApiResult<T> result) =>
            string.IsNullOrEmpty(result.ErrorCode)
                ? result.Error ?? "Request failed."
                : result.ErrorCode + ": " + result.Error;

        private GameObject CreatePanel(string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(panelRoot, false);
            Stretch(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.92f);
            panel.SetActive(false);
            return panel;
        }

        private static TMP_InputField CreateInput(Transform parent, string name, Vector2 anchoredPosition)
        {
            GameObject host = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            host.transform.SetParent(parent, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 36f);
            host.GetComponent<Image>().color = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.95f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(host.transform, false);
            Stretch(textObject.GetComponent<RectTransform>());
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = 16f;
            text.color = ShowcasePalette.TextPrimary;
            text.margin = new Vector4(10f, 4f, 10f, 4f);

            TMP_InputField input = host.GetComponent<TMP_InputField>();
            input.textComponent = text;
            return input;
        }

        private static TMP_Text CreateLabel(
            Transform parent,
            string name,
            float fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject host = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            host.transform.SetParent(parent, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            TextMeshProUGUI label = host.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.color = ShowcasePalette.TextPrimary;
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject host = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            host.transform.SetParent(parent, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(220f, 36f);
            host.GetComponent<Image>().color = ShowcasePalette.AccentSoft(0.85f);

            GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(host.transform, false);
            Stretch(labelObject.GetComponent<RectTransform>());
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.fontSize = 14f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = ShowcasePalette.TextPrimary;
            label.text = name.Replace("Button - ", string.Empty);

            Button button = host.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            return button;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            UnityEngine.Events.UnityAction onClick,
            string labelText)
        {
            Button button = CreateButton(parent, name, anchoredPosition, onClick);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = labelText;
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
