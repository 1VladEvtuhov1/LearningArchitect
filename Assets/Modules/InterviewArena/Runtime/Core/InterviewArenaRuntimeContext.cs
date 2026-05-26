using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Scene-level composition root: state, config, and authored references. No runtime entity spawning.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InterviewArenaRuntimeContext : MonoBehaviour
    {
        [Header("Scene roots")]
        [SerializeField] private Transform levelRoot;
        [SerializeField] private Transform actorsRoot;
        [SerializeField] private Transform runtimeRoot;
        [SerializeField] private Transform projectilesRoot;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private Transform canvasHudDynamic;

        [Header("Authoring")]
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private Transform arenaRoot;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private PlayerMotor player;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private InterviewArenaCameraFollow cameraFollow;
        [SerializeField] private InterviewArenaCombatServices combatServices;

        private GameStateMachine stateMachine;

        public GameStateMachine StateMachine => stateMachine;
        public PlayerConfig PlayerConfig => playerConfig;
        public PlayerMotor Player => player;
        public Transform SpawnPoint => spawnPoint;
        public Transform LevelRoot => levelRoot != null ? levelRoot : arenaRoot;
        public Transform ActorsRoot => actorsRoot;
        public Transform RuntimeRoot => runtimeRoot;
        public Transform ProjectilesRoot => projectilesRoot;
        public Transform VfxRoot => vfxRoot;

        private void Awake()
        {
            stateMachine = new GameStateMachine();
            stateMachine.Enter(GameState.LocalArena);
        }

        public Result TryWirePlayerAndCamera()
        {
            if (player == null)
            {
                return Result.Failure(
                    "Assign a player prefab instance in the scene (Player field). Runtime spawn is intentionally disabled.");
            }

            if (playerConfig != null)
                player.ApplyConfig(playerConfig, ResolveMovementCamera());

            Transform followTarget = player.ViewPivot != null ? player.ViewPivot : player.transform;
            InterviewArenaCameraFollow follow = ResolveCameraFollow();
            if (follow != null)
                follow.SetTarget(followTarget);

            WirePlayerIframeHud();
            WirePlayerCombatServices();
            SnapPlayerToSpawn();

            return Result.Success();
        }

        private void SnapPlayerToSpawn()
        {
            if (spawnPoint == null)
                return;

            Collider floor = ResolveArenaFloorCollider();
            player.ConfigureArenaFloor(floor);
            player.SnapToSpawnPose(spawnPoint.position, spawnPoint.rotation, floor);
        }

        private Collider ResolveArenaFloorCollider()
        {
            Transform root = LevelRoot;
            if (root == null)
                return null;

            Transform platform = root.Find("Static/PreviewPlatform");
            if (platform == null)
                platform = root.Find("PreviewPlatform");

            return platform != null ? platform.GetComponent<Collider>() : null;
        }

        private void WirePlayerCombatServices()
        {
            InterviewArenaCombatServices services = ResolveCombatServices();
            if (services == null)
                return;

            services.ConfigurePlayerProjectilePool(projectilesRoot);
            services.WirePlayerCrossbow(player);
        }

        private InterviewArenaCombatServices ResolveCombatServices()
        {
            if (combatServices != null)
                return combatServices;

            return FindFirstObjectByType<InterviewArenaCombatServices>();
        }

        private void WirePlayerIframeHud()
        {
            Health health = player.GetComponent<Health>();
            if (health == null)
                return;

            PlayerIframeHud hud = canvasHudDynamic != null
                ? canvasHudDynamic.GetComponentInChildren<PlayerIframeHud>(true)
                : null;
            if (hud == null)
                hud = FindFirstObjectByType<PlayerIframeHud>();

            if (hud != null)
                hud.BindPlayerHealth(health);
        }

        private Camera ResolveMovementCamera()
        {
            return Camera.main;
        }

        private InterviewArenaCameraFollow ResolveCameraFollow()
        {
            if (cameraFollow != null)
                return cameraFollow;

            Camera main = Camera.main;
            return main != null ? main.GetComponent<InterviewArenaCameraFollow>() : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (player == null || spawnPoint == null)
                return;

            Transform parent = actorsRoot != null ? actorsRoot : LevelRoot;
            if (parent != null && player.transform.parent != parent)
                player.transform.SetParent(parent, true);

            Collider floor = ResolveArenaFloorCollider();
            float halfHeight = InterviewArenaPlatformLayout.ResolveCapsuleHalfHeight(
                player.GetComponent<CapsuleCollider>());
            Vector3 spawnPosition = InterviewArenaPlatformLayout.ResolveSpawnPosition(
                spawnPoint.position,
                halfHeight,
                floor);
            player.transform.SetPositionAndRotation(spawnPosition, spawnPoint.rotation);
        }
#endif
    }
}
