using System.Collections.Generic;

using System.Linq;

using System.Text;

using LearningArchitect.Modules.Animation3D;
using LearningArchitect.Modules.InterviewArena;

using UnityEditor;

using UnityEditor.Animations;

using UnityEngine;



namespace LearningArchitect.EditorTools

{

    /// <summary>

    /// LongswordAnimsetPro (Vampire skeleton) imported as Humanoid via CopyFromOther from Vampire.fbx.

    /// Clips retarget to Paladin at runtime through Unity humanoid playback.

    /// Update pt1-4 for melee locomotion/actions; legacy pt1 for turn-in-place only.

    /// </summary>

    public static class LongswordAnimationSetup

    {

        private const string LongswordLegacyPt1Path = "Assets/LongswordAnimsetPro/Animations/Longsword_Animset_pt1.fbx";

        private const string LongswordUpdatePt1Path =

            "Assets/LongswordAnimsetPro/Animations/Update/LongswordAnimsetPro_Update_pt1.fbx";

        private const string LongswordUpdatePt2Path =

            "Assets/LongswordAnimsetPro/Animations/Update/LongswordAnimsetPro_Update_pt2.fbx";

        private const string LongswordUpdatePt3Path =

            "Assets/LongswordAnimsetPro/Animations/Update/LongswordAnimsetPro_Update_pt3.fbx";

        private const string LongswordUpdatePt4Path =

            "Assets/LongswordAnimsetPro/Animations/Update/LongswordAnimsetPro_Update_pt4.fbx";

        private const string LongswordAvatarSourceFbx =

            "Assets/LongswordAnimsetPro/Models/Vampire/Vampire.fbx";

        private const string ControllerPath =

            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";

        private const string ProfilePath =

            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";



        private const string TurnLeftClipName = "Longs_TurnL_90";

        private const string TurnRightClipName = "Longs_TurnR_90";



        private const string MeleeIdleClipName = "LongsLP_Idle1";

        private const string MeleeWalkForwardClipName = "LongsLP_WalkFwd";

        private const string MeleeWalkBackClipName = "LongsLP_WalkBwd";

        private const string MeleeWalkLeftClipName = "LongsLP_WalkLeft";

        private const string MeleeWalkRightClipName = "LongsLP_WalkRight";



        private const string MeleeAttackClipName = "LS_ComboRight_1";

        private const string DashClipName = "LS_SprintBash";

        private const string HitReactClipName = "LS_HitLegsRight";
        private const string HitLegsLeftClipName = "LS_HitLegsLeft";
        private const string BlockLoopClipName = "LongsLP_BlockLoop";
        private const string LegacyHitFrontClipName = "Longs_HitFront_p";
        private const string LegacyHitBackClipName = "Longs_Hit_Back";
        private const string LegacyHitLeftClipName = "Longs_HitLeft_p";
        private const string LegacyHitRightClipName = "Longs_HitRight_p";
        private const string LegacyMeleeAttackClipName = "Longs_Attack_R";
        private const string LegacyMeleePt2Path = "Assets/LongswordAnimsetPro/Animations/Longsword_Animset_pt2.fbx";
        private const string LegacyMeleePt3Path = "Assets/LongswordAnimsetPro/Animations/Longsword_Animset_pt3.fbx";



        private static readonly string[] UpdateFbxPaths =

        {

            LongswordUpdatePt1Path,

            LongswordUpdatePt2Path,

            LongswordUpdatePt3Path,

            LongswordUpdatePt4Path,

        };



        private static readonly HashSet<string> LoopingTurnClips = new HashSet<string>

        {

            TurnLeftClipName,

            TurnRightClipName,

        };



        [MenuItem("Learning Architect/Animation/Setup Longsword Update Package Rig")]

        public static void SetupLongswordUpdatePackageRig()

        {

            if (!ConfigureLongswordUpdatePackageRig(out StringBuilder report))

                return;



            Debug.Log("[LongswordAnimationSetup] " + report);

        }



        [MenuItem("Learning Architect/Animation/Apply Longsword Update Actions To Paladin Controller")]

        public static void ApplyLongswordUpdateActionsToPaladinController()

