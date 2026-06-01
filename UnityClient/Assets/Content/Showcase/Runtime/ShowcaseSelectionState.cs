using System;
using UnityEngine;

namespace LearningArchitect.Core
{
    public sealed class ShowcaseSelectionState
    {
        private int currentModuleIndex;
        private int currentVariantIndex;

        public int CurrentModuleIndex => currentModuleIndex;

        public int CurrentVariantIndex => currentVariantIndex;

        public void Reset()
        {
            currentModuleIndex = 0;
            currentVariantIndex = 0;
        }

        public void MoveToNextModule(int moduleCount)
        {
            if (moduleCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(moduleCount));

            currentModuleIndex = (currentModuleIndex + 1) % moduleCount;
            currentVariantIndex = 0;
        }

        public void MoveToPreviousModule(int moduleCount)
        {
            if (moduleCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(moduleCount));

            currentModuleIndex = (currentModuleIndex + moduleCount - 1) % moduleCount;
            currentVariantIndex = 0;
        }

        public void MoveToNextVariant(int variantCount)
        {
            if (variantCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(variantCount));

            currentVariantIndex = (currentVariantIndex + 1) % variantCount;
        }

        public void MoveToPreviousVariant(int variantCount)
        {
            if (variantCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(variantCount));

            currentVariantIndex = (currentVariantIndex + variantCount - 1) % variantCount;
        }

        public void SetSelection(int moduleIndex, int variantIndex, ModuleDefinitionSO[] modules)
        {
            if (modules == null || modules.Length == 0)
                throw new InvalidOperationException($"{nameof(ShowcaseSelectionState)} requires at least one module.");

            currentModuleIndex = Mathf.Clamp(moduleIndex, 0, modules.Length - 1);

            VariantDefinitionSO[] variants = modules[currentModuleIndex].Variants;
            if (variants == null || variants.Length == 0)
            {
                currentVariantIndex = 0;
                return;
            }

            currentVariantIndex = Mathf.Clamp(variantIndex, 0, variants.Length - 1);
        }

        public void Clamp(ModuleDefinitionSO[] modules)
        {
            if (modules == null || modules.Length == 0)
                throw new InvalidOperationException($"{nameof(ShowcaseSelectionState)} requires at least one module.");

            currentModuleIndex = Mathf.Clamp(currentModuleIndex, 0, modules.Length - 1);

            VariantDefinitionSO[] variants = modules[currentModuleIndex].Variants;
            if (variants == null || variants.Length == 0)
            {
                currentVariantIndex = 0;
                return;
            }

            currentVariantIndex = Mathf.Clamp(currentVariantIndex, 0, variants.Length - 1);
        }
    }
}
