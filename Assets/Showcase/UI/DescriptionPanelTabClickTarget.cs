using UnityEngine;
using UnityEngine.EventSystems;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    public sealed class DescriptionPanelTabClickTarget : MonoBehaviour, IPointerClickHandler
    {
        private DescriptionPanel owner;
        private int tabIndex;

        public void Initialize(DescriptionPanel owner, int tabIndex)
        {
            this.owner = owner;
            this.tabIndex = tabIndex;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || owner == null)
                return;

            owner.SetTab(tabIndex);
        }
    }
}
