using System.Linq;
using LearningArchitect.EditorTools;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LearningArchitect.Editor
{
    /// <summary>
    /// Wires named Combo Right 1/2/3 states to LS_ComboRight_* clips.
    /// Melee / Melee2 triggers stay as aliases so older assets still fire hit 1–2.
    /// </summary>
    public static class PaladinMelee2AnimatorSetup
    {
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const float ComboStateSpeed = 1.8f;
        private const float ReturnExitTime = 0.82f;

        [MenuItem("Learning Architect/Animation/Setup Paladin Melee Combo")]
        [MenuItem("Learning Architect/Animation/Setup Paladin Melee2 State")]
        public static void SetupMelee2()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[PaladinMeleeCombo] Controller not found: {ControllerPath}");
                return;
            }

            AnimationClip clip1 = LongswordAnimationSetup.TryLoadComboRightClip(1);
            AnimationClip clip2 = LongswordAnimationSetup.TryLoadComboRightClip(2);
            AnimationClip clip3 = LongswordAnimationSetup.TryLoadComboRightClip(3);
            if (clip1 == null || clip2 == null || clip3 == null)
            {
                Debug.LogError(
                    "[PaladinMeleeCombo] Missing LS_ComboRight_1/2/3. Run Setup Longsword Update Package Rig.");
                return;
            }

            EnsureTrigger(controller, "Melee");
            EnsureTrigger(controller, "Melee2");
            EnsureTrigger(controller, "ComboRight1");
            EnsureTrigger(controller, "ComboRight2");
            EnsureTrigger(controller, "ComboRight3");

            Undo.RecordObject(controller, "Setup Paladin Melee Combo");

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            FindLocomotion(sm, out AnimatorState bowLoco, out AnimatorState meleeLoco);

            AnimatorState hit1 = GetOrCreateState(sm, "Combo Right 1", new Vector3(300f, -140f, 0f), "Melee");
            AnimatorState hit2 = GetOrCreateState(sm, "Combo Right 2", new Vector3(540f, -140f, 0f), "Melee2");
            AnimatorState hit3 = GetOrCreateState(sm, "Combo Right 3", new Vector3(780f, -140f, 0f));

            WireComboState(sm, hit1, clip1, bowLoco, meleeLoco, "ComboRight1", "Melee");
            WireComboState(sm, hit2, clip2, bowLoco, meleeLoco, "ComboRight2", "Melee2");
            WireComboState(sm, hit3, clip3, bowLoco, meleeLoco, "ComboRight3");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "[PaladinMeleeCombo] Combo Right 1/2/3 = LS_ComboRight_1/2/3. "
                + "Triggers ComboRight* (Melee/Melee2 aliases for hits 1–2).");
        }

        private static void WireComboState(
            AnimatorStateMachine sm,
            AnimatorState state,
            AnimationClip clip,
            AnimatorState bowLoco,
            AnimatorState meleeLoco,
            params string[] triggers)
        {
            state.motion = clip;
            state.speed = ComboStateSpeed;

            for (int i = state.transitions.Length - 1; i >= 0; i--)
                state.RemoveTransition(state.transitions[i]);

            if (bowLoco != null)
                AddReturn(state, bowLoco, AnimatorConditionMode.If, "IsBowStance");
            if (meleeLoco != null)
                AddReturn(state, meleeLoco, AnimatorConditionMode.IfNot, "IsBowStance");

            for (int t = 0; t < triggers.Length; t++)
                EnsureAnyStateTrigger(sm, state, triggers[t]);
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

        private static void EnsureAnyStateTrigger(
            AnimatorStateMachine sm,
            AnimatorState destination,
            string triggerName)
        {
            foreach (AnimatorStateTransition existing in sm.anyStateTransitions.ToArray())
            {
                if (existing.destinationState != destination)
                    continue;

                bool sameTrigger = existing.conditions.Any(c => c.parameter == triggerName);
                if (sameTrigger)
                {
                    existing.hasExitTime = false;
                    existing.hasFixedDuration = true;
                    existing.duration = 0.08f;
                    existing.canTransitionToSelf = false;
                    return;
                }
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

        private static void FindLocomotion(
            AnimatorStateMachine sm,
            out AnimatorState bowLoco,
            out AnimatorState meleeLoco)
        {
            bowLoco = FindState(sm, "Bow Locomotion");
            meleeLoco = FindState(sm, "Melee Locomotion");
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
    }
}
