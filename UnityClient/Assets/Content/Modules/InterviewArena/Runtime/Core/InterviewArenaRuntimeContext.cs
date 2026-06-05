using System.Collections.Generic;
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
        [SerializeField] private Collider arenaFloorCollider;
        [SerializeField] private Transform arenaRoot;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private PlayerMotor player;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private InterviewArenaCameraFollow cameraFollow;
        [SerializeField] private InterviewArenaCombatServices combatServices;
        [SerializeField] private PlayerIframeHud playerIframeHud;
        [SerializeField] private PlayerBuffHud playerBuffHud;

        [Header("Online")]
        [SerializeField] private BackendApiConfig backendConfig;
        [SerializeField] private InterviewArenaOnlineFlowController onlineFlow;

        private GameStateMachine stateMachine;

        public GameStateMachine StateMachine => stateMachine;
        public BackendApiConfig BackendConfig => backendConfig;
        public InterviewArenaOnlineFlowController OnlineFlow => onlineFlow;
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
            stateMachine.Enter(GameState.Boot);
        }

        public bool ShouldDeferGameplayWire()
        {
            if (backendConfig == null || !backendConfig.UseOnlineFlow)
                return false;

            InterviewArenaOnlineFlowController flow = onlineFlow;
            if (flow == null)
                flow = GetComponentInChildren<InterviewArenaOnlineFlowController>(true);

            return flow != null;
        }

        public Result TryWirePlayerAndCamera()
        {
            List<string> missing = new List<string>(10);

            if (player == null)
                missing.Add(nameof(player));
            if (spawnPoint == null)
                missing.Add(nameof(spawnPoint));
            if (playerConfig == null)
                missing.Add(nameof(playerConfig));
            if (arenaFloorCollider == null)
                missing.Add(nameof(arenaFloorCollider));
            if (combatServices == null)
                missing.Add(nameof(combatServices));
            if (cameraFollow == null)
                missing.Add(nameof(cameraFollow));
            if (playerIframeHud == null)
                missing.Add(nameof(playerIframeHud));
            if (playerBuffHud == null)
                missing.Add(nameof(playerBuffHud));
            if (projectilesRoot == null)
                missing.Add(nameof(projectilesRoot));

            if (missing.Count > 0)
            {
                for (int i = 0; i < missing.Count; i++)
                    InterviewArenaAuthoringLog.MissingReference(this, missing[i]);

                return Result.Failure(
                    "Scene wiring failed. Missing: " + string.Join(", ", missing) + ".");
            }

            if (actorsRoot == null)
                InterviewArenaAuthoringLog.MissingReference(this, nameof(actorsRoot));

            player.ApplyConfig(playerConfig, cameraFollow.GetComponent<Camera>());

            Transform followTarget = player.ViewPivot != null ? player.ViewPivot : player.transform;
            cameraFollow.SetTarget(followTarget);

            WirePlayerIframeHud();
            WirePlayerBuffHud();
            WirePlayerCombatServices();
            WireSceneEnemies();
            SnapPlayerToSpawn();

            return Result.Success();
        }

        private void SnapPlayerToSpawn()
        {
            player.ConfigureArenaFloor(arenaFloorCollider);
            player.SnapToSpawnPose(spawnPoint.position, spawnPoint.rotation, arenaFloorCollider);
        }

        private void WirePlayerCombatServices()
        {
            combatServices.ConfigurePlayerProjectilePool(projectilesRoot);
            combatServices.WirePlayerCrossbow(player);
        }

        private void WirePlayerIframeHud()
        {
            Health health = player.GetComponent<Health>();
            if (health == null)
            {
                InterviewArenaAuthoringLog.MissingReference(player, "Health");
                return;
            }

            playerIframeHud.BindPlayerHealth(health);
        }

        private void WirePlayerBuffHud()
        {
            PlayerBuffController buff = player.GetComponent<PlayerBuffController>();
            if (buff == null)
            {
                InterviewArenaAuthoringLog.MissingReference(player, nameof(PlayerBuffController));
                return;
            }

            PlayerBuffPickupInteractor interactor = player.GetComponent<PlayerBuffPickupInteractor>();
            if (interactor == null)
                InterviewArenaAuthoringLog.MissingReference(player, nameof(PlayerBuffPickupInteractor));

            playerBuffHud.Bind(buff, interactor);
        }

        private void WireSceneEnemies()
        {
            if (actorsRoot == null || player == null)
                return;

            EnemyBrain[] enemies = actorsRoot.GetComponentsInChildren<EnemyBrain>(true);
            for (int i = 0; i < enemies.Length; i++)
                enemies[i].BindPlayer(player.transform);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (player == null || spawnPoint == null)
                return;

            Transform parent = actorsRoot != null ? actorsRoot : LevelRoot;
            if (parent != null && player.transform.parent != parent)
                player.transform.SetParent(parent, true);

            float halfHeight = InterviewArenaPlatformLayout.ResolveCapsuleHalfHeight(
                player.GetComponent<CapsuleCollider>());
            Vector3 spawnPosition = InterviewArenaPlatformLayout.ResolveSpawnPosition(
                spawnPoint.position,
                halfHeight,
                arenaFloorCollider);
            player.transform.SetPositionAndRotation(spawnPosition, spawnPoint.rotation);
        }
#endif
    }
}
