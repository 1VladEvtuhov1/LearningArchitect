using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.AI
{
    public sealed class AiSystemModule : MonoBehaviour, IModule
    {
        public void Enter()
        {
            gameObject.SetActive(true);
        }

        public void Exit()
        {
            gameObject.SetActive(false);
        }
    }
}
