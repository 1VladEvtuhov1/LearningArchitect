namespace LearningArchitect.Core
{
    public interface IShowcaseStressTarget
    {
        int ActiveCount { get; }
        void SetStressLevel(int count);
    }
}
