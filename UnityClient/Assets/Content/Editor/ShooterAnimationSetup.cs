using System.Collections.Generic;
using System.Text;
using System.Linq;
using LearningArchitect.Modules.Animation3D;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    /// <summary>
    /// One-time authoring helpers for the Mixamo Shooter Pack clips: configures each FBX as a
    /// Humanoid clip that copies the Paladin avatar so it retargets onto the shared actor.
    /// </summary>
    public static class ShooterAnimationSetup
    {
        private const string ModelsDir = "Assets/Content/Modules/LayeredCharacterAnimation/Models/";
        private const string AvatarSourceFbx = ModelsDir + "ShootingAndModel.fbx";
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const string ProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";
        private const string UpperBodyMaskPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/AvatarMasks/Animation_PaladinUpperBody.mask";

        private static readonly string[] ClipFiles =
        {
            "firing rifle",
            "jump backward",
            "jump forward",
            "rifle aiming idle",
            "rifle run",
            "run backwards",
            "start walking backwards",
            "start walking",
            "stop walking",
            "strafe (2)",
            "strafe",
            "walk backwards stop",
            "walking backwards",
            "walking to dying",
            "walking",
        };

        private static readonly HashSet<string> LoopingClips = new HashSet<string>
        {
            "rifle aiming idle",
            "rifle run",
            "walking",
            "walking backwards",
            "run backwards",
            "strafe",
            "strafe (2)",
            "firing rifle",
        };

        [MenuItem("Learning Architect/Animation/Setup Shooter Rig")]
        public static void SetupShooterRig()
        {
            Avatar source = AssetDatabase.LoadAllAssetsAtPath(AvatarSourceFbx)
                .OfType<Avatar>()
                .FirstOrDefault();

            if (source == null)
            {
                Debug.LogError($"[ShooterAnimationSetup] Paladin source avatar not found in {AvatarSourceFbx}.");
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
            Debug.Log("[ShooterAnimationSetup] " + report);
        }

        [MenuItem("Learning Architect/Animation/Setup Shooter Combat Controller")]
        public static void SetupShooterCombatController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[ShooterAnimationSetup] Controller not found: {ControllerPath}");
                return;
            }

            AnimationClip idleClip = LoadClip("rifle aiming idle");
            AnimationClip runForwardClip = LoadClip("rifle run");
            AnimationClip runBackClip = LoadClip("run backwards");
            AnimationClip strafeLeftClip = LoadClip("strafe");
            AnimationClip strafeRightClip = LoadClip("strafe (2)");
            AnimationClip fireClip = LoadClip("firing rifle");
            AnimationClip jumpForwardClip = LoadClip("jump forward");
            AnimationClip jumpBackwardClip = LoadClip("jump backward");

            if (idleClip == null || runForwardClip == null || runBackClip == null ||
                strafeLeftClip == null || strafeRightClip == null || fireClip == null ||
                jumpForwardClip == null || jumpBackwardClip == null)
            {
                Debug.LogError("[ShooterAnimationSetup] Missing one or more shooter clips. Run Setup Shooter Rig first.");
                return;
            }

            EnsureParameter(controller, "MoveX", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "MoveY", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Turn", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsShooting", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Jump", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "JumpBackward", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Dash", AnimatorControllerParameterType.Trigger);

            RebuildBaseLocomotionLayer(controller, idleClip, runForwardClip, runBackClip, strafeLeftClip, strafeRightClip);
            RebuildActionStates(controller, jumpForwardClip, jumpBackwardClip, runForwardClip);
            RebuildUpperBodyLayer(controller, fireClip);
            UpdateProfile(idleClip, runForwardClip, jumpForwardClip, jumpBackwardClip);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ShooterAnimationSetup] Combat controller rebuilt: directional locomotion + jump/dash + firing rifle upper body.");
        }

        private static void RebuildActionStates(
            AnimatorController controller,
            AnimationClip jumpForward,
            AnimationClip jumpBackward,
            AnimationClip dashClip)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            AnimatorState locomotionState = stateMachine.defaultState;
            if (locomotionState == null)
                return;

            RemoveStatesNamed(stateMachine, "Jump Forward", "Jump Backward", "Dash");

            AnimatorState jumpForwardState = stateMachine.AddState("Jump Forward", new Vector3(540f, -80f, 0f));
            jumpForwardState.motion = jumpForward;

            AnimatorState jumpBackwardState = stateMachine.AddState("Jump Backward", new Vector3(540f, 80f, 0f));
            jumpBackwardState.motion = jumpBackward;

            AnimatorState dashState = stateMachine.AddState("Dash", new Vector3(540f, 200f, 0f));
            dashState.motion = dashClip;
            dashState.speed = 1.35f;

            AddAnyStateTrigger(stateMachine, jumpForwardState, "Jump", requireBackward: false);
            AddAnyStateTrigger(stateMachine, jumpBackwardState, "Jump", requireBackward: true);
            AddAnyStateTrigger(stateMachine, dashState, "Dash", requireBackward: null);

            AddReturnTransition(jumpForwardState, locomotionState, 0.9f);
            AddReturnTransition(jumpBackwardState, locomotionState, 0.9f);
            AddReturnTransition(dashState, locomotionState, 0.72f);
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

        private static void RebuildBaseLocomotionLayer(
            AnimatorController controller,
            AnimationClip idle,
            AnimationClip runForward,
            AnimationClip runBack,
            AnimationClip strafeLeft,
            AnimationClip strafeRight)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;

            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
                stateMachine.RemoveState(stateMachine.states[i].state);

            BlendTree blendTree = new BlendTree
            {
                name = "Directional Locomotion",
                blendType = BlendTreeType.FreeformDirectional2D,
                blendParameter = "MoveX",
                blendParameterY = "MoveY",
                useAutomaticThresholds = false,
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);

            blendTree.AddChild(idle, new Vector2(0f, 0f));
            blendTree.AddChild(runForward, new Vector2(0f, 1f));
            blendTree.AddChild(runBack, new Vector2(0f, -1f));
            blendTree.AddChild(strafeLeft, new Vector2(-1f, 0f));
            blendTree.AddChild(strafeRight, new Vector2(1f, 0f));

            AnimatorState locomotionState = stateMachine.AddState("Locomotion", new Vector3(300f, 0f, 0f));
            locomotionState.motion = blendTree;
            stateMachine.defaultState = locomotionState;
        }

        private static void RebuildUpperBodyLayer(AnimatorController controller, AnimationClip fireClip)
        {
            if (controller.layers.Length < 2)
                return;

            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(UpperBodyMaskPath);
            AnimatorControllerLayer upperLayer = controller.layers[1];
            if (mask != null)
                upperLayer.avatarMask = mask;

            AnimatorStateMachine stateMachine = upperLayer.stateMachine;
            AnimatorState defaultState = null;
            AnimatorState shootState = null;

            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                AnimatorState state = stateMachine.states[i].state;
                if (state.motion == null)
                    defaultState = state;
                else if (state.name.Contains("Shoot"))
                    shootState = state;
            }

            if (shootState != null)
                shootState.motion = fireClip;

            if (defaultState == null || shootState == null)
                return;

            for (int i = defaultState.transitions.Length - 1; i >= 0; i--)
                defaultState.RemoveTransition(defaultState.transitions[i]);

            for (int i = shootState.transitions.Length - 1; i >= 0; i--)
                shootState.RemoveTransition(shootState.transitions[i]);

            AnimatorStateTransition toShoot = defaultState.AddTransition(shootState);
            toShoot.hasExitTime = false;
            toShoot.duration = 0.08f;
            toShoot.AddCondition(AnimatorConditionMode.If, 0f, "IsShooting");

            AnimatorStateTransition toDefault = shootState.AddTransition(defaultState);
            toDefault.hasExitTime = false;
            toDefault.duration = 0.1f;
            toDefault.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsShooting");

            stateMachine.defaultState = defaultState;
        }

        private static void UpdateProfile(
            AnimationClip idleClip,
            AnimationClip runForwardClip,
            AnimationClip jumpForwardClip,
            AnimationClip jumpBackwardClip)
        {
            HumanoidAnimationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(ProfilePath);
            if (profile == null)
                return;

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("idleClip").objectReferenceValue = idleClip;
            serialized.FindProperty("locomotionClip").objectReferenceValue = runForwardClip;
            serialized.FindProperty("jumpClip").objectReferenceValue = jumpForwardClip;
            serialized.FindProperty("jumpBackwardClip").objectReferenceValue = jumpBackwardClip;
            serialized.FindProperty("moveXParameter").stringValue = "MoveX";
            serialized.FindProperty("moveYParameter").stringValue = "MoveY";
            serialized.FindProperty("jumpTriggerParameter").stringValue = "Jump";
            serialized.FindProperty("jumpBackwardParameter").stringValue = "JumpBackward";
            serialized.FindProperty("dashTriggerParameter").stringValue = "Dash";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static AnimationClip LoadClip(string fileName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(ModelsDir + fileName + ".fbx")
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == fileName);
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
    }
}
