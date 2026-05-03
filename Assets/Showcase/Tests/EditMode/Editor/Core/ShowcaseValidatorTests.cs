using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseValidatorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void ValidateDefinitions_ReportsErrorsAndWarnings_ForMissingPrefabContracts()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = new GameObject("VariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                variant = CreateVariant("VariantContracts", prefab, 1000, 2000, 4000);
                module = CreateModule("ModuleContracts", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("IShowcaseStressTarget")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("IShowcaseMetricsSource")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenStressPresetLabelsMismatchPresetCount()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantLabels", prefab, 500, 1000, 2000);
                SetField(variant, "stressPresetLabels", new[] { "Low", "Mid" });
                module = CreateModule("ModuleLabels", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("stress preset labels do not match")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenRequiredArchitectureContentIsMissing()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantContent", prefab, 1000);
                SetField(variant, "architectureDescription", string.Empty);
                SetField(variant, "architectureDescriptionRu", string.Empty);
                SetField(variant, "dataFlow", string.Empty);
                SetField(variant, "dataFlowRu", string.Empty);
                module = CreateModule("ModuleContent", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("architecture description is missing English content")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("data flow is missing Russian content")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsWarnings_ForDuplicateAndOrphanVariants()
        {
            VariantDefinitionSO sharedVariant = null;
            VariantDefinitionSO orphanVariant = null;
            ModuleDefinitionSO module = null;
            GameObject sharedPrefab = null;
            GameObject orphanPrefab = null;

            try
            {
                sharedPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("SharedPrefab");
                orphanPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("OrphanPrefab");
                sharedVariant = CreateVariant("SharedVariant", sharedPrefab, 1000, 2000, 3000);
                orphanVariant = CreateVariant("OrphanVariant", orphanPrefab, 1000, 2000, 3000);
                module = CreateModule("ModuleDuplicate", sharedVariant, sharedVariant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { sharedVariant, orphanVariant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("more than once")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("not referenced by any module")));
            }
            finally
            {
                DestroyImmediateSafe(sharedPrefab);
                DestroyImmediateSafe(orphanPrefab);
                DestroyImmediateSafe(sharedVariant);
                DestroyImmediateSafe(orphanVariant);
                DestroyImmediateSafe(module);
            }
        }

        private static ModuleDefinitionSO CreateModule(string assetName, params VariantDefinitionSO[] variants)
        {
            ModuleDefinitionSO module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
            module.name = assetName;
            SetField(module, "moduleName", assetName);
            SetField(module, "moduleNameRu", assetName + " RU");
            SetField(module, "thesis", "Module thesis");
            SetField(module, "thesisRu", "Тезис модуля");
            SetField(module, "description", "Module description");
            SetField(module, "descriptionRu", "Описание модуля");
            SetField(module, "problemStatement", "Module problem");
            SetField(module, "problemStatementRu", "Проблема модуля");
            SetField(module, "activeItemLabel", "Active agents");
            SetField(module, "activeItemLabelRu", "Активные агенты");
            SetField(module, "webGlPresetNote", "WebGL note");
            SetField(module, "webGlPresetNoteRu", "Заметка WebGL");
            SetField(module, "variants", variants);
            return module;
        }

        private static VariantDefinitionSO CreateVariant(string assetName, GameObject prefab, params int[] stressPresets)
        {
            VariantDefinitionSO variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
            variant.name = assetName;
            SetField(variant, "variantName", assetName);
            SetField(variant, "variantNameRu", assetName + " RU");
            SetField(variant, "prefab", prefab);
            SetField(variant, "stressPresets", stressPresets);
            SetField(variant, "architectureDescription", "Architecture body");
            SetField(variant, "architectureDescriptionRu", "Архитектура");
            SetField(variant, "dataFlow", "Input -> Simulation -> View");
            SetField(variant, "dataFlowRu", "Ввод -> Симуляция -> Представление");
            SetField(variant, "runtimeLifecycle", "Bootstrap, simulate, release");
            SetField(variant, "runtimeLifecycleRu", "Запуск, симуляция, освобождение");
            SetField(variant, "whyThisApproach", "Clear ownership");
            SetField(variant, "whyThisApproachRu", "Явное владение");
            SetField(variant, "compareSummary", "Compares well against the naive baseline.");
            SetField(variant, "compareSummaryRu", "Хорошо сравнивается с наивной базой.");
            SetField(variant, "takeaway", "Use when coordination matters.");
            SetField(variant, "takeawayRu", "Использовать, когда важна координация.");
            SetField(variant, "tradeOffs", "More setup, less per-frame duplication.");
            SetField(variant, "tradeOffsRu", "Больше настройки, меньше дублирования в кадре.");
            SetField(variant, "pros", "- Stable");
            SetField(variant, "prosRu", "- Стабильно");
            SetField(variant, "cons", "- More authoring discipline");
            SetField(variant, "consRu", "- Требует дисциплины при наполнении");
            return variant;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName, fieldName);

            field.SetValue(target, value);
        }

        private static void DestroyImmediateSafe(UnityEngine.Object target)
        {
            if (target != null)
                UnityEngine.Object.DestroyImmediate(target);
        }
    }

    internal sealed class ModuleOnlyRuntimeComponent : MonoBehaviour, IModule
    {
        public void Enter()
        {
        }

        public void Exit()
        {
        }
    }
}
