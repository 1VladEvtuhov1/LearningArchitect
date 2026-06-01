using System;
using UnityEngine;

namespace LearningArchitect.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseRuntimeController))]
    public sealed class ShowcaseCommandRouter : MonoBehaviour
    {
        [SerializeField] private ShowcaseRuntimeController runtimeController;

        public ShowcaseRuntimeController RuntimeController
        {
            get => runtimeController;
            set => runtimeController = value;
        }

        private void Awake()
        {
            runtimeController = runtimeController != null
                ? runtimeController
                : GetComponent<ShowcaseRuntimeController>();

            if (runtimeController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} is required.");
        }

        public void Configure(ShowcaseRuntimeController runtimeController)
        {
            this.runtimeController = runtimeController != null
                ? runtimeController
                : GetComponent<ShowcaseRuntimeController>();

            if (this.runtimeController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} is required.");
        }

        public void PreviousModule()
        {
            runtimeController.PreviousModule();
        }

        public void NextModule()
        {
            runtimeController.NextModule();
        }

        public void NextCategory()
        {
            runtimeController.NextCategory();
        }

        public void PreviousVariant()
        {
            runtimeController.PreviousVariant();
        }

        public void NextVariant()
        {
            runtimeController.NextVariant();
        }

        public void SetStressLevel(int level)
        {
            runtimeController.SetStressLevel(level);
        }

        public void LoadSelection(int moduleIndex, int variantIndex)
        {
            runtimeController.LoadSelection(moduleIndex, variantIndex);
        }
    }
}