        {

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

            if (controller == null)

            {

                Debug.LogError($"[LongswordAnimationSetup] Controller not found: {ControllerPath}");

                return;

            }



            if (!TryPatchActionClipsToController(controller))

            {

                Debug.LogError(

                    "[LongswordAnimationSetup] Missing Update action clips. "

                    + "Run Setup Longsword Update Package Rig first.");

                return;

            }



            EditorUtility.SetDirty(controller);

            AssetDatabase.SaveAssets();

            Debug.Log(

                "[LongswordAnimationSetup] Actions wired: "

                + $"{MeleeAttackClipName}, {DashClipName}, {HitReactClipName}.");

        }



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



        public static bool ConfigureLongswordUpdatePackageRig(out StringBuilder report)

        {

            report = new StringBuilder();

            int files = 0;



            foreach (string fbxPath in UpdateFbxPaths)

            {

                if (!ConfigureUpdateFbx(fbxPath, report))

                    continue;



                files++;

            }



            if (files == 0)

            {

                Debug.LogError("[LongswordAnimationSetup] No Update FBX files configured.");

                return false;

            }



            report.Insert(0, $"Configured {files}/{UpdateFbxPaths.Length} Update FBX file(s).\n");

            LogClipStatus(LongswordUpdatePt1Path, MeleeIdleClipName, report);

            LogClipStatus(LongswordUpdatePt3Path, MeleeAttackClipName, report);

            LogClipStatus(LongswordUpdatePt4Path, DashClipName, report);

            LogClipStatus(LongswordUpdatePt3Path, HitReactClipName, report);

            return true;

        }



        public static bool ConfigureLongswordTurnRig(out StringBuilder report)

