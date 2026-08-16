using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.VFX
{
    public sealed class VfxModule : MonoBehaviour, IModule
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
