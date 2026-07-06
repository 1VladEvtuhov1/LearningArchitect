namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Active character action sources. Higher <see cref="CharacterActionPriority"/> wins buffered input consume order.
    /// </summary>
    public enum CharacterActionKind
    {
        Locomotion = 0,
        Jump = 10,
        MeleeStrike = 20,
        Hitstun = 30,
        Dash = 40,
    }
}
