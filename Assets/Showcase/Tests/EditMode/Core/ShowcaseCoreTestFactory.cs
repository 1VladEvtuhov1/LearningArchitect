using System;
using System.Reflection;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    internal static class ShowcaseCoreTestFactory
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        public static ModuleDefinitionSO CreateModule(string assetName, ShowcaseModuleCategory category, params VariantDefinitionSO[] variants)
        {
            ModuleDefinitionSO module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
            module.name = assetName;
            SetField(module, "category", category);
            SetField(module, "moduleName", assetName);
            SetField(module, "variants", variants);
            return module;
        }

        public static VariantDefinitionSO CreateVariant(string assetName, GameObject prefab, params int[] stressPresets)
        {
            VariantDefinitionSO variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
            variant.name = assetName;
            SetField(variant, "variantName", assetName);
            SetField(variant, "prefab", prefab);
            SetField(variant, "stressPresets", stressPresets);
            return variant;
        }

        public static GameObject CreateRuntimePrefab(string name)
        {
            GameObject prefab = new(name);
            prefab.AddComponent<ShowcaseTestRuntimeModule>();
            return prefab;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName, fieldName);

            field.SetValue(target, value);
        }
    }

    internal sealed class ShowcaseTestRuntimeModule : MonoBehaviour, IModule, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        public int ActiveCountValue { get; private set; }
        public int LastStressLevel { get; private set; }
        public int EnterCallCount { get; private set; }
        public int ExitCallCount { get; private set; }

        public int ActiveCount => ActiveCountValue;

        public void Enter()
        {
            EnterCallCount++;
            gameObject.SetActive(true);
        }

        public void Exit()
        {
            ExitCallCount++;
            gameObject.SetActive(false);
        }

        public void SetStressLevel(int count)
        {
            LastStressLevel = count;
            ActiveCountValue = count;
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(ActiveCountValue, ActiveCountValue, ActiveCountValue * 2, 0.25f);
        }
    }
}
