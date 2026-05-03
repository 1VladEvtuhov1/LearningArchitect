using LearningArchitect.Core;
using LearningArchitect.Modules.Animation3D;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    public static class PaladinCombatAnimationSetup
    {
        private const string ModuleRootFolder = "Assets/Modules/LayeredCharacterAnimation";
        private const string DataFolder = ModuleRootFolder + "/Data";
        private const string PrefabsFolder = ModuleRootFolder + "/Prefabs";
        private const string MaterialsFolder = ModuleRootFolder + "/Materials";
        private const string AnimationsFolder = ModuleRootFolder + "/Animations";
        private const string AvatarMasksFolder = ModuleRootFolder + "/AvatarMasks";
        private const string ModelsFolder = ModuleRootFolder + "/Models";
        private const string ShootingPath = ModelsFolder + "/ShootingAndModel.fbx";
        private static readonly string[] RunningClipCandidates =
        {
            ModelsFolder + "/Running.fbx",
            ModelsFolder + "/Paladin J Nordstrom@Running.fbx",
            "Assets/Content/Models/Running.fbx",
            "Assets/Content/Models/Paladin J Nordstrom@Running.fbx"
        };

        [MenuItem("Learning Architect/Setup Paladin Combat Animation")]
        public static void Setup()
        {
            string runningPath = ResolveFirstExistingPath(RunningClipCandidates);
            ConfigureHumanoidSourceImporter(ShootingPath, "Shooting");
            Avatar avatar = LoadHumanoidAvatar(ShootingPath);
            ConfigureHumanoidAnimationImporter(runningPath, "Running", avatar);

            AnimationClip runningClip = LoadClip(runningPath, "Running");
            AnimationClip shootingClip = LoadClip(ShootingPath, "Shooting");

            if (runningClip == null)
                throw new System.InvalidOperationException("Running clip was not found.");

            if (shootingClip == null)
                throw new System.InvalidOperationException("Shooting clip was not found.");

            if (avatar == null)
                throw new System.InvalidOperationException("Valid humanoid avatar was not found on shooting model.");

            Material bodyMaterial = CreateMaterial(
                $"{MaterialsFolder}/Animation_PaladinBody.mat",
                "Animation_PaladinBody",
                new Color(0.24f, 0.42f, 0.56f, 1f));

            Material helmetMaterial = CreateMaterial(
                $"{MaterialsFolder}/Animation_PaladinHelmet.mat",
                "Animation_PaladinHelmet",
                new Color(1f, 0.56f, 0.24f, 1f));

            AvatarMask upperBodyMask = CreateUpperBodyMask($"{AvatarMasksFolder}/Animation_PaladinUpperBody.mask");
            AnimatorController controller = CreateCombatController(
                $"{AnimationsFolder}/Animation_PaladinCombat.controller",
                runningClip,
                shootingClip,
                upperBodyMask);

            GameObject actorPrefab = CreateActorPrefab(
                $"{PrefabsFolder}/AnimationActor_PaladinCombat.prefab",
                avatar,
                controller,
                bodyMaterial,
                helmetMaterial);

            HumanoidAnimationProfileSO profile = CreateProfile(
                $"{DataFolder}/Animation_PaladinCombatProfile.asset",
                actorPrefab,
                avatar,
                controller,
                runningClip,
                shootingClip);

            VariantDefinitionSO runVariant = CreateVariant(
                $"{DataFolder}/Animation_RunVariant.asset",
                $"{PrefabsFolder}/AnimationVariant_Run.prefab",
                "AnimationVariant_Run",
                "Run",
                "Бег",
                HumanoidAnimationVariant.PlaybackMode.Run,
                profile,
                new VariantCopy
                {
                    ArchitectureEn = "The same Paladin model plays a humanoid locomotion clip through a reusable AnimatorController. Movement drives Speed and Turn parameters, while the upper-body shooting layer stays disabled.",
                    ArchitectureRu = "Та же модель Paladin воспроизводит humanoid locomotion clip через переиспользуемый AnimatorController. Движение управляет Speed и Turn, а верхний слой стрельбы отключён.",
                    CompareEn = "Clean baseline for checking retargeted locomotion, root locking and readable movement direction.",
                    CompareRu = "Чистый baseline для проверки retargeted locomotion, root locking и читаемого направления движения.",
                    TakeawayEn = "Best when the character only needs full-body movement and simple parameter driving.",
                    TakeawayRu = "Подходит, когда персонажу нужен только full-body movement и простое управление параметрами.",
                    TradeOffsEn = "Readable and cheap, but it does not cover concurrent combat actions.",
                    TradeOffsRu = "Читаемо и дёшево, но не покрывает параллельные боевые действия.",
                    ProsEn = "Simple controller path\nClear locomotion baseline\nGood for validating imported clips",
                    ProsRu = "Простой controller path\nЧистый locomotion baseline\nУдобно для проверки импортированных клипов",
                    ConsEn = "No concurrent action layer\nCombat interrupts movement\nLess representative for action games",
                    ConsRu = "Нет параллельного action layer\nБой прерывает движение\nСлабее отражает action-game механику"
                });

            VariantDefinitionSO shootVariant = CreateVariant(
                $"{DataFolder}/Animation_ShootVariant.asset",
                $"{PrefabsFolder}/AnimationVariant_Shoot.prefab",
                "AnimationVariant_Shoot",
                "Shoot",
                "Стрельба",
                HumanoidAnimationVariant.PlaybackMode.Shoot,
                profile,
                new VariantCopy
                {
                    ArchitectureEn = "The model stays in place and plays the shooting clip as the base full-body state. This isolates aiming and weapon pose readability before mixing is enabled.",
                    ArchitectureRu = "Модель остаётся на месте и проигрывает shooting clip как базовое full-body состояние. Так отдельно проверяется читаемость прицеливания и позы оружия до включения смешивания.",
                    CompareEn = "Useful for checking the authored combat pose, but movement and shooting still compete for the same body.",
                    CompareRu = "Полезно для проверки боевой позы, но движение и стрельба всё ещё конкурируют за одно тело.",
                    TakeawayEn = "Best as an authored combat reference state, not as the final runtime solution.",
                    TakeawayRu = "Хорошо как authored combat reference state, но не как финальное runtime-решение.",
                    TradeOffsEn = "Strong pose clarity, weak locomotion composition.",
                    TradeOffsRu = "Сильная читаемость позы, слабая композиция с locomotion.",
                    ProsEn = "Clear weapon pose\nEasy to debug clip import\nStable stationary preview",
                    ProsRu = "Чёткая поза оружия\nЛегко отлаживать импорт клипа\nСтабильный stationary preview",
                    ConsEn = "Cannot move while firing\nFull-body clip owns the legs\nNot enough for shooter locomotion",
                    ConsRu = "Нельзя двигаться во время стрельбы\nFull-body clip владеет ногами\nНедостаточно для shooter locomotion"
                });

            VariantDefinitionSO runShootVariant = CreateVariant(
                $"{DataFolder}/Animation_RunShootVariant.asset",
                $"{PrefabsFolder}/AnimationVariant_RunShoot.prefab",
                "AnimationVariant_RunShoot",
                "Run + Shoot",
                "Бег + стрельба",
                HumanoidAnimationVariant.PlaybackMode.RunAndShoot,
                profile,
                new VariantCopy
                {
                    ArchitectureEn = "The base layer keeps the running clip on the legs while an AvatarMask routes the shooting clip to body, head, arms and fingers on a second Animator layer.",
                    ArchitectureRu = "Базовый слой оставляет бег на ногах, а AvatarMask отправляет shooting clip на корпус, голову, руки и пальцы во втором Animator layer.",
                    CompareEn = "This is the production-style case: locomotion and combat are composed instead of replacing each other.",
                    CompareRu = "Это production-style случай: locomotion и combat компонуются, а не заменяют друг друга.",
                    TakeawayEn = "Best demonstration of why Animator layers and masks matter for game animation systems.",
                    TakeawayRu = "Лучше всего показывает, зачем игровым animation systems нужны Animator layers и masks.",
                    TradeOffsEn = "More setup and masking discipline, but the mechanic remains responsive while moving.",
                    TradeOffsRu = "Больше setup и дисциплины с масками, зато механика остаётся отзывчивой во время движения.",
                    ProsEn = "Movement and shooting coexist\nUpper-body mask keeps legs running\nCloser to real action-game animation graphs",
                    ProsRu = "Движение и стрельба сосуществуют\nUpper-body mask сохраняет бег ног\nБлиже к реальным action-game animation graphs",
                    ConsEn = "Needs clean humanoid avatar\nMask boundaries must be tuned\nLayered clips can expose pose conflicts",
                    ConsRu = "Нужен корректный humanoid avatar\nГраницы mask нужно настраивать\nLayered clips могут показать конфликты поз"
                });

            UpdateModule(runVariant, shootVariant, runShootVariant);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Paladin combat animation showcase assets configured.");
        }

        private static void ConfigureHumanoidSourceImporter(string path, string clipName)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                throw new System.InvalidOperationException($"Missing ModelImporter: {path}");

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;
            importer.importAnimation = true;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;

            ApplyClipSettings(importer, clipName);
            importer.SaveAndReimport();
        }

        private static void ConfigureHumanoidAnimationImporter(string path, string clipName, Avatar sourceAvatar)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                throw new System.InvalidOperationException($"Missing ModelImporter: {path}");

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = sourceAvatar != null
                ? ModelImporterAvatarSetup.CopyFromOther
                : ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = sourceAvatar;
            importer.importAnimation = true;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;

            ApplyClipSettings(importer, clipName);
            importer.SaveAndReimport();
        }

        private static void ApplyClipSettings(ModelImporter importer, string clipName)
        {
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips != null && clips.Length > 0)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].name = clipName;
                    clips[i].loopTime = true;
                    clips[i].loopPose = true;
                    clips[i].lockRootRotation = true;
                    clips[i].lockRootHeightY = true;
                    clips[i].lockRootPositionXZ = true;
                }

                importer.clipAnimations = clips;
            }
        }

        private static AnimationClip LoadClip(string path, string clipName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && clip.name == clipName)
                    return clip;
            }

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                    return clip;
            }

            return null;
        }

        private static Avatar LoadHumanoidAvatar(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isValid && avatar.isHuman)
                    return avatar;
            }

            return null;
        }

        private static string ResolveFirstExistingPath(string[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                string path = candidates[i];
                if (AssetDatabase.LoadMainAssetAtPath(path) != null || File.Exists(path))
                    return path;
            }

            throw new System.InvalidOperationException(
                "No running animation source was found. Expected one of: " + string.Join(", ", candidates));
        }

        private static Material CreateMaterial(string path, string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = materialName;
            if (shader != null)
                material.shader = shader;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            EditorUtility.SetDirty(material);
            return material;
        }

        private static AvatarMask CreateUpperBodyMask(string path)
        {
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            if (mask == null)
            {
                mask = new AvatarMask();
                AssetDatabase.CreateAsset(mask, path);
            }

            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);

            EditorUtility.SetDirty(mask);
            return mask;
        }

        private static AnimatorController CreateCombatController(string path, AnimationClip runningClip, AnimationClip shootingClip, AvatarMask upperBodyMask)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Turn", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsShooting", AnimatorControllerParameterType.Bool);

            AnimatorControllerLayer[] layers = controller.layers;
            layers[0].name = "Base Locomotion";
            layers[0].defaultWeight = 1f;
            controller.layers = layers;

            AnimatorStateMachine baseMachine = controller.layers[0].stateMachine;
            AnimatorState shootBase = baseMachine.AddState("Shoot Standing");
            shootBase.motion = shootingClip;
            shootBase.speed = 1f;
            shootBase.writeDefaultValues = true;

            AnimatorState runState = baseMachine.AddState("Run");
            runState.motion = runningClip;
            runState.speed = 1f;
            runState.writeDefaultValues = true;
            baseMachine.defaultState = shootBase;

            AnimatorStateTransition toRun = shootBase.AddTransition(runState);
            ConfigureTransition(toRun, 0.1f);
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");

            AnimatorStateTransition toShootBase = runState.AddTransition(shootBase);
            ConfigureTransition(toShootBase, 0.12f);
            toShootBase.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");

            controller.AddLayer("Upper Body Shooting");
            layers = controller.layers;
            AnimatorControllerLayer upperLayer = layers[layers.Length - 1];
            upperLayer.avatarMask = upperBodyMask;
            upperLayer.blendingMode = AnimatorLayerBlendingMode.Override;
            upperLayer.defaultWeight = 1f;
            layers[layers.Length - 1] = upperLayer;
            controller.layers = layers;

            AnimatorStateMachine upperMachine = controller.layers[controller.layers.Length - 1].stateMachine;
            AnimatorState noOverride = upperMachine.AddState("No Upper Override");
            noOverride.writeDefaultValues = true;

            AnimatorState upperShoot = upperMachine.AddState("Upper Body Shoot");
            upperShoot.motion = shootingClip;
            upperShoot.speed = 1f;
            upperShoot.writeDefaultValues = true;
            upperMachine.defaultState = noOverride;

            AnimatorStateTransition toUpperShoot = noOverride.AddTransition(upperShoot);
            ConfigureTransition(toUpperShoot, 0.08f);
            toUpperShoot.AddCondition(AnimatorConditionMode.If, 0f, "IsShooting");

            AnimatorStateTransition toNoOverride = upperShoot.AddTransition(noOverride);
            ConfigureTransition(toNoOverride, 0.1f);
            toNoOverride.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsShooting");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void ConfigureTransition(AnimatorStateTransition transition, float duration)
        {
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
        }

        private static GameObject CreateActorPrefab(
            string path,
            Avatar avatar,
            RuntimeAnimatorController controller,
            Material bodyMaterial,
            Material helmetMaterial)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ShootingPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (instance == null)
                throw new System.InvalidOperationException("Could not instantiate Paladin shooting model.");

            instance.name = "AnimationActor_PaladinCombat";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                if (bounds.size.y > 0.01f)
                    instance.transform.localScale = Vector3.one * (1.25f / bounds.size.y);
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    materials[materialIndex] = renderer.name.ToLowerInvariant().Contains("helmet") ? helmetMaterial : bodyMaterial;
                renderer.sharedMaterials = materials;
            }

            Animator animator = instance.GetComponentInChildren<Animator>(true);
            if (animator == null)
                animator = instance.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            animator.avatar = avatar;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            HumanoidCrowdActor crowdActor = instance.GetComponent<HumanoidCrowdActor>();
            if (crowdActor == null)
                crowdActor = instance.AddComponent<HumanoidCrowdActor>();

            SerializedObject actorSo = new SerializedObject(crowdActor);
            SetObject(actorSo, "animator", animator);
            SetObject(actorSo, "visualRoot", instance.transform);
            actorSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static HumanoidAnimationProfileSO CreateProfile(
            string path,
            GameObject actorPrefab,
            Avatar avatar,
            RuntimeAnimatorController controller,
            AnimationClip runningClip,
            AnimationClip shootingClip)
        {
            HumanoidAnimationProfileSO profile = AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<HumanoidAnimationProfileSO>();
                AssetDatabase.CreateAsset(profile, path);
            }

            SerializedObject profileSo = new SerializedObject(profile);
            SetObject(profileSo, "actorPrefab", actorPrefab);
            SetObject(profileSo, "avatar", avatar);
            SetObject(profileSo, "animatorController", controller);
            SetObject(profileSo, "idleClip", shootingClip);
            SetObject(profileSo, "locomotionClip", runningClip);
            SetObject(profileSo, "turnLeftClip", null);
            SetObject(profileSo, "turnRightClip", null);
            SetObject(profileSo, "jumpClip", null);
            SetString(profileSo, "speedParameter", "Speed");
            SetString(profileSo, "moveXParameter", string.Empty);
            SetString(profileSo, "moveYParameter", "MoveY");
            SetString(profileSo, "turnParameter", "Turn");
            SetString(profileSo, "movingParameter", "IsMoving");
            SetString(profileSo, "groundedParameter", "IsGrounded");
            SetString(profileSo, "shootingParameter", "IsShooting");
            SetFloat(profileSo, "speedNormalization", 1.35f);
            SetFloat(profileSo, "parameterDampTime", 0.08f);
            SetFloat(profileSo, "turnResponsiveness", 10f);
            SetFloat(profileSo, "movingThreshold", 0.08f);
            SetBool(profileSo, "randomizeStartTime", true);
            SetBool(profileSo, "applyRootMotion", false);
            SetEnum(profileSo, "updateMode", (int)AnimatorUpdateMode.Normal);
            SetEnum(profileSo, "cullingMode", (int)AnimatorCullingMode.CullUpdateTransforms);
            profileSo.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static VariantDefinitionSO CreateVariant(
            string assetPath,
            string prefabPath,
            string prefabName,
            string nameEn,
            string nameRu,
            HumanoidAnimationVariant.PlaybackMode mode,
            HumanoidAnimationProfileSO profile,
            VariantCopy copy)
        {
            GameObject root = new GameObject(prefabName);
            root.AddComponent<Animation3DModule>();
            HumanoidAnimationVariant variantComponent = root.AddComponent<HumanoidAnimationVariant>();

            SerializedObject componentSo = new SerializedObject(variantComponent);
            SetInt(componentSo, "count", 1);
            SetInt(componentSo, "visibleCount", 8);
            SetInt(componentSo, "visualLimit", 12);
            SetFloat(componentSo, "radius", 4.8f);
            SetFloat(componentSo, "moveSpeed", mode == HumanoidAnimationVariant.PlaybackMode.Shoot ? 0f : 1.35f);
            SetFloat(componentSo, "cycleSpeed", 4f);
            SetFloat(componentSo, "spawnPadding", 0.08f);
            SetObject(componentSo, "animationProfile", profile);
            SetInt(componentSo, "playbackMode", (int)mode);
            SetBool(componentSo, "translateActors", false);
            SetVector3(componentSo, "presentationForward", Vector3.forward);
            componentSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            VariantDefinitionSO variant = AssetDatabase.LoadAssetAtPath<VariantDefinitionSO>(assetPath);
            if (variant == null)
            {
                variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
                AssetDatabase.CreateAsset(variant, assetPath);
            }

            SerializedObject variantSo = new SerializedObject(variant);
            SetString(variantSo, "localizationKey", string.Empty);
            SetString(variantSo, "variantName", nameEn);
            SetString(variantSo, "variantNameRu", nameRu);
            SetObject(variantSo, "prefab", AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            SetIntArray(variantSo, "stressPresets", new[] { 1, 4, 8 });
            SetStringArray(variantSo, "stressPresetLabels", new[] { "Solo", "Squad", "Crowd" });
            SetStringArray(variantSo, "stressPresetLabelsRu", new[] { "Соло", "Группа", "Толпа" });
            SetString(variantSo, "architectureDescription", copy.ArchitectureEn);
            SetString(variantSo, "architectureDescriptionRu", copy.ArchitectureRu);
            SetString(variantSo, "compareSummary", copy.CompareEn);
            SetString(variantSo, "compareSummaryRu", copy.CompareRu);
            SetString(variantSo, "takeaway", copy.TakeawayEn);
            SetString(variantSo, "takeawayRu", copy.TakeawayRu);
            SetString(variantSo, "tradeOffs", copy.TradeOffsEn);
            SetString(variantSo, "tradeOffsRu", copy.TradeOffsRu);
            SetString(variantSo, "pros", copy.ProsEn);
            SetString(variantSo, "prosRu", copy.ProsRu);
            SetString(variantSo, "cons", copy.ConsEn);
            SetString(variantSo, "consRu", copy.ConsRu);
            variantSo.ApplyModifiedPropertiesWithoutUndo();
            return variant;
        }

        private static void UpdateModule(params VariantDefinitionSO[] variants)
        {
            ModuleDefinitionSO module = AssetDatabase.LoadAssetAtPath<ModuleDefinitionSO>($"{DataFolder}/Animation3DModule.asset");
            if (module == null)
                throw new System.InvalidOperationException("Animation3DModule asset was not found.");

            SerializedObject moduleSo = new SerializedObject(module);
            SetString(moduleSo, "moduleName", "Layered Character Animation");
            SetString(moduleSo, "moduleNameRu", "Слоистая анимация персонажа");
            SetString(moduleSo, "thesis", "Show how one humanoid model can run, shoot, or combine both through Animator layers and an upper-body AvatarMask.");
            SetString(moduleSo, "thesisRu", "Показать, как одна humanoid-модель может бежать, стрелять или совмещать оба действия через Animator layers и upper-body AvatarMask.");
            SetString(moduleSo, "description", "Interactive character-animation module focused on a common game-dev mechanic: keeping locomotion active while a combat action plays on the upper body.");
            SetString(moduleSo, "descriptionRu", "Интерактивный модуль анимации персонажа про частую game-dev механику: locomotion остаётся активной, пока combat action играет на верхней части тела.");
            SetString(moduleSo, "problemStatement", "Full-body clips are easy to preview, but action games need composition. The interesting architecture is separating locomotion ownership from upper-body actions.");
            SetString(moduleSo, "problemStatementRu", "Full-body клипы легко смотреть отдельно, но action-играм нужна композиция. Важная архитектура — разделить владение locomotion и upper-body actions.");
            SetString(moduleSo, "webGlPresetNote", "Use Solo/Squad/Crowd presets for browser-safe previews. The goal is readability of layer mixing, not pushing thousands of skinned meshes.");
            SetString(moduleSo, "webGlPresetNoteRu", "Используйте пресеты Соло/Группа/Толпа для browser-safe preview. Цель — читаемость layer mixing, а не тысячи skinned meshes.");
            SetString(moduleSo, "activeItemLabel", "animated actors");
            SetString(moduleSo, "activeItemLabelRu", "анимируемых актёров");

            SerializedProperty variantsProperty = moduleSo.FindProperty("variants");
            variantsProperty.arraySize = variants.Length;
            for (int i = 0; i < variants.Length; i++)
                variantsProperty.GetArrayElementAtIndex(i).objectReferenceValue = variants[i];

            moduleSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObject(SerializedObject target, string propertyName, Object value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.objectReferenceValue = value;
        }

        private static void SetString(SerializedObject target, string propertyName, string value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.stringValue = value;
        }

        private static void SetInt(SerializedObject target, string propertyName, int value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.intValue = value;
        }

        private static void SetFloat(SerializedObject target, string propertyName, float value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.floatValue = value;
        }

        private static void SetBool(SerializedObject target, string propertyName, bool value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.boolValue = value;
        }

        private static void SetVector3(SerializedObject target, string propertyName, Vector3 value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.vector3Value = value;
        }

        private static void SetEnum(SerializedObject target, string propertyName, int value)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.enumValueIndex = value;
        }

        private static void SetIntArray(SerializedObject target, string propertyName, int[] values)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).intValue = values[i];
        }

        private static void SetStringArray(SerializedObject target, string propertyName, string[] values)
        {
            SerializedProperty property = RequireProperty(target, propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).stringValue = values[i];
        }

        private static SerializedProperty RequireProperty(SerializedObject target, string propertyName)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            if (property == null)
                throw new System.InvalidOperationException($"Missing serialized property '{propertyName}' on {target.targetObject}.");
            return property;
        }

        private struct VariantCopy
        {
            public string ArchitectureEn;
            public string ArchitectureRu;
            public string CompareEn;
            public string CompareRu;
            public string TakeawayEn;
            public string TakeawayRu;
            public string TradeOffsEn;
            public string TradeOffsRu;
            public string ProsEn;
            public string ProsRu;
            public string ConsEn;
            public string ConsRu;
        }
    }
}
