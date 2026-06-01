namespace LearningArchitect.Core
{
    /// <summary>
    /// Build settings scene names (file name without path). Used with <see cref="UnityEngine.SceneManagement.SceneManager"/>.
    /// </summary>
    public static class ShowcaseSceneNames
    {
        public const string ArchitectureShowcase = "ArchitectureShowcase";
        public const string InterviewArena = "InterviewArena";

        public const string ContentRoot = "Assets/Content";

        public const string ArchitectureShowcasePath = ContentRoot + "/Showcase/Scenes/ArchitectureShowcase.unity";
        public const string InterviewArenaPath = ContentRoot + "/Modules/InterviewArena/Scenes/InterviewArena.unity";
    }
}
