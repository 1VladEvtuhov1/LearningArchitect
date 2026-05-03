using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Pooling
{
    public sealed class ObjectPoolingModule : MonoBehaviour, IModule
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
