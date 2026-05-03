using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Inventory
{
    public sealed class InventoryModule : MonoBehaviour, IModule
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
