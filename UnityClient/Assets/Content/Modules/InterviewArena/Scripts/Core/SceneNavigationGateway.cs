using LearningArchitect.Core;

namespace LearningArchitect.Modules.InterviewArena
{
    public sealed class SceneNavigationGateway : ISceneNavigation
    {
        public void LoadArchitectureShowcase()
        {
            ShowcaseSceneLoader.LoadArchitectureShowcase();
        }

        public void LoadInterviewArena()
        {
            ShowcaseSceneLoader.LoadInterviewArena();
        }
    }
}
