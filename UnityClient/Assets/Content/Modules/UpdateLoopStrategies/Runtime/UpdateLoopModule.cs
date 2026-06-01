using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Performance
{
    public sealed class UpdateLoopModule : MonoBehaviour, IModule
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
