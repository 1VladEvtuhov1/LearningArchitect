using System.Collections.Generic;
using System.Linq;
using System.Text;
using LearningArchitect.Modules.Animation3D;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    /// <summary>
    /// Mixamo Longbow Aiming Pack clips retargeted onto the Paladin combat actor.
    /// </summary>
    public static class LongbowAnimationSetup
    {
        private const string ModelsDir = "Assets/Content/Prefabs/LongbowAimingPack/";
        private const string LegacyShooterModelsDir = "Assets/Content/Modules/LayeredCharacterAnimation/Models/";
        private const string AvatarSourceFbx = LegacyShooterModelsDir + "ShootingAndModel.fbx";
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const string ProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";

        private static readonly string[] ClipFiles =
        {
            "standingidle 01",
            "standingaimwalk forward",
            "standingaimwalk back",
            "standingaimwalk left",
            "standingaimwalk right",
            "standingdrawarrow",
            "standingaimoverdraw",
            "standingaimrecoil",
        };

        private static readonly HashSet<string> LoopingClips = new HashSet<string>
        {
            "standingidle 01",
            "standingaimwalk forward",
            "standingaimwalk back",
            "standingaimwalk left",
            "standingaimwalk right",
            "standingaimoverdraw",
        };

        [MenuItem("Learning Architect/Animation/Setup Longbow Aiming Pack Rig")]
        public static void SetupLongbowRig()
        {
            Avatar source = AssetDatabase.LoadAllAssetsAtPath(AvatarSourceFbx)
                .OfType<Avatar>()
                .FirstOrDefault();

            if (source == null)
            {
                Debug.LogError($"[LongbowAnimationSetup] Paladin source avatar not found in {AvatarSourceFbx}.");
                return;
            }

            StringBuilder report = new StringBuilder();
            report.AppendLine("Source avatar: " + source.name);
            int configured = 0;

            foreach (string file in ClipFiles)
            {
                string path = ModelsDir + file + ".fbx";
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
                {
                    report.AppendLine("MISSING: " + path);
                    continue;
                }

                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = source;

                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                bool loop = LoopingClips.Contains(file);
                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].name = file;
                    clips[i].loopTime = loop;
                }

                importer.clipAnimations = clips;
                importer.SaveAndReimport();

                report.AppendLine((loop ? "[loop] " : "[once] ") + file + "  clips=" + clips.Length);
                configured++;
            }

            report.AppendLine($"Configured {configured}/{ClipFiles.Length}.");
            Debug.Log("[LongbowAnimationSetup] " + report);
        }

        [MenuItem("Learning Architect/Animation/Setup Longbow Combat Controller")]
        public static void SetupLongbowCombatController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[LongbowAnimationSetup] Controller not found: {ControllerPath}");
                return;
            }

            AnimationClip idleClip = LoadLongbowClip("standingidle 01");
            AnimationClip walkForwardClip = LoadLongbowClip("standingaimwalk forward");
            AnimationClip walkBackClip = LoadLongbowClip("standingaimwalk back");
            AnimationClip strafeLeftClip = LoadLongbowClip("standingaimwalk left");
            AnimationClip strafeRightClip = LoadLongbowClip("standingaimwalk right");
            AnimationClip recoilClip = LoadLongbowClip("standingaimrecoil");
            AnimationClip jumpForwardClip = LoadLegacyClip("jump forward");
            AnimationClip jumpBackwardClip = LoadLegacyClip("jump backward");

            if (idleClip == null || walkForwardClip == null || walkBackClip == null ||
                strafeLeftClip == null || strafeRightClip == null || recoilClip == null ||
                jumpForwardClip == null || jumpBackwardClip == null)
            {
                Debug.LogError(
                    "[LongbowAnimationSetup] Missing clips. Run Setup Longbow Aiming Pack Rig first "
                    + "(and keep jump clips in LayeredCharacterAnimation/Models).");
                return;
            }

            EnsureParameter(controller, "MoveX", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "MoveY", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Turn", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsShooting", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Fire", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Jump", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "JumpBackward", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Dash", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "IsBowStance", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "AimYaw", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "TurnInPlace", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "TurnDirection", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "TurnAngle", AnimatorControllerParameterType.Float);

            LongswordAnimationSetup.ConfigureLongswordTurnRig(out _);
            LongswordAnimationSetup.ConfigureLongswordMeleeLocomotionRig(out _);
            LongswordAnimationSetup.ConfigureLongswordAttackRig(out _);
            AssetDatabase.Refresh();

            if (!LongswordAnimationSetup.TryLoadMeleeLocomotionClips(
                    out AnimationClip meleeIdle,
                    out AnimationClip meleeForward,
                    out AnimationClip meleeBack,
                    out AnimationClip meleeLeft,
                    out AnimationClip meleeRight))
            {
                RpgAnimationSetup.TryLoadMeleeLocomotionClips(
                    out meleeIdle,
                    out meleeForward,
                    out meleeBack,
                    out meleeLeft,
                    out meleeRight);
            }
            else
            {
                Debug.Log("[LongbowAnimationSetup] Melee locomotion: LongswordAnimsetPro_Update_pt1 (LongsLP_*).");
            }

            (AnimatorState bowLocomotion, AnimatorState meleeLocomotion) = RebuildBaseLocomotionLayer(
                controller,
                idleClip,
                walkForwardClip,
                walkBackClip,
                strafeLeftClip,
                strafeRightClip,
                meleeIdle ?? idleClip,
                meleeForward ?? walkForwardClip,
                meleeBack ?? walkBackClip,
                meleeLeft ?? strafeLeftClip,
                meleeRight ?? strafeRightClip);
            AnimationClip dashClip = RpgAnimationSetup.TryLoadSprintClip() ?? walkForwardClip;

            RebuildActionStates(controller, jumpForwardClip, jumpBackwardClip, dashClip, bowLocomotion, meleeLocomotion);
            RebuildShootRecoilState(controller, recoilClip, bowLocomotion, meleeLocomotion);
            DisableUpperBodyShootingLayer(controller);
            EnableBaseLayerIkPass(controller);

            AnimatorState locomotionState = controller.layers[0].stateMachine.defaultState;
            if (locomotionState != null)
                RpgAnimationSetup.PatchActionClips(controller, bowLocomotion, meleeLocomotion, walkForwardClip);

            UpdateProfile(idleClip, walkForwardClip, jumpForwardClip, jumpBackwardClip);

            if (LongswordAnimationSetup.TryApplyTurnInPlaceToController(controller))
                Debug.Log("[LongbowAnimationSetup] Longsword turn-in-place layer applied.");

            if (LongswordAnimationSetup.TryPatchMeleeAttackToController(controller))
                Debug.Log("[LongbowAnimationSetup] Longsword melee attack applied.");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[LongbowAnimationSetup] Combat controller rebuilt with Longbow Aiming Pack "
                + "(directional aim-walk + full-body bow recoil on base layer).");
        }

        private static void RebuildShootRecoilState(
            AnimatorController controller,
            AnimationClip recoilClip,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            if (bowLocomotion == null && meleeLocomotion == null)
                return;

            if (recoilClip == null)
                return;

            RemoveStatesNamed(stateMachine, "Bow Recoil");
            RemoveAnyStateTransitionsUsing(stateMachine, "IsShooting");
            RemoveAnyStateTransitionsUsing(stateMachine, "Fire");

            AnimatorState recoilState = stateMachine.AddState("Bow Recoil", new Vector3(300f, 140f, 0f));
            recoilState.motion = recoilClip;

            AnimatorStateTransition toRecoil = stateMachine.AddAnyStateTransition(recoilState);
            toRecoil.hasExitTime = false;
            toRecoil.duration = 0.05f;
            toRecoil.canTransitionToSelf = false;
            toRecoil.AddCondition(AnimatorConditionMode.If, 0f, "Fire");

            AddStanceReturnTransitions(recoilState, bowLocomotion, meleeLocomotion, 0.85f);
        }

        private static void RemoveAnyStateTransitionsUsing(AnimatorStateMachine stateMachine, string parameterName)
        {
            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition transition = stateMachine.anyStateTransitions[i];
                for (int c = 0; c < transition.conditions.Length; c++)
                {
                    if (transition.conditions[c].parameter == parameterName)
                    {
                        stateMachine.RemoveAnyStateTransition(transition);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Longbow clips are full-body aim poses. Layering recoil on a masked upper body while the
        /// base layer plays the same skeleton breaks retargeting — keep one full-body layer only.
        /// </summary>
        private static void EnableBaseLayerIkPass(AnimatorController controller)
        {
            if (controller.layers.Length == 0)
                return;

            SerializedObject controllerSerialized = new SerializedObject(controller);
            controllerSerialized.Update();
            SerializedProperty layers = controllerSerialized.FindProperty("m_AnimatorLayers");
            SerializedProperty baseLayer = layers.GetArrayElementAtIndex(0);
            baseLayer.FindPropertyRelative("m_IKPass").boolValue = true;
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DisableUpperBodyShootingLayer(AnimatorController controller)
        {
            if (controller.layers.Length < 2)
                return;

            AnimatorControllerLayer upperLayer = controller.layers[0];
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name.Contains("Upper"))
                {
                    upperLayer = controller.layers[i];
                    break;
                }
            }

            upperLayer.defaultWeight = 0f;

            SerializedObject controllerSerialized = new SerializedObject(controller);
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (!controller.layers[i].name.Contains("Upper"))
                    continue;

                controllerSerialized.Update();
                SerializedProperty layers = controllerSerialized.FindProperty("m_AnimatorLayers");
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                layer.FindPropertyRelative("m_DefaultWeight").floatValue = 0f;
                controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
                break;
            }

            AnimatorStateMachine stateMachine = upperLayer.stateMachine;

            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);

            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                AnimatorState state = stateMachine.states[i].state;
                for (int t = state.transitions.Length - 1; t >= 0; t--)
                    state.RemoveTransition(state.transitions[t]);
            }
        }

        private static void RebuildActionStates(
            AnimatorController controller,
            AnimationClip jumpForward,
            AnimationClip jumpBackward,
            AnimationClip dashClip,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            if (bowLocomotion == null && meleeLocomotion == null)
                return;

            RemoveStatesNamed(stateMachine, "Jump Forward", "Jump Backward", "Dash");

            AnimatorState jumpForwardState = stateMachine.AddState("Jump Forward", new Vector3(540f, -80f, 0f));
            jumpForwardState.motion = jumpForward;

            AnimatorState jumpBackwardState = stateMachine.AddState("Jump Backward", new Vector3(540f, 80f, 0f));
            jumpBackwardState.motion = jumpBackward;

            AnimatorState dashState = stateMachine.AddState("Dash", new Vector3(540f, 200f, 0f));
            dashState.motion = dashClip;
            dashState.speed = dashClip != null && dashClip.name.Contains("Sprint") ? 1.15f : 1.35f;

            AddAnyStateTrigger(stateMachine, jumpForwardState, "Jump", requireBackward: false);
            AddAnyStateTrigger(stateMachine, jumpBackwardState, "Jump", requireBackward: true);
            AddAnyStateTrigger(stateMachine, dashState, "Dash", requireBackward: null);

            AddStanceReturnTransitions(jumpForwardState, bowLocomotion, meleeLocomotion, 0.9f);
            AddStanceReturnTransitions(jumpBackwardState, bowLocomotion, meleeLocomotion, 0.9f);
            AddStanceReturnTransitions(dashState, bowLocomotion, meleeLocomotion, 0.72f);
        }

        private static void RemoveStatesNamed(AnimatorStateMachine stateMachine, params string[] names)
        {
            HashSet<string> targets = new HashSet<string>(names);
            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                if (targets.Contains(stateMachine.states[i].state.name))
                    stateMachine.RemoveState(stateMachine.states[i].state);
            }
        }

        private static void AddAnyStateTrigger(
            AnimatorStateMachine stateMachine,
            AnimatorState destination,
            string triggerName,
            bool? requireBackward)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);

            if (requireBackward.HasValue)
            {
                transition.AddCondition(
                    requireBackward.Value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                    0f,
                    "JumpBackward");
            }
        }

        private static void AddReturnTransition(
            AnimatorState source,
            AnimatorState destination,
            float normalizedExitTime)
        {
            for (int i = source.transitions.Length - 1; i >= 0; i--)
                source.RemoveTransition(source.transitions[i]);

            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = Mathf.Clamp01(normalizedExitTime);
            transition.duration = 0.1f;
        }

        private static void AddStanceReturnTransitions(
            AnimatorState source,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion,
            float normalizedExitTime)
        {
            for (int i = source.transitions.Length - 1; i >= 0; i--)
                source.RemoveTransition(source.transitions[i]);

            if (bowLocomotion != null)
            {
                AnimatorStateTransition toBow = source.AddTransition(bowLocomotion);
                toBow.hasExitTime = true;
                toBow.exitTime = Mathf.Clamp01(normalizedExitTime);
                toBow.duration = 0.12f;
                toBow.AddCondition(AnimatorConditionMode.If, 0f, "IsBowStance");
            }

            if (meleeLocomotion != null)
            {
                AnimatorStateTransition toMelee = source.AddTransition(meleeLocomotion);
                toMelee.hasExitTime = true;
                toMelee.exitTime = Mathf.Clamp01(normalizedExitTime);
                toMelee.duration = 0.12f;
                toMelee.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBowStance");
            }
        }

        private static (AnimatorState Bow, AnimatorState Melee) RebuildBaseLocomotionLayer(
            AnimatorController controller,
            AnimationClip bowIdle,
            AnimationClip bowForward,
            AnimationClip bowBack,
            AnimationClip bowLeft,
            AnimationClip bowRight,
            AnimationClip meleeIdle,
            AnimationClip meleeForward,
            AnimationClip meleeBack,
            AnimationClip meleeLeft,
            AnimationClip meleeRight)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;

            RemoveStatesNamed(stateMachine, "Locomotion", "Bow Locomotion", "Melee Locomotion");

            AnimatorState bowLocomotion = CreateDirectionalLocomotionState(
                controller,
                stateMachine,
                "Bow Locomotion",
                new Vector3(300f, 0f, 0f),
                bowIdle,
                bowForward,
                bowBack,
                bowLeft,
                bowRight);

            AnimatorState meleeLocomotion = CreateDirectionalLocomotionState(
                controller,
                stateMachine,
                "Melee Locomotion",
                new Vector3(300f, 120f, 0f),
                meleeIdle,
                meleeForward,
                meleeBack,
                meleeLeft,
                meleeRight);

            AddStanceSwapTransition(bowLocomotion, meleeLocomotion);
            AddStanceSwapTransition(meleeLocomotion, bowLocomotion);

            stateMachine.defaultState = bowLocomotion;
            return (bowLocomotion, meleeLocomotion);
        }

        private static AnimatorState CreateDirectionalLocomotionState(
            AnimatorController controller,
            AnimatorStateMachine stateMachine,
            string stateName,
            Vector3 position,
            AnimationClip idle,
            AnimationClip walkForward,
            AnimationClip walkBack,
            AnimationClip strafeLeft,
            AnimationClip strafeRight)
        {
            BlendTree blendTree = new BlendTree
            {
                name = stateName + " Blend",
                blendType = BlendTreeType.FreeformDirectional2D,
                blendParameter = "MoveX",
                blendParameterY = "MoveY",
                useAutomaticThresholds = false,
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);

            blendTree.AddChild(idle, new Vector2(0f, 0f));
            blendTree.AddChild(walkForward, new Vector2(0f, 1f));
            blendTree.AddChild(walkBack, new Vector2(0f, -1f));
            blendTree.AddChild(strafeLeft, new Vector2(-1f, 0f));
            blendTree.AddChild(strafeRight, new Vector2(1f, 0f));

            AnimatorState locomotionState = stateMachine.AddState(stateName, position);
            locomotionState.motion = blendTree;
            return locomotionState;
        }

        private static void AddStanceSwapTransition(AnimatorState from, AnimatorState to)
        {
            bool enteringBow = to.name.Contains("Bow");
            for (int i = from.transitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition existing = from.transitions[i];
                if (existing.destinationState == to)
                    from.RemoveTransition(existing);
            }

            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.15f;
            transition.AddCondition(
                enteringBow ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                "IsBowStance");
        }

        private static void UpdateProfile(
            AnimationClip idleClip,
            AnimationClip walkForwardClip,
            AnimationClip jumpForwardClip,
            AnimationClip jumpBackwardClip)
        {
            HumanoidAnimationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(ProfilePath);
            if (profile == null)
                return;

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("idleClip").objectReferenceValue = idleClip;
            serialized.FindProperty("locomotionClip").objectReferenceValue = walkForwardClip;
            serialized.FindProperty("jumpClip").objectReferenceValue = jumpForwardClip;
            serialized.FindProperty("jumpBackwardClip").objectReferenceValue = jumpBackwardClip;
            serialized.FindProperty("moveXParameter").stringValue = "MoveX";
            serialized.FindProperty("moveYParameter").stringValue = "MoveY";
            serialized.FindProperty("jumpTriggerParameter").stringValue = "Jump";
            serialized.FindProperty("jumpBackwardParameter").stringValue = "JumpBackward";
            serialized.FindProperty("dashTriggerParameter").stringValue = "Dash";
            serialized.FindProperty("visualYawOffsetDegrees").floatValue = 90f;
            serialized.FindProperty("visualYawOffsetMeleeDegrees").floatValue = 0f;
            serialized.FindProperty("aimStanceParameter").stringValue = "IsBowStance";
            serialized.FindProperty("aimYawParameter").stringValue = "AimYaw";
            serialized.FindProperty("maxUpperBodyAimDegrees").floatValue = 75f;
            serialized.FindProperty("upperBodyAimWeightBlendSpeed").floatValue = 12f;
            serialized.FindProperty("turnInPlaceParameter").stringValue = "TurnInPlace";
            serialized.FindProperty("turnDirectionParameter").stringValue = "TurnDirection";
            serialized.FindProperty("turnAngleParameter").stringValue = "TurnAngle";
            serialized.FindProperty("idleTurnStartAngle").floatValue = 20f;
            serialized.FindProperty("idleTurnStopAngle").floatValue = 8f;
            serialized.FindProperty("turnInPlaceDegreesPerSecond").floatValue = 240f;
            serialized.FindProperty("turnInPlaceAnimatorDampTime").floatValue = 0.08f;
            serialized.FindProperty("turnInPlaceAimIkWeight").floatValue = 0.65f;
            serialized.FindProperty("movingTurnDegreesPerSecond").floatValue = 720f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static AnimationClip LoadLongbowClip(string fileName) =>
            LoadClip(ModelsDir + fileName + ".fbx", fileName);

        private static AnimationClip LoadLegacyClip(string fileName) =>
            LoadClip(LegacyShooterModelsDir + fileName + ".fbx", fileName);

        private static AnimationClip LoadClip(string assetPath, string clipName) =>
            AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == clipName);

        private static void EnsureParameter(
            AnimatorController controller,
            string name,
            AnimatorControllerParameterType type)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == name)
                    return;
            }

            controller.AddParameter(name, type);
        }
    }

    /// <summary>
    /// DoubleL RPG_Animations_Pack_FREE clips for Arena dash, melee, and hit react.
    /// Uses baked Demo/Anim humanoid clips (humanoid retarget at runtime).
    /// </summary>
    public static class RpgAnimationSetup
    {
        private const string RpgRoot = "Assets/DoubleL/FBX Unity/";
        private const string DoubleLAvatarFbx = "Assets/DoubleL/Model/Armature.fbx";
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const string ProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";

        private const string SprintClipPath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Sprint_InPlace.anim";
        private const string MeleeClipPath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Attack_1_InPlace.anim";
        private const string HitClipPath = "Assets/DoubleL/Demo/Anim/Hit_F_1_InPlace.anim";

        private const string SprintClipName = "OneHand_Up_Sprint_InPlace";
        private const string MeleeClipName = "OneHand_Up_Attack_1_InPlace";
        private const string HitClipName = "Hit_F_1_InPlace";

        private const string SprintFbxPath =
            RpgRoot + "One Hand Up/Movement/Sprint/Base/InPlace/1Hand_Up_Sprint_A_F_InPlace.fbx";
        private const string MeleeFbxPath =
            RpgRoot + "One Hand Up/Attack_A/InPlace/1Hand_Up_Attack_A_1_InPlace.fbx";
        private const string HitFbxPath = RpgRoot + "Hit/InPlace/Hit_B_4_InPlace.fbx";

        [MenuItem("Learning Architect/Animation/Setup RPG Action Clips Rig")]
        public static void SetupRpgActionClipsRig()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("Using baked Demo/Anim humanoid clips (no FBX retarget required).");
            LogClipStatus(SprintClipPath, SprintClipName, report);
            LogClipStatus(MeleeClipPath, MeleeClipName, report);
            LogClipStatus(HitClipPath, HitClipName, report);

            Avatar source = AssetDatabase.LoadAllAssetsAtPath(DoubleLAvatarFbx)
                .OfType<Avatar>()
                .FirstOrDefault();

            if (source != null)
            {
                report.AppendLine("Optional FBX reimport (CreateFromThisModel):");
                ConfigureHumanoidClip(SprintFbxPath, "1Hand_Up_Sprint_A_F_InPlace", loop: true, report);
                ConfigureHumanoidClip(MeleeFbxPath, "1Hand_Up_Attack_A_1_InPlace", loop: false, report);
                ConfigureHumanoidClip(HitFbxPath, "Hit_B_4_InPlace", loop: false, report);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
                report.AppendLine("Skip optional FBX reimport: DoubleL avatar missing.");

            Debug.Log("[RpgAnimationSetup] " + report);
        }

        [MenuItem("Learning Architect/Animation/Apply RPG Melee And Dash To Paladin Controller")]
        public static void ApplyRpgMeleeAndDashToPaladinController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[RpgAnimationSetup] Controller not found: {ControllerPath}");
                return;
            }

            AnimationClip sprintClip = LoadClip(SprintClipPath, SprintClipName);
            AnimationClip meleeClip = LongswordAnimationSetup.TryLoadMeleeAttackClip()
                ?? LoadClip(MeleeClipPath, MeleeClipName);
            AnimationClip hitClip = LoadClip(HitClipPath, HitClipName);

            if (sprintClip == null || meleeClip == null)
            {
                Debug.LogError("[RpgAnimationSetup] Missing sprint or melee Demo/Anim clip.");
                return;
            }

            FindLocomotionStates(controller.layers[0].stateMachine, out AnimatorState bowLocomotion, out AnimatorState meleeLocomotion);
            PatchActionClipsInternal(controller, sprintClip, meleeClip, hitClip, bowLocomotion, meleeLocomotion);
            Debug.Log(
                "[RpgAnimationSetup] Paladin controller patched: dash=sprint, melee=OneHand_Up_Attack_1_InPlace"
                + (hitClip != null ? ", hit=Hit_F_1_InPlace." : "."));
        }

        public static AnimationClip TryLoadSprintClip() => LoadClip(SprintClipPath, SprintClipName);

        private const string MeleeIdlePath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Idle.anim";
        private const string MeleeWalkForwardPath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Walk_F_InPlace.anim";
        private const string MeleeWalkBackPath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Walk_B_InPlace.anim";
        private const string MeleeWalkLeftPath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Walk_L_InPlace.anim";
        private const string MeleeWalkRightPath = "Assets/DoubleL/Demo/Anim/OneHand_Up_Walk_R_InPlace.anim";

        public static bool TryLoadMeleeLocomotionClips(
            out AnimationClip idle,
            out AnimationClip walkForward,
            out AnimationClip walkBack,
            out AnimationClip walkLeft,
            out AnimationClip walkRight)
        {
            idle = LoadClip(MeleeIdlePath, "OneHand_Up_Idle");
            walkForward = LoadClip(MeleeWalkForwardPath, "OneHand_Up_Walk_F_InPlace");
            walkBack = LoadClip(MeleeWalkBackPath, "OneHand_Up_Walk_B_InPlace");
            walkLeft = LoadClip(MeleeWalkLeftPath, "OneHand_Up_Walk_L_InPlace");
            walkRight = LoadClip(MeleeWalkRightPath, "OneHand_Up_Walk_R_InPlace");
            return idle != null && walkForward != null && walkBack != null && walkLeft != null && walkRight != null;
        }

        public static void PatchActionClips(
            AnimatorController controller,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion,
            AnimationClip dashFallback)
        {
            AnimationClip sprintClip = TryLoadSprintClip() ?? dashFallback;
            AnimationClip meleeClip = LongswordAnimationSetup.TryLoadMeleeAttackClip()
                ?? LoadClip(MeleeClipPath, MeleeClipName);
            AnimationClip hitClip = LoadClip(HitClipPath, HitClipName);

            if (sprintClip == null)
                sprintClip = dashFallback;

            PatchActionClipsInternal(controller, sprintClip, meleeClip, hitClip, bowLocomotion, meleeLocomotion);
        }

        private static void PatchActionClipsInternal(
            AnimatorController controller,
            AnimationClip sprintClip,
            AnimationClip meleeClip,
            AnimationClip hitClip,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion)
        {
            EnsureParameter(controller, "Melee", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Hit", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            if (bowLocomotion == null && meleeLocomotion == null)
                FindLocomotionStates(stateMachine, out bowLocomotion, out meleeLocomotion);

            ReplaceDashClip(stateMachine, sprintClip);

            if (meleeClip != null)
                RebuildMeleeState(stateMachine, bowLocomotion, meleeLocomotion, meleeClip);

            if (hitClip != null)
                RebuildHitState(stateMachine, bowLocomotion, meleeLocomotion, hitClip);

            UpdateProfileMeleeParameter();

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        private static void FindLocomotionStates(
            AnimatorStateMachine stateMachine,
            out AnimatorState bowLocomotion,
            out AnimatorState meleeLocomotion)
        {
            bowLocomotion = null;
            meleeLocomotion = null;

            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                AnimatorState state = stateMachine.states[i].state;
                if (state.name == "Bow Locomotion")
                    bowLocomotion = state;
                else if (state.name == "Melee Locomotion")
                    meleeLocomotion = state;
            }
        }

        private static void LogClipStatus(string assetPath, string clipName, StringBuilder report)
        {
            AnimationClip clip = LoadClip(assetPath, clipName);
            report.AppendLine(clip != null ? "[ok] " + assetPath : "[MISSING] " + assetPath);
        }

        private static void ConfigureHumanoidClip(string assetPath, string clipName, bool loop, StringBuilder report)
        {
            if (AssetImporter.GetAtPath(assetPath) is not ModelImporter importer)
            {
                report.AppendLine("MISSING: " + assetPath);
                return;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].name = clipName;
                clips[i].loopTime = loop;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            report.AppendLine((loop ? "[loop] " : "[once] ") + clipName);
        }

        private static void ReplaceDashClip(AnimatorStateMachine stateMachine, AnimationClip sprintClip)
        {
            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                AnimatorState state = stateMachine.states[i].state;
                if (state.name != "Dash")
                    continue;

                state.motion = sprintClip;
                state.speed = sprintClip.name.Contains("Sprint") ? 1.15f : 1f;
                return;
            }
        }

        private static void RebuildMeleeState(
            AnimatorStateMachine stateMachine,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion,
            AnimationClip meleeClip)
        {
            RemoveStatesNamed(stateMachine, "Melee");
            RemoveAnyStateTransitionsUsing(stateMachine, "Melee");

            AnimatorState meleeState = stateMachine.AddState("Melee", new Vector3(300f, -140f, 0f));
            meleeState.motion = meleeClip;

            AnimatorStateTransition toMelee = stateMachine.AddAnyStateTransition(meleeState);
            toMelee.hasExitTime = false;
            toMelee.duration = 0.06f;
            toMelee.canTransitionToSelf = false;
            toMelee.AddCondition(AnimatorConditionMode.If, 0f, "Melee");

            AddStanceReturnTransitionsRpg(meleeState, bowLocomotion, meleeLocomotion, 0.82f);
        }

        private static void RebuildHitState(
            AnimatorStateMachine stateMachine,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion,
            AnimationClip hitClip)
        {
            RemoveStatesNamed(stateMachine, "Hit React");
            RemoveAnyStateTransitionsUsing(stateMachine, "Hit");

            AnimatorState hitState = stateMachine.AddState("Hit React", new Vector3(300f, -260f, 0f));
            hitState.motion = hitClip;

            AnimatorStateTransition toHit = stateMachine.AddAnyStateTransition(hitState);
            toHit.hasExitTime = false;
            toHit.duration = 0.05f;
            toHit.canTransitionToSelf = false;
            toHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");

            AddStanceReturnTransitionsRpg(hitState, bowLocomotion, meleeLocomotion, 0.88f);
        }

        private static void AddStanceReturnTransitionsRpg(
            AnimatorState source,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion,
            float normalizedExitTime)
        {
            for (int i = source.transitions.Length - 1; i >= 0; i--)
                source.RemoveTransition(source.transitions[i]);

            if (bowLocomotion != null)
            {
                AnimatorStateTransition toBow = source.AddTransition(bowLocomotion);
                toBow.hasExitTime = true;
                toBow.exitTime = Mathf.Clamp01(normalizedExitTime);
                toBow.duration = 0.12f;
                toBow.AddCondition(AnimatorConditionMode.If, 0f, "IsBowStance");
            }

            if (meleeLocomotion != null)
            {
                AnimatorStateTransition toMelee = source.AddTransition(meleeLocomotion);
                toMelee.hasExitTime = true;
                toMelee.exitTime = Mathf.Clamp01(normalizedExitTime);
                toMelee.duration = 0.12f;
                toMelee.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBowStance");
            }
        }

        private static void UpdateProfileMeleeParameter()
        {
            HumanoidAnimationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(ProfilePath);
            if (profile == null)
                return;

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("meleeTriggerParameter").stringValue = "Melee";
            serialized.FindProperty("hitTriggerParameter").stringValue = "Hit";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static AnimationClip LoadClip(string assetPath, string clipName)
        {
            AnimationClip direct = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (direct != null)
                return direct;

            return AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == clipName);
        }

        private static void EnsureParameter(
            AnimatorController controller,
            string name,
            AnimatorControllerParameterType type)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == name)
                    return;
            }

            controller.AddParameter(name, type);
        }

        private static void RemoveStatesNamed(AnimatorStateMachine stateMachine, params string[] names)
        {
            HashSet<string> targets = new HashSet<string>(names);
            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                if (targets.Contains(stateMachine.states[i].state.name))
                    stateMachine.RemoveState(stateMachine.states[i].state);
            }
        }

        private static void RemoveAnyStateTransitionsUsing(AnimatorStateMachine stateMachine, string parameterName)
        {
            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition transition = stateMachine.anyStateTransitions[i];
                for (int c = 0; c < transition.conditions.Length; c++)
                {
                    if (transition.conditions[c].parameter != parameterName)
                        continue;

                    stateMachine.RemoveAnyStateTransition(transition);
                    break;
                }
            }
        }

        private static void AddReturnTransition(
            AnimatorState source,
            AnimatorState destination,
            float normalizedExitTime)
        {
            for (int i = source.transitions.Length - 1; i >= 0; i--)
                source.RemoveTransition(source.transitions[i]);

            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = Mathf.Clamp01(normalizedExitTime);
            transition.duration = 0.1f;
            transition.canTransitionToSelf = false;
        }
    }
}