        {

            report = new StringBuilder();

            if (!TryConfigureNamedFbxClips(

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

            TryLoadComboRightClip(1)

            ?? LoadClip(LegacyMeleePt2Path, LegacyMeleeAttackClipName);



        public static AnimationClip TryLoadComboRightClip(int hitIndex)

        {

            string clipName = hitIndex == 1

                ? "LS_ComboRight_1"

                : hitIndex == 2

                    ? "LS_ComboRight_2"

                    : hitIndex == 3

                        ? "LS_ComboRight_3"

                        : null;

            return string.IsNullOrEmpty(clipName) ? null : LoadClip(LongswordUpdatePt3Path, clipName);

        }



        public static AnimationClip TryLoadDashClip() =>

            LoadClip(LongswordUpdatePt4Path, DashClipName);



        public static AnimationClip TryLoadHitReactClip() =>
            LoadClip(LongswordUpdatePt3Path, HitReactClipName);

        public static AnimationClip TryLoadBlockLoopClip() =>
            LoadClip(LongswordUpdatePt2Path, BlockLoopClipName);

        public static AnimationClip TryLoadHitClip(HitReactDirection direction) =>
            direction switch
            {
                HitReactDirection.Front =>
                    LoadClip(LegacyMeleePt3Path, LegacyHitFrontClipName)
                    ?? TryLoadHitReactClip(),
                HitReactDirection.Back =>
                    LoadClip(LegacyMeleePt3Path, LegacyHitBackClipName)
                    ?? LoadClip(LongswordUpdatePt3Path, HitLegsLeftClipName),
                HitReactDirection.Left =>
                    LoadClip(LongswordUpdatePt3Path, HitLegsLeftClipName)
                    ?? LoadClip(LegacyMeleePt3Path, LegacyHitLeftClipName),
                HitReactDirection.Right =>
                    TryLoadHitReactClip()
                    ?? LoadClip(LegacyMeleePt3Path, LegacyHitRightClipName),
                _ => TryLoadHitReactClip()
            };

        /// <summary>
        /// Legacy pt3 hit clips ship demo SendEvent messages. Arena HumanoidVisual has no receiver.
        /// </summary>
        public static void StripLegacyHitClipEvents()
        {
            StripNamedClipEvents(
                LegacyMeleePt3Path,
                LegacyHitFrontClipName,
                LegacyHitBackClipName);
        }

        private static void StripNamedClipEvents(string fbxPath, params string[] clipNames)
        {
            if (AssetImporter.GetAtPath(fbxPath) is not ModelImporter importer)
            {
                Debug.LogError($"[LongswordAnimationSetup] FBX not found: {fbxPath}");
                return;
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            var names = new HashSet<string>(clipNames);
            bool dirty = false;
            for (int i = 0; i < clips.Length; i++)
            {
                if (!names.Contains(clips[i].name))
                    continue;
                if (clips[i].events == null || clips[i].events.Length == 0)
                    continue;

                clips[i].events = System.Array.Empty<AnimationEvent>();
                dirty = true;
            }

            if (!dirty)
                return;

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }



        public static bool TryPatchActionClipsToController(AnimatorController controller)

        {

            AnimationClip meleeClip = TryLoadMeleeAttackClip();

            AnimationClip dashClip = TryLoadDashClip();

            AnimationClip hitClip = TryLoadHitReactClip();



            if (controller == null || meleeClip == null)

                return false;



            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            bool patched = false;



            for (int i = 0; i < stateMachine.states.Length; i++)

            {

                AnimatorState state = stateMachine.states[i].state;

                switch (state.name)

                {

                    case "Melee":
                    case "Combo Right 1":

                        state.motion = meleeClip;

                        patched = true;

                        break;

                    case "Combo Right 2":
                    {
                        AnimationClip clip2 = TryLoadComboRightClip(2);
                        if (clip2 != null)
                            state.motion = clip2;
                        break;
                    }

                    case "Combo Right 3":
                    {
                        AnimationClip clip3 = TryLoadComboRightClip(3);
                        if (clip3 != null)
                            state.motion = clip3;
                        break;
                    }

                    case "Dash" when dashClip != null:

                        state.motion = dashClip;

                        state.speed = 1.1f;

                        break;

                    case "Block Loop":
                    {
                        AnimationClip blockClip = TryLoadBlockLoopClip();
                        if (blockClip != null)
                            state.motion = blockClip;
                        break;
                    }

                    case "Hit Front":
                    {
                        AnimationClip front = TryLoadHitClip(HitReactDirection.Front);
                        if (front != null)
                            state.motion = front;
                        break;
                    }

                    case "Hit Back":
                    {
                        AnimationClip back = TryLoadHitClip(HitReactDirection.Back);
                        if (back != null)
                            state.motion = back;
                        break;
                    }

                    case "Hit Left":
                    {
                        AnimationClip left = TryLoadHitClip(HitReactDirection.Left);
                        if (left != null)
                            state.motion = left;
                        break;
                    }

                    case "Hit Right":
                    case "Hit React" when hitClip != null:
                    {
                        AnimationClip right = TryLoadHitClip(HitReactDirection.Right) ?? hitClip;
                        if (right != null)
                            state.motion = right;
                        break;
                    }

                }

            }



            return patched;

        }



        public static bool TryPatchMeleeAttackToController(AnimatorController controller) =>

            TryPatchActionClipsToController(controller);



        public static bool TryApplyTurnInPlaceToController(AnimatorController controller)

        {

            if (controller == null || !TryLoadTurnClips(out AnimationClip turnLeft, out AnimationClip turnRight))

                return false;



            EnsureTurnParameters(controller);

            RebuildTurnInPlaceState(controller, turnLeft, turnRight);

            UpdateProfileTurnClips(turnLeft, turnRight);

            return true;

        }



        private static bool ApplyLongswordHumanoidImportSettings(ModelImporter importer, bool copyFromVampire)

        {

            importer.animationType = ModelImporterAnimationType.Human;



            if (copyFromVampire)

            {

                Avatar source = AssetDatabase.LoadAllAssetsAtPath(LongswordAvatarSourceFbx)

                    .OfType<Avatar>()

                    .FirstOrDefault();



                if (source == null)

                {

                    Debug.LogError(

                        $"[LongswordAnimationSetup] Vampire avatar not found: {LongswordAvatarSourceFbx}");

                    return false;

                }



                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;

                importer.sourceAvatar = source;

                return true;

            }



            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            importer.sourceAvatar = null;

            return true;

        }



        private static bool ConfigureUpdateFbx(string fbxPath, StringBuilder report)

        {

            if (AssetImporter.GetAtPath(fbxPath) is not ModelImporter importer)

            {

                report.AppendLine("[MISSING] " + fbxPath);

                return false;

            }



            if (!ApplyLongswordHumanoidImportSettings(importer, copyFromVampire: true))

                return false;



            ModelImporterClipAnimation[] clips = importer.clipAnimations;

            if (clips == null || clips.Length == 0)

                clips = importer.defaultClipAnimations;



            int configured = 0;

            for (int i = 0; i < clips.Length; i++)

            {

                if (clips[i].name == "tpose")

                    continue;



                ApplyUpdateClipImportRules(clips[i]);

                configured++;

            }



            importer.clipAnimations = clips;

            importer.SaveAndReimport();

            report.AppendLine($"[ok] {fbxPath} ({configured} clip(s), avatar=Vampire)");

            return true;

        }



        private static void ApplyUpdateClipImportRules(ModelImporterClipAnimation clip)

        {

            string name = clip.name;

            bool loop = name.StartsWith("LongsLP_") && !name.Contains("Block_");

            if (name == "LongsLP_BlockLoop")

                loop = true;



            clip.loopTime = loop;

            clip.loopPose = loop;

            clip.lockRootRotation = true;

            clip.lockRootHeightY = true;

            clip.lockRootPositionXZ = true;

            clip.events = System.Array.Empty<AnimationEvent>();

        }



        private static bool TryConfigureNamedFbxClips(

            string fbxPath,

            HashSet<string> clipNames,

            bool loop,

            bool inPlace,

            bool stripEvents,

            out int configured,

            StringBuilder report)

        {

            configured = 0;

            if (AssetImporter.GetAtPath(fbxPath) is not ModelImporter importer)

            {

                Debug.LogError($"[LongswordAnimationSetup] FBX not found: {fbxPath}");

                return false;

            }



            if (!ApplyLongswordHumanoidImportSettings(importer, copyFromVampire: false))

                return false;



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

            report.AppendLine("Humanoid import: CreateFromThisModel (legacy animation skeleton).");

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



            AddLocomotionEntryTransitions(bowLocomotion, meleeLocomotion, turnState);

            AddTurnExitTransitions(turnState, bowLocomotion, meleeLocomotion);

        }



        private static void AddLocomotionEntryTransitions(

            AnimatorState bowLocomotion,

            AnimatorState meleeLocomotion,

            AnimatorState turnState)

        {

            if (bowLocomotion != null)

            {

                RemoveTransitionsTo(bowLocomotion, turnState);

                AnimatorStateTransition toTurn = bowLocomotion.AddTransition(turnState);

                toTurn.hasExitTime = false;

                toTurn.duration = 0.12f;

                toTurn.canTransitionToSelf = false;

                toTurn.AddCondition(AnimatorConditionMode.If, 0f, "TurnInPlace");

            }



            if (meleeLocomotion != null)

            {

                RemoveTransitionsTo(meleeLocomotion, turnState);

                AnimatorStateTransition toTurn = meleeLocomotion.AddTransition(turnState);

                toTurn.hasExitTime = false;

                toTurn.duration = 0.12f;

                toTurn.canTransitionToSelf = false;

                toTurn.AddCondition(AnimatorConditionMode.If, 0f, "TurnInPlace");

            }

        }



        private static void RemoveTransitionsTo(AnimatorState source, AnimatorState destination)

        {

            for (int i = source.transitions.Length - 1; i >= 0; i--)

            {

                if (source.transitions[i].destinationState == destination)

                    source.RemoveTransition(source.transitions[i]);

            }

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

                toBow.duration = 0.2f;

                toBow.canTransitionToSelf = false;

                toBow.AddCondition(AnimatorConditionMode.IfNot, 0f, "TurnInPlace");

                toBow.AddCondition(AnimatorConditionMode.If, 0f, "IsBowStance");

            }



            if (meleeLocomotion != null)

            {

                AnimatorStateTransition toMelee = turnState.AddTransition(meleeLocomotion);

                toMelee.hasExitTime = false;

                toMelee.duration = 0.2f;

                toMelee.canTransitionToSelf = false;

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


