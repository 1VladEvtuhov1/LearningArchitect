using LearningArchitect.Modules.Animation3D;
using UnityEditor;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    /// <summary>
    /// Player visual: Vampire Knight (authored sword grip). Paladin stays on the enemy profile.
    /// </summary>
    public static class VampireCombatActorSetup
    {
        private const string VampirePrefabPath = "Assets/LongswordAnimsetPro/Prefabs/Vampire.prefab";
        private const string VampireFbxPath = "Assets/LongswordAnimsetPro/Models/Vampire/Vampire.fbx";
        private const string ActorPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Prefabs/AnimationActor_VampireCombat.prefab";
        private const string PaladinProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset";
        private const string VampireProfilePath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Data/Animation_VampireCombatProfile.asset";
        private const string ControllerPath =
            "Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller";
        private const float TargetHeight = 1.8f;

        [MenuItem("Learning Architect/Animation/Setup Vampire As Player Visual")]
        public static void Setup()
        {
            HumanoidAnimationProfileSO paladinProfile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(PaladinProfilePath);
            if (paladinProfile == null)
            {
                Debug.LogError("[VampireCombatActorSetup] Paladin profile missing. Run Setup Paladin Combat Animation first.");
                return;
            }

            Avatar avatar = LoadVampireAvatar();
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (avatar == null || controller == null)
            {
                Debug.LogError("[VampireCombatActorSetup] Vampire avatar or combat controller is missing.");
                return;
            }

            GameObject actor = CreateActorPrefab(avatar, controller);
            HumanoidAnimationProfileSO vampireProfile = CreateVampireProfile(paladinProfile, actor, avatar, controller);
            InterviewArenaSceneSetup.BakePlayerVisual(vampireProfile);
            Debug.Log(
                "[VampireCombatActorSetup] Player uses Vampire combat actor. "
                + "Enemies keep Animation_PaladinCombatProfile.");
        }

        private static Avatar LoadVampireAvatar()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(VampireFbxPath))
            {
                if (asset is Avatar avatar && avatar.isHuman)
                    return avatar;
            }

            return AssetDatabase.LoadAssetAtPath<Avatar>(VampireFbxPath);
        }

        private static GameObject CreateActorPrefab(Avatar avatar, RuntimeAnimatorController controller)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(VampirePrefabPath);
            if (source == null)
                source = AssetDatabase.LoadAssetAtPath<GameObject>(VampireFbxPath);
            if (source == null)
                throw new System.InvalidOperationException("Vampire prefab/FBX was not found.");

            GameObject instance = Object.Instantiate(source);
            instance.name = "AnimationActor_VampireCombat";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            StripVisualPhysics(instance);
            NormalizeHeight(instance);

            Animator animator = instance.GetComponentInChildren<Animator>(true);
            if (animator == null)
                animator = instance.AddComponent<Animator>();

            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            HumanoidCrowdActor crowdActor = instance.GetComponent<HumanoidCrowdActor>();
            if (crowdActor == null)
                crowdActor = instance.AddComponent<HumanoidCrowdActor>();

            SerializedObject actorSo = new SerializedObject(crowdActor);
            actorSo.FindProperty("animator").objectReferenceValue = animator;
            actorSo.FindProperty("visualRoot").objectReferenceValue = instance.transform;
            actorSo.ApplyModifiedPropertiesWithoutUndo();

            HumanoidUpperBodyAimIk aimIk = instance.GetComponent<HumanoidUpperBodyAimIk>();
            if (aimIk == null)
                aimIk = instance.AddComponent<HumanoidUpperBodyAimIk>();
            aimIk.enabled = false;

            PrefabUtility.SaveAsPrefabAsset(instance, ActorPath);
            Object.DestroyImmediate(instance);
            return AssetDatabase.LoadAssetAtPath<GameObject>(ActorPath);
        }

        private static void StripVisualPhysics(GameObject instance)
        {
            foreach (Joint joint in instance.GetComponentsInChildren<Joint>(true))
                Object.DestroyImmediate(joint);

            foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
                Object.DestroyImmediate(body);

            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(collider);

            foreach (Cloth cloth in instance.GetComponentsInChildren<Cloth>(true))
                Object.DestroyImmediate(cloth);
        }

        private static void NormalizeHeight(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            if (bounds.size.y < 0.2f)
                return;

            float scale = TargetHeight / bounds.size.y;
            if (scale > 0.85f && scale < 1.15f)
                return;

            instance.transform.localScale = instance.transform.localScale * scale;
        }

        private static HumanoidAnimationProfileSO CreateVampireProfile(
            HumanoidAnimationProfileSO paladinProfile,
            GameObject actorPrefab,
            Avatar avatar,
            RuntimeAnimatorController controller)
        {
            HumanoidAnimationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(VampireProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<HumanoidAnimationProfileSO>();
                AssetDatabase.CreateAsset(profile, VampireProfilePath);
            }

            EditorUtility.CopySerialized(paladinProfile, profile);
            profile.name = "Animation_VampireCombatProfile";
            SerializedObject profileSo = new SerializedObject(profile);
            profileSo.FindProperty("actorPrefab").objectReferenceValue = actorPrefab;
            profileSo.FindProperty("avatar").objectReferenceValue = avatar;
            profileSo.FindProperty("animatorController").objectReferenceValue = controller;
            profileSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }
    }
}
