using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Single source of truth for player action locks and capability gates.
    /// </summary>
    [DefaultExecutionOrder(-35)]
    [DisallowMultipleComponent]
    public sealed class PlayerActionCoordinator : MonoBehaviour, IPlayerActionGate
    {
        private const int MaxLocks = 6;

        private readonly CharacterActionLock[] locks = new CharacterActionLock[MaxLocks];
        private int lockCount;
        private Health health;

        public bool CanMove => IsAlive && CharacterActionGateRules.CanMove(locks, lockCount);
        public bool CanTurn => IsAlive && CharacterActionGateRules.CanTurn(locks, lockCount);
        public bool CanAttack => IsAlive && CharacterActionGateRules.CanAttack(locks, lockCount);
        public bool CanDash => IsAlive && CharacterActionGateRules.CanDash(locks, lockCount);
        public bool CanJump => IsAlive && CharacterActionGateRules.CanJump(locks, lockCount);
        public bool HasActiveAction => CharacterActionGateRules.HasActiveAction(locks, lockCount);

        private bool IsAlive => health == null || health.IsAlive;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            TickLocks(Time.deltaTime);
        }

        public void SetLock(CharacterActionLock actionLock)
        {
            for (int i = 0; i < lockCount; i++)
            {
                if (locks[i].Kind != actionLock.Kind)
                    continue;

                locks[i] = actionLock;
                return;
            }

            if (lockCount >= MaxLocks)
            {
                Debug.LogError("[PlayerActionCoordinator] Lock capacity exceeded.", this);
                return;
            }

            locks[lockCount++] = actionLock;
        }

        public bool HasLock(CharacterActionKind kind)
        {
            for (int i = 0; i < lockCount; i++)
            {
                if (locks[i].Kind == kind)
                    return true;
            }

            return false;
        }

        public void ClearLock(CharacterActionKind kind)
        {
            for (int i = 0; i < lockCount; i++)
            {
                if (locks[i].Kind != kind)
                    continue;

                locks[i] = locks[lockCount - 1];
                locks[lockCount - 1] = default;
                lockCount--;
                return;
            }
        }

        public void ClearAllLocks()
        {
            for (int i = 0; i < lockCount; i++)
                locks[i] = default;

            lockCount = 0;
        }

        public void ApplyHitstun(float durationSeconds)
        {
            if (durationSeconds <= 0f)
                return;

            SetLock(CharacterActionLock.Hitstun(durationSeconds));
        }

        private void TickLocks(float deltaTime)
        {
            for (int i = lockCount - 1; i >= 0; i--)
            {
                CharacterActionLock current = locks[i];
                float remaining = current.RemainingSeconds - deltaTime;
                if (remaining > 0f)
                {
                    locks[i] = new CharacterActionLock(
                        current.Kind,
                        remaining,
                        current.BlocksMovement,
                        current.BlocksTurning,
                        current.BlocksAttack,
                        current.BlocksDash,
                        current.BlocksJump);
                    continue;
                }

                locks[i] = locks[lockCount - 1];
                locks[lockCount - 1] = default;
                lockCount--;
            }
        }
    }
}
