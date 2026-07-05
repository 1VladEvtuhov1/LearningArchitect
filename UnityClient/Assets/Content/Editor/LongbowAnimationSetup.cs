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

            RebuildBaseLocomotionLayer(
                controller,
                idleClip,
                walkForwardClip,
                walkBackClip,
                strafeLeftClip,
                strafeRightClip);
            RebuildActionStates(controller, jumpForwardClip, jumpBackwardClip, walkForwardClip);
            RebuildShootRecoilState(controller, recoilClip);
            DisableUpperBodyShootingLayer(controller);
            UpdateProfile(idleClip, walkForwardClip, jumpForwardClip, jumpBackwardClip);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[LongbowAnimationSetup] Combat controller rebuilt with Longbow Aiming Pack "
                + "(directional aim-walk + full-body bow recoil on base layer).");
        }

        private static void RebuildShootRecoilState(AnimatorController controller, AnimationClip recoilClip)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            AnimatorState locomotionState = stateMachine.defaultState;
            if (locomotionState == null || recoilClip == null)
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

            for (int i = recoilState.transitions.Length - 1; i >= 0; i--)
                recoilState.RemoveTransition(recoilState.transitions[i]);

            AnimatorStateTransition exitWhenDone = recoilState.AddTransition(locomotionState);
            exitWhenDone.hasExitTime = true;
            exitWhenDone.exitTime = 0.85f;
            exitWhenDone.duration = 0.08f;
            exitWhenDone.canTransitionToSelf = false;
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
            AnimationClip walkForward,
            AnimationClip walkBack,
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
            blendTree.AddChild(walkForward, new Vector2(0f, 1f));
            blendTree.AddChild(walkBack, new Vector2(0f, -1f));
            blendTree.AddChild(strafeLeft, new Vector2(-1f, 0f));
            blendTree.AddChild(strafeRight, new Vector2(1f, 0f));

            AnimatorState locomotionState = stateMachine.AddState("Locomotion", new Vector3(300f, 0f, 0f));
            locomotionState.motion = blendTree;
            stateMachine.defaultState = locomotionState;
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
}
