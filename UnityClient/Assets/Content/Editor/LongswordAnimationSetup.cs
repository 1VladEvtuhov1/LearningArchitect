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
    /// LongswordAnimsetPro clips retargeted onto the Paladin humanoid avatar:
    /// Update_pt1 melee locomotion, legacy pt1 turn-in-place, pt2 melee attack.
    /// </summary>
    public static class LongswordAnimationSetup
    {
        private const string LongswordLegacyPt1Path = "Assets/LongswordAnimsetPro/Animations/Longsword_Animset_pt1.fbx";
        private const string LongswordUpdatePt1Path =
            "Assets/LongswordAnimsetPro/Animations/Update/LongswordAnimsetPro_Update_pt1.fbx";
        private const string LongswordPt2Path = "Assets/LongswordAnimsetPro/Animations/Longsword_Animset_pt2.fbx";
        private const string AvatarSourceFbx =
            "Assets/Content/Modules/LayeredCharacterAnimation/Models/ShootingAndModel.fbx";
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const string ProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";

        private const string TurnLeftClipName = "Longs_TurnL_90";
        private const string TurnRightClipName = "Longs_TurnR_90";
        private const string MeleeAttackClipName = "Longs_Attack_R";
        private const string MeleeIdleClipName = "LongsLP_Idle1";
        private const string MeleeWalkForwardClipName = "LongsLP_WalkFwd";
        private const string MeleeWalkBackClipName = "LongsLP_WalkBwd";
        private const string MeleeWalkLeftClipName = "LongsLP_WalkLeft";
        private const string MeleeWalkRightClipName = "LongsLP_WalkRight";

        private static readonly HashSet<string> LoopingTurnClips = new HashSet<string>
        {
            TurnLeftClipName,
            TurnRightClipName,
        };

        private static readonly HashSet<string> LoopingMeleeLocomotionClips = new HashSet<string>
        {
            MeleeIdleClipName,
            MeleeWalkForwardClipName,
            MeleeWalkBackClipName,
            MeleeWalkLeftClipName,
            MeleeWalkRightClipName,
        };

        private static readonly HashSet<string> InPlaceMeleeClips = new HashSet<string>
        {
            MeleeAttackClipName,
        };

        [MenuItem("Learning Architect/Animation/Setup Longsword Turn-In-Place Rig")]
        public static void SetupLongswordTurnInPlaceRig()
        {
            if (!ConfigureLongswordTurnRig(out StringBuilder report))
                return;

            Debug.Log("[LongswordAnimationSetup] " + report);
        }

        [MenuItem("Learning Architect/Animation/Setup Longsword Turn-In-Place (Rig + Controller)")]
        public static void SetupLongswordTurnInPlaceAll()
        {
            ConfigureLongswordTurnRig(out _);
            ApplyLongswordTurnInPlaceToPaladinController();
        }

        [MenuItem("Learning Architect/Animation/Setup Longsword Melee Locomotion Rig")]
        public static void SetupLongswordMeleeLocomotionRig()
        {
            if (!ConfigureLongswordMeleeLocomotionRig(out StringBuilder report))
                return;

            Debug.Log("[LongswordAnimationSetup] " + report);
        }

        [MenuItem("Learning Architect/Animation/Setup Longsword Melee Attack Rig")]
        public static void SetupLongswordMeleeAttackRig()
        {
            if (!ConfigureLongswordAttackRig(out StringBuilder report))
                return;

            Debug.Log("[LongswordAnimationSetup] " + report);
        }

        [MenuItem("Learning Architect/Animation/Apply Longsword Melee Attack To Paladin Controller")]
        public static void ApplyLongswordMeleeAttackToPaladinController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[LongswordAnimationSetup] Controller not found: {ControllerPath}");
                return;
            }

            if (!TryPatchMeleeAttackToController(controller))
            {
                Debug.LogError(
                    "[LongswordAnimationSetup] Missing melee attack clip. "
                    + "Run Setup Longsword Melee Attack Rig first.");
                return;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LongswordAnimationSetup] Melee state wired to {MeleeAttackClipName}.");
        }

        public static bool ConfigureLongswordTurnRig(out StringBuilder report)
        {
            report = new StringBuilder();
            if (!TryConfigureFbxClips(
                    LongswordLegacyPt1Path,
                    LoopingTurnClips,
                    loop: true,
                    inPlace: true,
                    stripEvents: true,
                    out int configured,
                    report))
                return false;

            report.Insert(0, $"Retargeted {LongswordLegacyPt1Path}.\n");
            report.AppendLine($"Loop + in-place configured for {configured} turn clip(s).");
            LogClipStatus(LongswordLegacyPt1Path, TurnLeftClipName, report);
            LogClipStatus(LongswordLegacyPt1Path, TurnRightClipName, report);
            return true;
        }

        public static bool ConfigureLongswordMeleeLocomotionRig(out StringBuilder report)
        {
            report = new StringBuilder();
            if (!TryConfigureFbxClips(
                    LongswordUpdatePt1Path,
                    LoopingMeleeLocomotionClips,
                    loop: true,
                    inPlace: true,
                    stripEvents: false,
                    out int configured,
                    report))
                return false;

            report.Insert(0, $"Retargeted {LongswordUpdatePt1Path}.\n");
            report.AppendLine($"Loop + in-place configured for {configured} melee locomotion clip(s).");
            LogClipStatus(LongswordUpdatePt1Path, MeleeIdleClipName, report);
            LogClipStatus(LongswordUpdatePt1Path, MeleeWalkForwardClipName, report);
            LogClipStatus(LongswordUpdatePt1Path, MeleeWalkBackClipName, report);
            LogClipStatus(LongswordUpdatePt1Path, MeleeWalkLeftClipName, report);
            LogClipStatus(LongswordUpdatePt1Path, MeleeWalkRightClipName, report);
            return true;
        }

        public static bool ConfigureLongswordAttackRig(out StringBuilder report)
        {
            report = new StringBuilder();
            if (!TryConfigureFbxClips(
                    LongswordPt2Path,
                    InPlaceMeleeClips,
                    loop: false,
                    inPlace: true,
                    stripEvents: true,
                    out int configured,
                    report))
                return false;

            report.Insert(0, $"Retargeted {LongswordPt2Path}.\n");
            report.AppendLine($"In-place configured for {configured} melee clip(s).");
            LogClipStatus(LongswordPt2Path, MeleeAttackClipName, report);
            return true;
        }

        private static bool TryConfigureFbxClips(
            string fbxPath,
            HashSet<string> clipNames,
            bool loop,
            bool inPlace,
            bool stripEvents,
            out int configured,
            StringBuilder report)
        {
            configured = 0;
            Avatar source = AssetDatabase.LoadAllAssetsAtPath(AvatarSourceFbx)
                .OfType<Avatar>()
                .FirstOrDefault();

            if (source == null)
            {
                Debug.LogError($"[LongswordAnimationSetup] Paladin avatar not found: {AvatarSourceFbx}");
                return false;
            }

            if (AssetImporter.GetAtPath(fbxPath) is not ModelImporter importer)
            {
                Debug.LogError($"[LongswordAnimationSetup] FBX not found: {fbxPath}");
                return false;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = source;

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            for (int i = 0; i < clips.Length; i++)
            {
                if (!clipNames.Contains(clips[i].name))
                    continue;

                clips[i].loopTime = loop;
                if (inPlace)
                {
                    clips[i].loopPose = loop;
                    clips[i].lockRootRotation = true;
                    clips[i].lockRootHeightY = true;
                    clips[i].lockRootPositionXZ = true;
                }

                if (stripEvents)
                    clips[i].events = System.Array.Empty<AnimationEvent>();

                configured++;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            report.AppendLine($"Avatar source: {source.name}.");
            return true;
        }

        [MenuItem("Learning Architect/Animation/Apply Longsword Turn-In-Place To Paladin Controller")]
        public static void ApplyLongswordTurnInPlaceToPaladinController()
        {
            if (!TryLoadTurnClips(out AnimationClip turnLeft, out AnimationClip turnRight))
            {
                Debug.LogError(
                    "[LongswordAnimationSetup] Missing turn clips. "
                    + "Run Setup Longsword Turn-In-Place Rig first.");
                return;
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[LongswordAnimationSetup] Controller not found: {ControllerPath}");
                return;
            }

            EnsureTurnParameters(controller);
            RebuildTurnInPlaceState(controller, turnLeft, turnRight);
            UpdateProfileTurnClips(turnLeft, turnRight);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log(
                "[LongswordAnimationSetup] Turn In Place wired: "
                + $"{TurnLeftClipName} / {TurnRightClipName} on TurnDirection blend tree.");
        }

        public static bool TryLoadTurnClips(out AnimationClip turnLeft, out AnimationClip turnRight)
        {
            turnLeft = LoadClip(LongswordLegacyPt1Path, TurnLeftClipName);
            turnRight = LoadClip(LongswordLegacyPt1Path, TurnRightClipName);
            return turnLeft != null && turnRight != null;
        }

        public static bool TryLoadMeleeLocomotionClips(
            out AnimationClip idle,
            out AnimationClip walkForward,
            out AnimationClip walkBack,
            out AnimationClip walkLeft,
            out AnimationClip walkRight)
        {
            idle = LoadClip(LongswordUpdatePt1Path, MeleeIdleClipName);
            walkForward = LoadClip(LongswordUpdatePt1Path, MeleeWalkForwardClipName);
            walkBack = LoadClip(LongswordUpdatePt1Path, MeleeWalkBackClipName);
            walkLeft = LoadClip(LongswordUpdatePt1Path, MeleeWalkLeftClipName);
            walkRight = LoadClip(LongswordUpdatePt1Path, MeleeWalkRightClipName);
            return idle != null
                && walkForward != null
                && walkBack != null
                && walkLeft != null
                && walkRight != null;
        }

        public static AnimationClip TryLoadMeleeAttackClip() =>
            LoadClip(LongswordPt2Path, MeleeAttackClipName);

        public static bool TryPatchMeleeAttackToController(AnimatorController controller)
        {
            AnimationClip meleeClip = TryLoadMeleeAttackClip();
            if (controller == null || meleeClip == null)
                return false;

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                AnimatorState state = stateMachine.states[i].state;
                if (state.name != "Melee")
                    continue;

                state.motion = meleeClip;
                return true;
            }

            return false;
        }

        public static bool TryApplyTurnInPlaceToController(AnimatorController controller)
        {
            if (controller == null || !TryLoadTurnClips(out AnimationClip turnLeft, out AnimationClip turnRight))
                return false;

            EnsureTurnParameters(controller);
            RebuildTurnInPlaceState(controller, turnLeft, turnRight);
            UpdateProfileTurnClips(turnLeft, turnRight);
            return true;
        }

        private static void RebuildTurnInPlaceState(
            AnimatorController controller,
            AnimationClip turnLeft,
            AnimationClip turnRight)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            FindLocomotionStates(stateMachine, out AnimatorState bowLocomotion, out AnimatorState meleeLocomotion);

            RemoveStatesNamed(stateMachine, "Turn In Place");
            RemoveAnyStateTransitionsTo(stateMachine, "Turn In Place");

            BlendTree blendTree = new BlendTree
            {
                name = "Turn In Place Blend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "TurnDirection",
                useAutomaticThresholds = false,
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.AddChild(turnLeft, -1f);
            blendTree.AddChild(turnRight, 1f);

            AnimatorState turnState = stateMachine.AddState("Turn In Place", new Vector3(300f, -200f, 0f));
            turnState.motion = blendTree;
            turnState.speed = 1.15f;

            AnimatorStateTransition toTurn = stateMachine.AddAnyStateTransition(turnState);
            toTurn.hasExitTime = false;
            toTurn.duration = 0.12f;
            toTurn.canTransitionToSelf = true;
            toTurn.AddCondition(AnimatorConditionMode.If, 0f, "TurnInPlace");

            AddTurnExitTransitions(turnState, bowLocomotion, meleeLocomotion);
        }

        private static void AddTurnExitTransitions(
            AnimatorState turnState,
            AnimatorState bowLocomotion,
            AnimatorState meleeLocomotion)
        {
            for (int i = turnState.transitions.Length - 1; i >= 0; i--)
                turnState.RemoveTransition(turnState.transitions[i]);

            if (bowLocomotion != null)
            {
                AnimatorStateTransition toBow = turnState.AddTransition(bowLocomotion);
                toBow.hasExitTime = false;
                toBow.duration = 0.15f;
                toBow.AddCondition(AnimatorConditionMode.IfNot, 0f, "TurnInPlace");
                toBow.AddCondition(AnimatorConditionMode.If, 0f, "IsBowStance");
            }

            if (meleeLocomotion != null)
            {
                AnimatorStateTransition toMelee = turnState.AddTransition(meleeLocomotion);
                toMelee.hasExitTime = false;
                toMelee.duration = 0.15f;
                toMelee.AddCondition(AnimatorConditionMode.IfNot, 0f, "TurnInPlace");
                toMelee.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBowStance");
            }
        }

        private static void EnsureTurnParameters(AnimatorController controller)
        {
            EnsureParameter(controller, "TurnInPlace", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "TurnDirection", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "TurnAngle", AnimatorControllerParameterType.Float);
        }

        private static void UpdateProfileTurnClips(AnimationClip turnLeft, AnimationClip turnRight)
        {
            HumanoidAnimationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(ProfilePath);
            if (profile == null)
                return;

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("turnLeftClip").objectReferenceValue = turnLeft;
            serialized.FindProperty("turnRightClip").objectReferenceValue = turnRight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
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

        private static void RemoveStatesNamed(AnimatorStateMachine stateMachine, params string[] names)
        {
            HashSet<string> targets = new HashSet<string>(names);
            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                if (targets.Contains(stateMachine.states[i].state.name))
                    stateMachine.RemoveState(stateMachine.states[i].state);
            }
        }

        private static void RemoveAnyStateTransitionsTo(AnimatorStateMachine stateMachine, string destinationName)
        {
            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition transition = stateMachine.anyStateTransitions[i];
                if (transition.destinationState != null && transition.destinationState.name == destinationName)
                    stateMachine.RemoveAnyStateTransition(transition);
            }
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

        private static AnimationClip LoadClip(string fbxPath, string clipName) =>
            AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == clipName);

        private static void LogClipStatus(string fbxPath, string clipName, StringBuilder report)
        {
            AnimationClip clip = LoadClip(fbxPath, clipName);
            report.AppendLine(clip != null ? "[ok] " + clipName : "[MISSING] " + clipName);
        }
    }
}
