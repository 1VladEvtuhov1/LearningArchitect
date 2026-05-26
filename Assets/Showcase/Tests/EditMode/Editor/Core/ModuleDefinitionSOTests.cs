using LearningArchitect.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ModuleDefinitionSOTests
    {
        [Test]
        public void Category_IsSerialized_ForTaxonomy()
        {
            ModuleDefinitionSO module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();

            try
            {
                SetCategory(module, ShowcaseModuleCategory.ArchitecturePatternModule);
                Assert.That(module.Category, Is.EqualTo(ShowcaseModuleCategory.ArchitecturePatternModule));

                SetCategory(module, ShowcaseModuleCategory.SimulationModule);
                Assert.That(module.Category, Is.EqualTo(ShowcaseModuleCategory.SimulationModule));
            }
            finally
            {
                Object.DestroyImmediate(module);
            }
        }

        private static void SetCategory(ModuleDefinitionSO module, ShowcaseModuleCategory category)
        {
            SerializedObject serializedModule = new(module);
            serializedModule.FindProperty("category").enumValueIndex = (int)category;
            serializedModule.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
