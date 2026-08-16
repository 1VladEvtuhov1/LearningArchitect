using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class InterviewArenaPhysicsMaterials
    {
        public const string LocomotionMaterialPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_Locomotion.physicMaterial";

        public static void ApplyLocomotionMaterial(Collider collider, PhysicsMaterial material)
        {
            if (collider == null || material == null)
                return;

            collider.material = material;
        }

#if UNITY_EDITOR
        public static PhysicsMaterial EnsureLocomotionMaterialAsset()
        {
            var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(LocomotionMaterialPath);
            if (existing != null)
                return existing;

            var material = new PhysicsMaterial("InterviewArena_Locomotion")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            UnityEditor.AssetDatabase.CreateAsset(material, LocomotionMaterialPath);
            UnityEditor.AssetDatabase.SaveAssets();
            return material;
        }
#endif
    }
}
