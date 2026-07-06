namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Buffered input resolution order (highest first).
    /// </summary>
    public enum CharacterActionPriority
    {
        Locomotion = 0,
        Jump = 10,
        Attack = 20,
        Hitstun = 30,
        Dash = 40,
    }
}
