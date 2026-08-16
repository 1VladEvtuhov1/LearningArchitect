using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class CrossbowWeaponController : MonoBehaviour
    {
        [SerializeField] private CrossbowWeaponConfig config;
        [SerializeField] private Transform muzzle;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Player;

        private readonly MeleeComboState comboState = new MeleeComboState();
        private float cooldownTimer;
        private IClampedAimProvider clampedAim;
        private ArenaCursorAim cursorAim;
        private PlayerActionCoordinator actionCoordinator;
        private Rigidbody body;
        private CrossbowShotDefinition activeShot;
        private int activeShotIndex = -1;
        private float shotElapsed;
        private bool shotActive;
        private int fireAnimStartVersion;

        public CrossbowWeaponConfig Config => config;
        public CombatTeam SourceTeam => sourceTeam;
        public Transform Muzzle => muzzle;
        public bool IsShotActive => shotActive;
        public bool IsShootingAnimActive => shotActive;
        public int FireAnimStartVersion => fireAnimStartVersion;
        public CrossbowShotDefinition ActiveShot => activeShot;
        public int ActiveShotIndex => activeShotIndex;

        private void Awake()
        {
            clampedAim = GetComponent<IClampedAimProvider>();
            cursorAim = GetComponent<ArenaCursorAim>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
            body = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            if (shotActive)
                InterruptShot();
            else
                comboState.Clear();
        }

        public void ApplyConfig(
            CrossbowWeaponConfig weaponConfig,
            Transform fireOrigin,
            ProjectilePool pool,
            CombatTeam team)
        {
            config = weaponConfig;
            muzzle = fireOrigin;
            projectilePool = pool;
            sourceTeam = team;
        }

        /// <summary>
        /// Idle → start shot 0; active → queue combo when inside the input window.
        /// </summary>
        public AttackInputResult HandleFireInput()
        {
            if (config == null)
                return AttackInputResult.Rejected;

            if (shotActive)
            {
                if (comboState.TryQueue(activeShot, shotElapsed, config.ShotCount, out _))
                    return AttackInputResult.QueuedNextStrike;

                return AttackInputResult.Rejected;
            }

            if (cooldownTimer > 0f)
                return AttackInputResult.Rejected;

            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate != null && !gate.CanAttack)
                return AttackInputResult.Rejected;

            return TryBeginShot(0) ? AttackInputResult.StartedStrike : AttackInputResult.Rejected;
        }

        /// <summary>Legacy single-fire entry used by older callers/tests.</summary>
        public bool TryFire() => HandleFireInput() != AttackInputResult.Rejected;

        public void InterruptShot()
        {
            comboState.Clear();

            if (!shotActive)
            {
                activeShot = null;
                activeShotIndex = -1;
                return;
            }

            shotActive = false;
            shotElapsed = 0f;
            activeShot = null;
            activeShotIndex = -1;
            ClearFireLock();
        }

        public void TickActiveShot(float deltaTime)
        {
            if (!shotActive)
                return;

            if (config == null || activeShot == null)
            {
                InterruptShot();
                return;
            }

            if (deltaTime < 0f)
                deltaTime = 0f;

            shotElapsed += deltaTime;

            if (comboState.ShouldTransition(activeShot, shotElapsed))
            {
                TransitionToQueuedShot();
                return;
            }

            PublishFireLock();

            if (shotElapsed >= activeShot.ShotDuration)
                FinishShotToIdle();
        }

        private bool TryBeginShot(int shotIndex)
        {
            if (config == null || projectilePool == null)
                return false;

            if (!config.TryGetShot(shotIndex, out CrossbowShotDefinition shot))
                return false;

            if (!TryLaunchProjectile())
                return false;

            BeginShot(shotIndex, shot);
            return true;
        }

        private void BeginShot(int shotIndex, CrossbowShotDefinition shot)
        {
            comboState.Clear();
            activeShotIndex = shotIndex;
            activeShot = shot;
            shotActive = true;
            shotElapsed = 0f;
            fireAnimStartVersion++;
            ZeroPlanarVelocity();
            PublishFireLock();
        }

        private void TransitionToQueuedShot()
        {
            if (!comboState.TryConsumeQueue(out int nextIndex))
                return;

            if (config == null || !config.TryGetShot(nextIndex, out CrossbowShotDefinition nextShot))
            {
                FinishShotToIdle();
                return;
            }

            if (!TryLaunchProjectile())
            {
                FinishShotToIdle();
                return;
            }

            activeShotIndex = nextIndex;
            activeShot = nextShot;
            shotActive = true;
            shotElapsed = 0f;
            fireAnimStartVersion++;
            ZeroPlanarVelocity();
            PublishFireLock();
        }

        private void FinishShotToIdle()
        {
            comboState.Clear();
            shotActive = false;
            shotElapsed = 0f;
            activeShot = null;
            activeShotIndex = -1;
            ClearFireLock();
            cooldownTimer = config != null ? config.Cooldown : 0f;
        }

        private bool TryLaunchProjectile()
        {
            if (config == null || projectilePool == null)
                return false;

            projectilePool.EnsureInitialized(
                config,
                sourceTeam,
                transform.root.gameObject.GetInstanceID());

            Vector3 fireDirection = ResolveFireDirection();
            Vector3 spawnPosition = ResolveSpawnPosition(fireDirection);

            if (!projectilePool.TryLaunch(spawnPosition, fireDirection, config, out _))
                return false;

            CombatHitFeedback.PlayCrossbowFire(spawnPosition, fireDirection);
            return true;
        }

        private void PublishFireLock()
        {
            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate == null || activeShot == null)
                return;

            float remaining = activeShot.ShotDuration - shotElapsed;
            bool inMoveLock = shotElapsed < activeShot.MovementLockDuration;
            gate.SetLock(CharacterActionLock.CrossbowFire(remaining, inMoveLock));
        }

        private void ClearFireLock()
        {
            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate != null)
                gate.ClearLock(CharacterActionKind.CrossbowFire);
        }

        private PlayerActionCoordinator ResolveActionCoordinator()
        {
            if (actionCoordinator == null)
                actionCoordinator = GetComponent<PlayerActionCoordinator>();
            return actionCoordinator;
        }

        private void ZeroPlanarVelocity()
        {
            if (body == null)
                return;

            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = new Vector3(0f, velocity.y, 0f);
        }

        private Vector3 ResolveFireDirection()
        {
            if (clampedAim == null)
                clampedAim = GetComponent<IClampedAimProvider>();

            if (clampedAim != null && clampedAim.TargetClampedAimDirection.sqrMagnitude > 0.0001f)
            {
                Vector3 clamped = clampedAim.TargetClampedAimDirection;
                clamped.y = 0f;
                if (clamped.sqrMagnitude > 0.0001f)
                    return clamped.normalized;
            }

            Vector3 fallback = transform.forward;
            fallback.y = 0f;
            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
        }

        private Vector3 ResolveSpawnPosition(Vector3 fireDirection)
        {
            if (muzzle == null)
                return transform.position + Vector3.up * 1.1f + fireDirection * 0.55f;

            if (IsMuzzleUnderCursorAimPivot())
                return RebuildSpawnAlongFireDirection(transform.position, muzzle.position, fireDirection);

            return muzzle.position;
        }

        public static Vector3 RebuildSpawnAlongFireDirection(
            Vector3 rootPosition,
            Vector3 muzzlePosition,
            Vector3 fireDirection)
        {
            float height = muzzlePosition.y - rootPosition.y;
            Vector3 planar = Vector3.ProjectOnPlane(muzzlePosition - rootPosition, Vector3.up);
            float radius = Mathf.Max(0.35f, planar.magnitude);
            Vector3 planarFire = Vector3.ProjectOnPlane(fireDirection, Vector3.up);
            if (planarFire.sqrMagnitude < 0.0001f)
                planarFire = Vector3.forward;
            return rootPosition + Vector3.up * height + planarFire.normalized * radius;
        }

        private bool IsMuzzleUnderCursorAimPivot()
        {
            if (muzzle == null)
                return false;

            if (cursorAim == null)
                cursorAim = GetComponent<ArenaCursorAim>();

            Transform pivot = cursorAim != null ? cursorAim.AimPivot : null;
            if (pivot == null)
                return false;

            Transform current = muzzle;
            while (current != null && current != transform)
            {
                if (current == pivot)
                    return true;
                current = current.parent;
            }

            return false;
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
            TickActiveShot(Time.deltaTime);
        }
    }
}
