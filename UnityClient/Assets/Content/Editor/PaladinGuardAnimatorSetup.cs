using System.Linq;
using LearningArchitect.EditorTools;
using LearningArchitect.Modules.InterviewArena;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LearningArchitect.Editor
{
    /// <summary>
    /// Wires Block Loop + directional Hit Front/Back/Left/Right on the shared combat controller.
    /// Does not delete Hit / Hit React.
    /// </summary>
    public static class PaladinGuardAnimatorSetup
    {
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const float ReturnExitTime = 0.82f;

        [MenuItem("Learning Architect/Animation/Setup Paladin Guard And Hit Reacts")]
        public static void Setup()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[PaladinGuard] Controller not found: {ControllerPath}");
                return;
            }

            LongswordAnimationSetup.StripLegacyHitClipEvents();

            AnimationClip blockClip = LongswordAnimationSetup.TryLoadBlockLoopClip();
            AnimationClip front = LongswordAnimationSetup.TryLoadHitClip(HitReactDirection.Front);
            AnimationClip back = LongswordAnimationSetup.TryLoadHitClip(HitReactDirection.Back);
            AnimationClip left = LongswordAnimationSetup.TryLoadHitClip(HitReactDirection.Left);
            AnimationClip right = LongswordAnimationSetup.TryLoadHitClip(HitReactDirection.Right);
            if (blockClip == null || front == null || back == null || left == null || right == null)
            {
                Debug.LogError("[PaladinGuard] Missing block/hit clips. Run Setup Longsword Update Package Rig.");
                return;
            }

            EnsureBool(controller, "IsBlocking");
            EnsureTrigger(controller, "BlockStart");
            EnsureTrigger(controller, "Hit");
            EnsureTrigger(controller, "HitFront");
            EnsureTrigger(controller, "HitBack");
            EnsureTrigger(controller, "HitLeft");
            EnsureTrigger(controller, "HitRight");

            Undo.RecordObject(controller, "Setup Paladin Guard And Hit Reacts");

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState bowLoco = FindState(sm, "Bow Locomotion");
            AnimatorState meleeLoco = FindState(sm, "Melee Locomotion");

            AnimatorState block = GetOrCreateState(sm, "Block Loop", new Vector3(300f, 40f, 0f));
            block.motion = blockClip;
            block.speed = 1f;
            ClearTransitions(block);
            if (meleeLoco != null)
                AddBoolReturn(block, meleeLoco, AnimatorConditionMode.IfNot, "IsBlocking");
            EnsureAnyStateTrigger(sm, block, "BlockStart");
            if (meleeLoco != null)
                EnsureStateBoolTransition(meleeLoco, block, "IsBlocking", true);

            WireHitState(sm, GetOrCreateState(sm, "Hit Front", new Vector3(300f, -280f, 0f)), front, bowLoco, meleeLoco, "HitFront");
            WireHitState(sm, GetOrCreateState(sm, "Hit Back", new Vector3(540f, -280f, 0f)), back, bowLoco, meleeLoco, "HitBack");
            WireHitState(sm, GetOrCreateState(sm, "Hit Left", new Vector3(780f, -280f, 0f)), left, bowLoco, meleeLoco, "HitLeft");
            WireHitState(
                sm,
                GetOrCreateState(sm, "Hit Right", new Vector3(1020f, -280f, 0f), "Hit React"),
                right,
                bowLoco,
                meleeLoco,
                "HitRight",
                "Hit");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[PaladinGuard] Block Loop + Hit Front/Back/Left/Right wired. Hit aliases Hit Right.");
        }

        private static void WireHitState(
            AnimatorStateMachine sm,
            AnimatorState state,
            AnimationClip clip,
            AnimatorState bowLoco,
            AnimatorState meleeLoco,
            params string[] triggers)
        {
            state.motion = clip;
            state.speed = 1f;
            ClearTransitions(state);
            if (bowLoco != null)
                AddReturn(state, bowLoco, AnimatorConditionMode.If, "IsBowStance");
            if (meleeLoco != null)
                AddReturn(state, meleeLoco, AnimatorConditionMode.IfNot, "IsBowStance");
            for (int i = 0; i < triggers.Length; i++)
                EnsureAnyStateTrigger(sm, state, triggers[i]);
        }

        private static void ClearTransitions(AnimatorState state)
        {
            for (int i = state.transitions.Length - 1; i >= 0; i--)
                state.RemoveTransition(state.transitions[i]);
        }

        private static void AddReturn(
            AnimatorState source,
            AnimatorState destination,
            AnimatorConditionMode mode,
            string parameter)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = ReturnExitTime;
            transition.hasFixedDuration = true;
            transition.duration = 0.12f;
            transition.AddCondition(mode, 0f, parameter);
        }

        private static void AddBoolReturn(
            AnimatorState source,
            AnimatorState destination,
            AnimatorConditionMode mode,
            string parameter)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.12f;
            transition.AddCondition(mode, 0f, parameter);
        }

        private static void EnsureStateBoolTransition(
            AnimatorState source,
            AnimatorState destination,
            string parameter,
            bool value)
        {
            foreach (AnimatorStateTransition existing in source.transitions)
            {
                if (existing.destinationState != destination)
                    continue;
                if (existing.conditions.Any(c => c.parameter == parameter))
                    return;
            }

            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.1f;
            transition.AddCondition(
                value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                parameter);
        }

        private static void EnsureAnyStateTrigger(
            AnimatorStateMachine sm,
            AnimatorState destination,
            string triggerName)
        {
            foreach (AnimatorStateTransition existing in sm.anyStateTransitions.ToArray())
            {
                if (existing.destinationState != destination)
                    continue;
                if (!existing.conditions.Any(c => c.parameter == triggerName))
                    continue;

                existing.hasExitTime = false;
                existing.hasFixedDuration = true;
                existing.duration = 0.08f;
                existing.canTransitionToSelf = false;
                return;
            }

            AnimatorStateTransition any = sm.AddAnyStateTransition(destination);
            any.hasExitTime = false;
            any.hasFixedDuration = true;
            any.duration = 0.08f;
            any.canTransitionToSelf = false;
            any.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static AnimatorState GetOrCreateState(
            AnimatorStateMachine sm,
            string name,
            Vector3 position,
            params string[] legacyNames)
        {
            AnimatorState state = FindState(sm, name);
            if (state != null)
                return state;

            for (int i = 0; i < legacyNames.Length; i++)
            {
                state = FindState(sm, legacyNames[i]);
                if (state == null)
                    continue;
                state.name = name;
                return state;
            }

            return sm.AddState(name, position);
        }

        private static AnimatorState FindState(AnimatorStateMachine sm, string name)
        {
            foreach (ChildAnimatorState child in sm.states)
            {
                if (child.state != null && child.state.name == name)
                    return child.state;
            }

            return null;
        }

        private static void EnsureTrigger(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(p => p.name == name))
                return;
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
        }

        private static void EnsureBool(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(p => p.name == name))
                return;
            controller.AddParameter(name, AnimatorControllerParameterType.Bool);
        }
    }
}
