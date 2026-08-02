namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Merges active action locks into capability gates (OR across locks).
    /// </summary>
    public static class CharacterActionGateRules
    {
        public static bool CanMove(CharacterActionLock[] locks, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (locks[i].BlocksMovement)
                    return false;
            }

            return true;
        }

        public static bool CanTurn(CharacterActionLock[] locks, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (locks[i].BlocksTurning)
                    return false;
            }

            return true;
        }

        public static bool CanAttack(CharacterActionLock[] locks, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (locks[i].BlocksAttack)
                    return false;
            }

            return true;
        }

        public static bool CanDash(CharacterActionLock[] locks, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (locks[i].BlocksDash)
                    return false;
            }

            return true;
        }

        public static bool CanJump(CharacterActionLock[] locks, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (locks[i].BlocksJump)
                    return false;
            }

            return true;
        }

        public static bool HasActiveAction(CharacterActionLock[] locks, int count)
        {
            // Any lock (including hitstun) suppresses locomotion anim / turn-in-place commitment.
            return count > 0;
        }
    }
}
