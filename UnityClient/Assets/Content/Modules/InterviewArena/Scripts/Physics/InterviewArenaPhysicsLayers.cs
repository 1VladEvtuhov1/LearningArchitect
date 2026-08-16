using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class InterviewArenaPhysicsLayers
    {
        public const string Ground = "InterviewArena_Ground";
        public const string Player = "InterviewArena_Player";
        public const string Enemy = "InterviewArena_Enemy";
        public const string Projectile = "InterviewArena_Projectile";

        public static int GroundLayer => LayerMask.NameToLayer(Ground);
        public static int PlayerLayer => LayerMask.NameToLayer(Player);
        public static int EnemyLayer => LayerMask.NameToLayer(Enemy);

        public static LayerMask GroundMask => LayerMask.GetMask(Ground);
        public static LayerMask PlayerMask => LayerMask.GetMask(Player);
        public static LayerMask EnemyMask => LayerMask.GetMask(Enemy);
        public static LayerMask CharacterMask => LayerMask.GetMask(Player, Enemy);
        public static LayerMask PlayerTargetMask => LayerMask.GetMask(Player);
        public static LayerMask EnemyTargetMask => LayerMask.GetMask(Enemy);

        public static bool LayersConfigured =>
            GroundLayer >= 0 && PlayerLayer >= 0 && EnemyLayer >= 0;
    }
}
