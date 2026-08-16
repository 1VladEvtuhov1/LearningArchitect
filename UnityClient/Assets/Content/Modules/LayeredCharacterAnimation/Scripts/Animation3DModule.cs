using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    public sealed class Animation3DModule : MonoBehaviour, IModule
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
