namespace LearningArchitect.Modules.AI
{
    public interface IAiDecisionModel
    {
        void Tick(AiWorld world, float deltaTime);
    }
}
