namespace LearningArchitect.Modules.InterviewArena
{
    public static class CombatRules
    {
        public static bool CanDamage(CombatTeam sourceTeam, CombatTeam targetTeam)
        {
            if (targetTeam == CombatTeam.Neutral)
                return false;

            if (sourceTeam == targetTeam)
                return false;

            return sourceTeam == CombatTeam.Player && targetTeam == CombatTeam.Enemy
                || sourceTeam == CombatTeam.Enemy && targetTeam == CombatTeam.Player;
        }
    }
}
