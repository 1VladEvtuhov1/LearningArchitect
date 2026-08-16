using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Effects
{
    public sealed class EffectsModule : MonoBehaviour, IModule
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
