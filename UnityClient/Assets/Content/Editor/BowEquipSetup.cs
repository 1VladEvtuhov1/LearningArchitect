using System.Text;
using LearningArchitect.Modules.Animation3D;
using UnityEditor;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    /// <summary>
    /// Places a bow prop in the left hand of combat actors. Arena presentation is melee-only
    /// and keeps WeaponSocket_L inactive.
    /// </summary>
    public static class BowEquipSetup
    {
        private const string VampireActorPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Prefabs/AnimationActor_VampireCombat.prefab";
        private const string PaladinActorPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Prefabs/AnimationActor_PaladinCombat.prefab";
        private const string BowPrefabPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Prefabs/Animation_BowProp.prefab";
        private const string VampireProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_VampireCombatProfile.asset";
        private const string PaladinProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";

        private const string SocketName = "WeaponSocket_L";
        private const string BowName = "Bow";

        private static readonly Vector3 BowLocalPosition = new Vector3(0.02f, 0f, 0f);
        private static readonly Vector3 BowLocalEuler = new Vector3(0f, 0f, 90f);
        private const float BowScale = 1f;

        [MenuItem("Learning Architect/Animation/Equip Bow Prop")]
        public static void Run()
        {
            var report = new StringBuilder();
            EnsureBowPrefab(report);
            EquipOnActor(VampireActorPath, new[] { "LeftHandProp", "mixamorig:LeftHand", "LeftHand" }, report);
            EquipOnActor(PaladinActorPath, new[] { "mixamorig:LeftHand", "LeftHand", "LeftHandProp" }, report);

            HumanoidAnimationProfileSO vampireProfile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(VampireProfilePath);
            HumanoidAnimationProfileSO paladinProfile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(PaladinProfilePath);
            if (vampireProfile != null)
                InterviewArenaSceneSetup.BakePlayerVisual(vampireProfile);
            if (paladinProfile != null)
                InterviewArenaSceneSetup.BakeEnemyVisual(paladinProfile);

            AssetDatabase.SaveAssets();
            Debug.Log("[BowEquipSetup]\n" + report);
        }

        private static void EnsureBowPrefab(StringBuilder report)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(BowPrefabPath);
            if (existing != null)
            {
                report.AppendLine("Bow prefab exists: " + BowPrefabPath);
                return;
            }

            var root = new GameObject(BowName);
            try
            {
                AddLimb(root.transform, "Grip", Vector3.zero, Vector3.zero, new Vector3(0.035f, 0.09f, 0.035f));
                AddLimb(
                    root.transform,
                    "UpperLimb",
                    new Vector3(0f, 0.22f, -0.05f),
                    new Vector3(18f, 0f, 0f),
                    new Vector3(0.022f, 0.2f, 0.022f));
                AddLimb(
                    root.transform,
                    "LowerLimb",
                    new Vector3(0f, -0.22f, -0.05f),
                    new Vector3(-18f, 0f, 0f),
                    new Vector3(0.022f, 0.2f, 0.022f));
                AddLimb(
                    root.transform,
                    "String",
                    new Vector3(0f, 0f, -0.12f),
                    Vector3.zero,
                    new Vector3(0.008f, 0.42f, 0.008f));

                PrefabUtility.SaveAsPrefabAsset(root, BowPrefabPath);
                report.AppendLine("Created " + BowPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void AddLimb(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            limb.name = name;
            limb.transform.SetParent(parent, false);
            limb.transform.localPosition = localPosition;
            limb.transform.localRotation = Quaternion.Euler(localEuler);
            limb.transform.localScale = localScale;
            Collider collider = limb.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
        }

        private static void EquipOnActor(string actorPath, string[] handNames, StringBuilder report)
        {
            GameObject actor = AssetDatabase.LoadAssetAtPath<GameObject>(actorPath);
            GameObject bowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BowPrefabPath);
            if (actor == null || bowPrefab == null)
            {
                report.AppendLine("SKIP equip: missing " + actorPath);
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(actorPath);
            try
            {
                Transform hand = FindFirst(root.transform, handNames);
                if (hand == null)
                {
                    report.AppendLine("ERROR: left hand not found on " + actorPath);
                    return;
                }

                Transform socket = hand.Find(SocketName);
                if (socket == null)
                {
                    var socketGo = new GameObject(SocketName);
                    socket = socketGo.transform;
                    socket.SetParent(hand, false);
                }

                socket.localPosition = Vector3.zero;
                socket.localRotation = Quaternion.identity;
                socket.localScale = Vector3.one;
                socket.gameObject.SetActive(false);

                for (int i = socket.childCount - 1; i >= 0; i--)
                {
                    Transform child = socket.GetChild(i);
                    if (child.name == BowName)
                        Object.DestroyImmediate(child.gameObject);
                }

                GameObject bow = (GameObject)PrefabUtility.InstantiatePrefab(bowPrefab, socket);
                bow.name = BowName;
                bow.transform.localPosition = BowLocalPosition;
                bow.transform.localRotation = Quaternion.Euler(BowLocalEuler);
                bow.transform.localScale = Vector3.one * BowScale;

                PrefabUtility.SaveAsPrefabAsset(root, actorPath);
                report.AppendLine($"OK: {BowName} under {hand.name}/{SocketName} on {actorPath}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindFirst(Transform root, string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Transform found = FindChild(root, names[i]);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChild(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
