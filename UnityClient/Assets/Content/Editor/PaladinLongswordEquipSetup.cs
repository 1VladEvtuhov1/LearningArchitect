using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    /// <summary>
    /// Ensures Paladin has a WeaponSocket_R under Mixamo RightHand with Longsword nested.
    /// Grip offsets are authored constants (Prefab Mode), not runtime axis guessing.
    /// </summary>
    public static class PaladinLongswordEquipSetup
    {
        private const string PrefabPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Prefabs/AnimationActor_PaladinCombat.prefab";
        private const string SwordPath =
            "Assets/LongswordAnimsetPro/Models/DaggerSword/Longsword.fbx";
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const string WeaponConfigPath =
            "Assets/Content/Modules/InterviewArena/Data/InterviewArena_MeleeWeapon.asset";

        private const string HandBoneName = "mixamorig:RightHand";
        private const string SocketName = "WeaponSocket_R";
        private const string SwordName = "Longsword";

        private const float MeleeAnimSpeed = 1.8f;
        private const float SwordScale = 0.85f;

        // Authored for Mixamo RightHand + LongswordAnimsetPro mesh (blade along +Z).
        // Map mesh +Z → hand +X (fingers), not world-up — otherwise idle/melee retarget
        // drives the tip through the torso/head.
        private static readonly Vector3 SwordLocalPosition = new Vector3(0.05f, 0.0f, 0.02f);
        private static readonly Vector3 SwordLocalEuler = new Vector3(0f, 90f, 0f);

        [MenuItem("Learning Architect/Animation/Equip Longsword + Speed Melee")]
        public static void Run()
        {
            var report = new StringBuilder();
            EnsureSocketAndSword(report);
            SpeedMeleeState(report);
            ReportWeaponConfig(report);
            AssetDatabase.SaveAssets();
            Debug.Log("[PaladinLongswordEquipSetup]\n" + report);
        }

        [MenuItem("Learning Architect/Animation/Fix Paladin Longsword Grip")]
        public static void FixGripOnly()
        {
            var report = new StringBuilder();
            report.AppendLine(
                $"Bake constants pos={SwordLocalPosition} euler={SwordLocalEuler} scale={SwordScale}");
            EnsureSocketAndSword(report);
            AssetDatabase.SaveAssets();
            Debug.Log("[PaladinLongswordEquipSetup] Fix grip\n" + report);
        }

        [MenuItem("Learning Architect/Animation/Verify Paladin Longsword Grip")]
        public static void VerifyGrip()
        {
            var report = new StringBuilder();
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform hand = FindChild(root.transform, HandBoneName);
                Transform sword = FindChild(root.transform, SwordName);
                if (hand == null || sword == null)
                {
                    report.AppendLine("ERROR: hand or sword missing.");
                    Debug.LogWarning("[PaladinLongswordEquipSetup] Verify\n" + report);
                    return;
                }

                report.AppendLine(
                    $"hierarchy={hand.name}/{SocketName}/{SwordName}; " +
                    $"localPos={sword.localPosition}; localEuler={sword.localEulerAngles}");

                var animator = root.GetComponent<Animator>();
                AnimationClip idle = FindClip(animator, "LongsLP_Idle1");
                AnimationClip melee = FindClip(animator, "LS_ComboRight_1");
                if (idle == null && melee == null)
                {
                    report.AppendLine("WARN: idle/melee clips not found on controller; bind-pose only.");
                    AppendPoseReport(report, hand, sword, "bind");
                }
                else
                {
                    AnimationMode.StartAnimationMode();
                    try
                    {
                        if (idle != null)
                        {
                            AnimationMode.BeginSampling();
                            AnimationMode.SampleAnimationClip(root, idle, idle.length * 0.25f);
                            AnimationMode.EndSampling();
                            AppendPoseReport(report, hand, sword, "idle@25%");
                        }

                        if (melee != null)
                        {
                            AnimationMode.BeginSampling();
                            AnimationMode.SampleAnimationClip(root, melee, melee.length * 0.35f);
                            AnimationMode.EndSampling();
                            AppendPoseReport(report, hand, sword, "melee@35%");
                        }
                    }
                    finally
                    {
                        AnimationMode.StopAnimationMode();
                    }
                }

                Debug.Log("[PaladinLongswordEquipSetup] Verify\n" + report);
                System.IO.File.WriteAllText(
                    "Assets/Content/Editor/PaladinLongswordGripVerify.txt",
                    report.ToString());
                AssetDatabase.Refresh();
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AppendPoseReport(
            StringBuilder report,
            Transform hand,
            Transform sword,
            string label)
        {
            MeshFilter filter = sword.GetComponentInChildren<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                report.AppendLine($"{label}: no mesh");
                return;
            }

            Bounds b = filter.sharedMesh.bounds;
            // Blade along +Z in Longsword.fbx.
            Vector3 tip = sword.TransformPoint(b.center + Vector3.forward * b.extents.z);
            Vector3 pommel = sword.TransformPoint(b.center - Vector3.forward * b.extents.z);
            Vector3 blade = (tip - pommel).normalized;
            float alongFingers = Vector3.Dot(blade, hand.right);
            float gripDist = Vector3.Distance(sword.TransformPoint(b.center - Vector3.forward * b.extents.z * 0.6f), hand.position);
            report.AppendLine(
                $"{label}: tipY={tip.y:F2} pommelY={pommel.y:F2} " +
                $"dot(blade,hand.right)={alongFingers:F2} gripDist={gripDist:F3}");
        }

        private static AnimationClip FindClip(Animator animator, string clipName)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return null;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip != null && clip.name == clipName)
                    return clip;
            }

            return null;
        }

        private static void EnsureSocketAndSword(StringBuilder report)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform hand = FindChild(root.transform, HandBoneName);
                if (hand == null)
                {
                    report.AppendLine($"ERROR: {HandBoneName} not found.");
                    return;
                }

                Transform socket = EnsureSocket(hand, report);
                ClearOldSwords(hand, socket);

                GameObject swordAsset = AssetDatabase.LoadAssetAtPath<GameObject>(SwordPath);
                if (swordAsset == null)
                {
                    report.AppendLine("ERROR: Longsword.fbx missing.");
                    return;
                }

                GameObject sword = (GameObject)PrefabUtility.InstantiatePrefab(swordAsset, socket);
                sword.name = SwordName;
                sword.transform.localPosition = SwordLocalPosition;
                sword.transform.localRotation = Quaternion.Euler(SwordLocalEuler);
                sword.transform.localScale = Vector3.one * SwordScale;

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                report.AppendLine(
                    $"OK: {SwordName} under {HandBoneName}/{SocketName}; " +
                    $"localPos={SwordLocalPosition}; localEuler={SwordLocalEuler}; scale={SwordScale}. " +
                    "Tweak further in Prefab Mode if needed, then bake constants here.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform EnsureSocket(Transform hand, StringBuilder report)
        {
            Transform socket = hand.Find(SocketName);
            if (socket == null)
            {
                var go = new GameObject(SocketName);
                socket = go.transform;
                socket.SetParent(hand, false);
                report.AppendLine($"Created {SocketName} under {hand.name}.");
            }

            socket.localPosition = Vector3.zero;
            socket.localRotation = Quaternion.identity;
            socket.localScale = Vector3.one;
            return socket;
        }

        private static void ClearOldSwords(Transform hand, Transform socket)
        {
            for (int i = hand.childCount - 1; i >= 0; i--)
            {
                Transform child = hand.GetChild(i);
                if (child == socket)
                    continue;
                if (IsSwordName(child.name) || child.name == SocketName)
                    Object.DestroyImmediate(child.gameObject);
            }

            for (int i = socket.childCount - 1; i >= 0; i--)
            {
                Transform child = socket.GetChild(i);
                if (IsSwordName(child.name))
                    Object.DestroyImmediate(child.gameObject);
            }
        }

        private static bool IsSwordName(string name) =>
            name == SwordName || name.StartsWith("Longsword");

        private static void SpeedMeleeState(StringBuilder report)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                report.AppendLine("ERROR: controller missing.");
                return;
            }

            int count = 0;
            foreach (AnimatorControllerLayer layer in controller.layers)
                count += SetMeleeSpeed(layer.stateMachine, MeleeAnimSpeed);

            EditorUtility.SetDirty(controller);
            report.AppendLine(count > 0
                ? $"Combo Right / Melee state speed -> {MeleeAnimSpeed} ({count} state(s))"
                : "WARN: Melee state not found.");
        }

        private static int SetMeleeSpeed(AnimatorStateMachine machine, float speed)
        {
            int count = 0;
            foreach (ChildAnimatorState child in machine.states)
            {
                if (child.state != null &&
                    (child.state.name == "Melee" || child.state.name.StartsWith("Combo Right")))
                {
                    child.state.speed = speed;
                    count++;
                }
            }

            foreach (ChildAnimatorStateMachine sub in machine.stateMachines)
                count += SetMeleeSpeed(sub.stateMachine, speed);

            return count;
        }

        private static void ReportWeaponConfig(StringBuilder report)
        {
            Object weapon = AssetDatabase.LoadAssetAtPath<Object>(WeaponConfigPath);
            if (weapon == null)
            {
                report.AppendLine("ERROR: MeleeWeapon config missing.");
                return;
            }

            var so = new SerializedObject(weapon);
            SerializedProperty duration = so.FindProperty("strikeDuration");
            SerializedProperty moveLock = so.FindProperty("movementLockDuration");
            report.AppendLine(
                $"strikeDuration={duration.floatValue}; movementLock={moveLock.floatValue} (unchanged).");
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t;
            }

            return null;
        }
    }
}
