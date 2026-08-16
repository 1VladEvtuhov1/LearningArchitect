using LearningArchitect.Modules.InterviewArena.Services;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Local death: freeze while HP is 0, then restore at spawn. Skips respawn during an online match.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerActionCoordinator))]
    public sealed class PlayerDeathController : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Collider arenaFloor;

        private Health health;
        private PlayerMotor motor;
        private PlayerActionCoordinator actionCoordinator;
        private MeleeStrikeController melee;
        private CrossbowWeaponController crossbow;
        private Vector3 spawnPlanar;
        private Quaternion spawnRotation;
        private float respawnTimer = -1f;
        private bool spawnCaptured;

        public void ApplyConfig(PlayerConfig playerConfig, Collider floorCollider)
        {
            config = playerConfig;
            arenaFloor = floorCollider;
        }

        public void CaptureSpawnPose()
        {
            spawnPlanar = transform.position;
            spawnRotation = transform.rotation;
            spawnCaptured = true;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            motor = GetComponent<PlayerMotor>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
            melee = GetComponent<MeleeStrikeController>();
            crossbow = GetComponent<CrossbowWeaponController>();
        }

        private void Start()
        {
            if (!spawnCaptured)
                CaptureSpawnPose();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= HandleDied;
        }

        private void Update()
        {
            if (!PlayerDeathRespawnRules.TickDue(ref respawnTimer, Time.deltaTime))
                return;

            Respawn();
        }

        private void HandleDied(Health _)
        {
            if (!PlayerDeathRespawnRules.ShouldLocalRespawn(SessionStorage.HasActiveMatch))
                return;

            InterruptActions();
            respawnTimer = PlayerDeathRespawnRules.ResolveDelay(
                config != null ? config.RespawnDelay : 0f);
            InterviewArenaRunSession.NotifyPlayerDied();
        }

        private void Respawn()
        {
            if (!spawnCaptured)
                CaptureSpawnPose();

            InterruptActions();
            health.RestoreFullHealth();
            motor.SnapToSpawnPose(spawnPlanar, spawnRotation, arenaFloor);
            InterviewArenaRunSession.NotifyPlayerRespawned();
        }

        private void InterruptActions()
        {
            if (motor != null)
                motor.InterruptDash();
            if (melee != null)
                melee.InterruptStrike();
            if (crossbow != null)
                crossbow.InterruptShot();
            if (actionCoordinator != null)
                actionCoordinator.ClearAllLocks();
        }
    }
}
