using LearningArchitect.Modules.InterviewArena.Net;
using LearningArchitect.Modules.InterviewArena.Services;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Submits match results to backend when finish portal or player death ends an online run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterviewArenaMatchReporter : MonoBehaviour
    {
        [SerializeField] private InterviewArenaRuntimeContext context;

        private MatchService _matchService;
        private Health _playerHealth;
        private bool _submitted;
        private bool _deathBound;

        private void Awake()
        {
            context = context != null ? context : GetComponent<InterviewArenaRuntimeContext>();
        }

        private void OnEnable()
        {
            InterviewArenaRunSession.RunCompleted += HandleRunCompleted;

            if (context?.StateMachine != null)
                context.StateMachine.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            InterviewArenaRunSession.RunCompleted -= HandleRunCompleted;
            UnbindPlayerDeath();

            if (context?.StateMachine != null)
                context.StateMachine.StateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            if (context?.BackendConfig == null || !context.BackendConfig.UseOnlineFlow)
                return;

            if (!SessionStorage.HasActiveMatch)
                return;

            _matchService = new MatchService(new UnityWebRequestBackendApiClient(
                context.BackendConfig.BaseUrl,
                context.BackendConfig.TimeoutSeconds));

            TryBindPlayerDeath();
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.LocalArena)
                TryBindPlayerDeath();
        }

        private void HandleRunCompleted()
        {
            _ = SubmitOnceAsync(MatchOutcomeValues.Extracted);
        }

        private void HandlePlayerDied(Health health)
        {
            _ = SubmitOnceAsync(MatchOutcomeValues.Death);
        }

        private async System.Threading.Tasks.Task SubmitOnceAsync(string outcome)
        {
            if (_submitted || _matchService == null || !SessionStorage.HasActiveMatch)
                return;

            _submitted = true;
            string matchId = SessionStorage.GetMatchId();
            float elapsed = SessionStorage.GetMatchElapsedSeconds();

            ApiResult<MatchResultResponseDto> result = await _matchService.SubmitResultAsync(
                matchId,
                outcome,
                elapsed);

            if (!result.Succeeded)
            {
                _submitted = false;
                Debug.LogWarning("[InterviewArena] Match result submit failed: " + FormatError(result));
                return;
            }

            context?.StateMachine?.Enter(GameState.MatchResult);
            SessionStorage.ClearMatch();
        }

        private void TryBindPlayerDeath()
        {
            if (_deathBound || _submitted || context?.Player == null)
                return;

            Health health = context.Player.GetComponent<Health>();
            if (health == null)
                return;

            _playerHealth = health;
            _playerHealth.Died += HandlePlayerDied;
            _deathBound = true;
        }

        private void UnbindPlayerDeath()
        {
            if (_playerHealth != null)
                _playerHealth.Died -= HandlePlayerDied;

            _playerHealth = null;
            _deathBound = false;
        }

        private static string FormatError(ApiResult<MatchResultResponseDto> result) =>
            string.IsNullOrEmpty(result.ErrorCode)
                ? result.Error ?? "Request failed."
                : result.ErrorCode + ": " + result.Error;
    }
}
