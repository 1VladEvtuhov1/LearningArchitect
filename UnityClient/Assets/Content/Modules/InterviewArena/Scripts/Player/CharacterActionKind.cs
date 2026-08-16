namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Active character action sources used as lock slot keys (one lock per kind).
    /// </summary>
    public enum CharacterActionKind
    {
        Jump = 0,
        MeleeStrike = 1,
        Hitstun = 2,
        Dash = 3,
        CrossbowFire = 4,
        Block = 5,
    }
}
