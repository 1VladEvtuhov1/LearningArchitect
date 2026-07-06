using System;

namespace LearningArchitect.Modules.InterviewArena
{
    public readonly struct CharacterActionLock
    {
        public CharacterActionKind Kind { get; }
        public bool BlocksMovement { get; }
        public bool BlocksTurning { get; }
        public bool BlocksAttack { get; }
        public bool BlocksDash { get; }
        public bool BlocksJump { get; }
        public float RemainingSeconds { get; }

        public CharacterActionLock(
            CharacterActionKind kind,
            float remainingSeconds,
            bool blocksMovement,
            bool blocksTurning,
            bool blocksAttack,
            bool blocksDash,
            bool blocksJump)
        {
            Kind = kind;
            RemainingSeconds = Math.Max(0f, remainingSeconds);
            BlocksMovement = blocksMovement;
            BlocksTurning = blocksTurning;
            BlocksAttack = blocksAttack;
            BlocksDash = blocksDash;
            BlocksJump = blocksJump;
        }

        public static CharacterActionLock Dash(float durationSeconds) =>
            new(CharacterActionKind.Dash, durationSeconds,
                blocksMovement: true,
                blocksTurning: false,
                blocksAttack: true,
                blocksDash: true,
                blocksJump: true);

        public static CharacterActionLock Jump(float durationSeconds) =>
            new(CharacterActionKind.Jump, durationSeconds,
                blocksMovement: false,
                blocksTurning: false,
                blocksAttack: true,
                blocksDash: true,
                blocksJump: true);

        public static CharacterActionLock Hitstun(float durationSeconds) =>
            new(CharacterActionKind.Hitstun, durationSeconds,
                blocksMovement: true,
                blocksTurning: true,
                blocksAttack: true,
                blocksDash: true,
                blocksJump: true);

        public static CharacterActionLock MeleeStrike(
            float remainingSeconds,
            bool inMovementStartup) =>
            new(CharacterActionKind.MeleeStrike, remainingSeconds,
                blocksMovement: inMovementStartup,
                blocksTurning: inMovementStartup,
                blocksAttack: true,
                blocksDash: true,
                blocksJump: true);
    }
}
