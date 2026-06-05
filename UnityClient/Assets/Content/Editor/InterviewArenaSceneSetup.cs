using LearningArchitect.Core;
using LearningArchitect.Modules.InterviewArena;
using LearningArchitect.UI;
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningArchitect.EditorTools
{
    public static class InterviewArenaSceneSetup
    {
        private const string HubPrefabPath = "Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab";
        private const string ShowcaseScenePath = ShowcaseSceneNames.ArchitectureShowcasePath;
        private const string ArenaScenePath = ShowcaseSceneNames.InterviewArenaPath;
        private const string PlayerConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_PlayerConfig.asset";
        private const string MeleeConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_MeleeWeapon.asset";
        private const string CrossbowConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_CrossbowWeapon.asset";
        private const string EnemyCrossbowConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_EnemyCrossbowWeapon.asset";
        private const string BoltPrefabPath = "Assets/Content/Modules/InterviewArena/Prefabs/InterviewArena_CrossbowBolt.prefab";
        private const string EnemyConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_EnemyConfig.asset";
        private const string EnemyPrefabPath = "Assets/Content/Modules/InterviewArena/Prefabs/InterviewArena_HollowSoldier.prefab";
        private const string PlayerPrefabPath = "Assets/Content/Modules/InterviewArena/Prefabs/InterviewArenaPlayer.prefab";
        private const string BuffSpeedConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_BuffSpeed.asset";
        private const string BuffMeleeConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_BuffMelee.asset";
        private const string BackendApiConfigPath = "Assets/Content/Modules/InterviewArena/Data/InterviewArena_BackendApiConfig.asset";

        [MenuItem("Learning Architect/Interview Arena/Setup Interview Arena Scenes")]
        public static void SetupAll()
        {
            SetupComplete();
        }

        [MenuItem("Learning Architect/Interview Arena/Setup Complete (Prefabs + Scene + Wire Player)")]
        public static void SetupComplete()
        {
            EnsureInterviewArenaPhysicsLayers();
            ApplyLayerMasksToConfigAssets();
            CreatePlayerPrefabAsset();
            CreateEnemyPrefabAsset();
            EnsureScene();
            WireScenePlayerFromPrefab();
            RegisterBuildScenes();
            AddLaunchDockToHubPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("[InterviewArena] Setup complete: prefabs, scene, player wire, build settings, hub dock.");
        }

        [MenuItem("Learning Architect/Interview Arena/Create Or Update Arena Scene")]
        public static void EnsureScene()
        {
            EnsureFolder("Assets", "Content");
            EnsureFolder("Assets/Content", "Modules");
            EnsureFolder("Assets/Content/Modules", "InterviewArena");
            EnsureFolder("Assets/Content/Modules/InterviewArena", "Data");
            EnsureFolder("Assets/Content/Modules/InterviewArena", "Scenes");

            PlayerConfig playerConfig = EnsurePlayerConfigAsset();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            SceneCompositionRoots roots = CreateSceneCompositionRoots(scene);

            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = "PreviewPlatform";
            platform.transform.SetParent(roots.Static, false);
            platform.transform.localScale = new Vector3(11.25f, 0.07f, 11.25f);
            SetGameObjectLayer(platform, InterviewArenaPhysicsLayers.GroundLayer);
            ConfigurePreviewPlatformCollider(platform);

            BuildPrototypeObstacles(roots.Static);
            CreateFinishPortal(roots.Dynamic);
            Collider platformCollider = platform.GetComponent<Collider>();
            Transform spawnPoint = CreateSpawnPoint(roots.SpawnPoints, platformCollider);

            BuildCombatTrainingDummies(roots.Actors);
            BuildCombatEnemies(roots.Actors);
            BuildBuffPickups(EnsureChildTransform(roots.Runtime, "Pickups"));

            InterviewArenaCombatServices combatServices = CreateSceneCombatServices(
                roots.Gameplay,
                roots.Projectiles,
                EnsureCrossbowWeaponConfigAsset(),
                EnsureBoltPrefabAsset());

            GameObject bootstrapObject = new GameObject("InterviewArenaBootstrap");
            bootstrapObject.transform.SetParent(roots.Gameplay, false);
            InterviewArenaRuntimeContext context = bootstrapObject.AddComponent<InterviewArenaRuntimeContext>();
            bootstrapObject.AddComponent<InterviewArenaBootstrap>();

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            AddUiInputModule(eventSystem);

            BuildSceneUi(roots.Ui, out Transform hudDynamicRoot, out PlayerIframeHud playerIframeHud, out PlayerBuffHud playerBuffHud, out GameObject popupsCanvasObject);

            BackendApiConfig backendConfig = EnsureBackendApiConfigAsset();
            InterviewArenaOnlineFlowController onlineFlow = WireOnlineFlow(popupsCanvasObject.transform, context, backendConfig);

            ReparentDefaultCameraAndLight(roots.Cameras, roots.Lighting, out InterviewArenaCameraFollow cameraFollow);

            SerializedObject serializedContext = new SerializedObject(context);
            serializedContext.FindProperty("playerConfig").objectReferenceValue = playerConfig;
            serializedContext.FindProperty("arenaFloorCollider").objectReferenceValue = platformCollider;
            serializedContext.FindProperty("playerIframeHud").objectReferenceValue = playerIframeHud;
            serializedContext.FindProperty("playerBuffHud").objectReferenceValue = playerBuffHud;
            serializedContext.FindProperty("levelRoot").objectReferenceValue = roots.Level;
            serializedContext.FindProperty("arenaRoot").objectReferenceValue = roots.Level;
            serializedContext.FindProperty("actorsRoot").objectReferenceValue = roots.Actors;
            serializedContext.FindProperty("runtimeRoot").objectReferenceValue = roots.Runtime;
            serializedContext.FindProperty("projectilesRoot").objectReferenceValue = roots.Projectiles;
            serializedContext.FindProperty("vfxRoot").objectReferenceValue = roots.Vfx;
            serializedContext.FindProperty("canvasHudDynamic").objectReferenceValue = hudDynamicRoot;
            serializedContext.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
            serializedContext.FindProperty("player").objectReferenceValue = null;
            serializedContext.FindProperty("playerPrefab").objectReferenceValue = null;
            serializedContext.FindProperty("cameraFollow").objectReferenceValue = cameraFollow;
            serializedContext.FindProperty("combatServices").objectReferenceValue = combatServices;
            serializedContext.FindProperty("backendConfig").objectReferenceValue = backendConfig;
            serializedContext.FindProperty("onlineFlow").objectReferenceValue = onlineFlow;
            serializedContext.ApplyModifiedPropertiesWithoutUndo();

            if (cameraFollow != null)
            {
                Camera mainCamera = cameraFollow.GetComponent<Camera>();
                if (mainCamera != null)
                {
                    mainCamera.transform.position = new Vector3(0f, 6f, -10f);
                    mainCamera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
                }
            }

            combatServices.ConfigurePlayerProjectilePool(roots.Projectiles);

            EditorSceneManager.SaveScene(scene, ArenaScenePath);
            Debug.Log($"[InterviewArena] Scene saved to {ArenaScenePath} (SCENE_COMPOSITION layout).");
        }

        [MenuItem("Learning Architect/Interview Arena/Create Player Prefab Asset")]
        public static void CreatePlayerPrefabAsset()
        {
            EnsureFolder("Assets/Content/Modules", "InterviewArena");
            EnsureFolder("Assets/Content/Modules/InterviewArena", "Prefabs");
            EnsureFolder("Assets/Content/Modules/InterviewArena", "Data");

            PlayerConfig config = EnsurePlayerConfigAsset();
            MeleeWeaponConfig meleeConfig = EnsureMeleeWeaponConfigAsset();
            CrossbowWeaponConfig crossbowConfig = EnsureCrossbowWeaponConfigAsset();

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "InterviewArenaPlayer";
            SetGameObjectLayer(root, InterviewArenaPhysicsLayers.PlayerLayer);

            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null)
                body = root.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.angularDamping = 0.05f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            ApplyLocomotionMaterial(root);

            GameObject viewPivot = new GameObject("ViewPivot");
            viewPivot.transform.SetParent(root.transform, false);
            viewPivot.transform.localPosition = new Vector3(0f, 0.35f, 0f);

            root.AddComponent<PlayerInputReader>();
            GroundDetector ground = root.AddComponent<GroundDetector>();
            PlayerMotor motor = root.AddComponent<PlayerMotor>();

            GameObject crossbowMuzzle = new GameObject("CrossbowMuzzle");
            crossbowMuzzle.transform.SetParent(viewPivot.transform, false);
            crossbowMuzzle.transform.localPosition = new Vector3(0f, 0.05f, 0.55f);

            MeleeStrikeController melee = root.AddComponent<MeleeStrikeController>();
            melee.ApplyConfig(meleeConfig, viewPivot.transform, CombatTeam.Player);

            CrossbowWeaponController crossbow = root.AddComponent<CrossbowWeaponController>();
            crossbow.ApplyConfig(crossbowConfig, crossbowMuzzle.transform, null, CombatTeam.Player);

            root.AddComponent<PlayerCombat>();

            Health playerHealth = root.AddComponent<Health>();
            playerHealth.ApplyConfig(CombatTeam.Player, 100f, 0.5f);
            root.AddComponent<KnockbackReceiver>();
            root.AddComponent<InvulnerabilityWorldIndicator>();
            root.AddComponent<PlayerBuffController>();
            root.AddComponent<PlayerBuffPickupInteractor>();
            root.AddComponent<PlayerBuffVfx>();

            SerializedObject motorSerialized = new SerializedObject(motor);
            motorSerialized.FindProperty("config").objectReferenceValue = config;
            motorSerialized.FindProperty("viewPivot").objectReferenceValue = viewPivot.transform;
            motorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject groundSerialized = new SerializedObject(ground);
            groundSerialized.FindProperty("config").objectReferenceValue = config;
            groundSerialized.FindProperty("probeOrigin").objectReferenceValue = viewPivot.transform;
            groundSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject meleeSerialized = new SerializedObject(melee);
            meleeSerialized.FindProperty("config").objectReferenceValue = meleeConfig;
            meleeSerialized.FindProperty("strikeOrigin").objectReferenceValue = viewPivot.transform;
            meleeSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject crossbowSerialized = new SerializedObject(crossbow);
            crossbowSerialized.FindProperty("config").objectReferenceValue = crossbowConfig;
            crossbowSerialized.FindProperty("muzzle").objectReferenceValue = crossbowMuzzle.transform;
            crossbowSerialized.FindProperty("projectilePool").objectReferenceValue = null;
            crossbowSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject combatSerialized = new SerializedObject(root.GetComponent<PlayerCombat>());
            combatSerialized.FindProperty("melee").objectReferenceValue = melee;
            combatSerialized.FindProperty("crossbow").objectReferenceValue = crossbow;
            combatSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[InterviewArena] Player prefab saved to {PlayerPrefabPath}. "
                + "Bolt pool lives in scene (ArenaCombatServices), not on the player prefab.");
        }

        [MenuItem("Learning Architect/Interview Arena/Create Enemy Prefab Asset")]
        public static void CreateEnemyPrefabAsset()
        {
            EnsureFolder("Assets/Content/Modules/InterviewArena", "Prefabs");
            EnsureFolder("Assets/Content/Modules/InterviewArena", "Data");

            EnemyConfig enemyConfig = EnsureEnemyConfigAsset();
            GameObject root = BuildEnemyGameObject(enemyConfig);
            PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"[InterviewArena] Enemy prefab saved to {EnemyPrefabPath}");
        }

        [MenuItem("Learning Architect/Interview Arena/Wire Scene Player From Prefab")]
        public static void WireScenePlayerFromPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[InterviewArena] Missing prefab at {PlayerPrefabPath}. Run Create Player Prefab Asset first.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ArenaScenePath, OpenSceneMode.Single);
            InterviewArenaRuntimeContext context = UnityEngine.Object.FindFirstObjectByType<InterviewArenaRuntimeContext>();
            if (context == null)
            {
                Debug.LogError("[InterviewArena] Open InterviewArena scene with InterviewArenaRuntimeContext first.");
                return;
            }

            Transform spawn = context.SpawnPoint;
            if (spawn == null)
                spawn = context.transform;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            StripEmbeddedPlayerBoltPool(instance);
            ParentPlayerUnderActorsRoot(instance, context);

            Collider floor = ResolvePreviewPlatformCollider(context);
            PlayerMotor motor = instance.GetComponent<PlayerMotor>();
            if (motor != null)
                motor.SnapToSpawnPose(spawn.position, spawn.rotation, floor);

            SerializedObject serializedContext = new SerializedObject(context);
            serializedContext.FindProperty("player").objectReferenceValue = motor;
            serializedContext.FindProperty("playerPrefab").objectReferenceValue = prefab;
            if (serializedContext.FindProperty("playerBuffHud").objectReferenceValue == null)
            {
                PlayerBuffHud buffHud = UnityEngine.Object.FindFirstObjectByType<PlayerBuffHud>();
                serializedContext.FindProperty("playerBuffHud").objectReferenceValue = buffHud;
            }

            InterviewArenaCombatServices combatServices = ResolveOrCreateSceneCombatServices(context);
            Transform projectilesRoot = serializedContext.FindProperty("projectilesRoot").objectReferenceValue as Transform;
            if (projectilesRoot != null)
            {
                combatServices.ConfigurePlayerProjectilePool(projectilesRoot);
                ReparentPlayerBoltInstancesToProjectilesRoot(combatServices, projectilesRoot);
            }
            if (combatServices != null && motor != null)
                combatServices.WirePlayerCrossbow(motor);

            serializedContext.FindProperty("arenaFloorCollider").objectReferenceValue = floor;
            serializedContext.ApplyModifiedPropertiesWithoutUndo();

            PlayerIframeHud hud = serializedContext.FindProperty("playerIframeHud").objectReferenceValue as PlayerIframeHud;
            Health playerHealth = instance.GetComponent<Health>();
            if (hud != null && playerHealth != null)
            {
                SerializedObject hudSerialized = new SerializedObject(hud);
                hudSerialized.FindProperty("playerHealth").objectReferenceValue = playerHealth;
                hudSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[InterviewArena] Player instance placed and wired on bootstrap.");
        }

        private static void EnsureInterviewArenaPhysicsLayers()
        {
            if (InterviewArenaPhysicsLayers.LayersConfigured)
                return;

            Debug.LogWarning(
                "[InterviewArena] Physics layers missing. Expected TagManager layers: "
                + InterviewArenaPhysicsLayers.Ground + ", "
                + InterviewArenaPhysicsLayers.Player + ", "
                + InterviewArenaPhysicsLayers.Enemy + ", "
                + InterviewArenaPhysicsLayers.Projectile);
        }

        private static void ApplyLayerMasksToConfigAssets()
        {
            if (!InterviewArenaPhysicsLayers.LayersConfigured)
                return;

            ApplyMask(PlayerConfigPath, "groundMask", InterviewArenaPhysicsLayers.GroundMask);
            ApplyMask(PlayerConfigPath, "obstructionMask", InterviewArenaPhysicsLayers.GroundMask);
            ApplyMask(MeleeConfigPath, "hitMask", InterviewArenaPhysicsLayers.EnemyTargetMask);
            ApplyMask(CrossbowConfigPath, "hitMask", InterviewArenaPhysicsLayers.EnemyTargetMask);
            ApplyMask(EnemyCrossbowConfigPath, "hitMask", InterviewArenaPhysicsLayers.PlayerTargetMask);
            ApplyMask(EnemyConfigPath, "hitMask", InterviewArenaPhysicsLayers.PlayerTargetMask);
        }

        private static void ApplyMask(string assetPath, string propertyName, LayerMask mask)
        {
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null)
                return;

            SerializedObject serialized = new SerializedObject(asset);
            serialized.FindProperty(propertyName).intValue = mask.value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePlayerIframeHudOnContext(
            InterviewArenaRuntimeContext context,
            Health playerHealth)
        {
            if (context == null || playerHealth == null)
                return;

            SerializedObject serializedContext = new SerializedObject(context);
            PlayerIframeHud hud = serializedContext.FindProperty("playerIframeHud").objectReferenceValue as PlayerIframeHud;
            if (hud == null)
                return;

            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetGameObjectLayer(GameObject target, int layer)
        {
            if (target == null || layer < 0)
                return;

            target.layer = layer;
            Transform[] children = target.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
                children[i].gameObject.layer = layer;
        }

        private static PlayerConfig EnsurePlayerConfigAsset()
        {
            return EnsureAsset(PlayerConfigPath, () => ScriptableObject.CreateInstance<PlayerConfig>());
        }

        private static MeleeWeaponConfig EnsureMeleeWeaponConfigAsset()
        {
            return EnsureAsset(MeleeConfigPath, () => ScriptableObject.CreateInstance<MeleeWeaponConfig>());
        }

        private static CrossbowWeaponConfig EnsureCrossbowWeaponConfigAsset()
        {
            return EnsureAsset(CrossbowConfigPath, () => ScriptableObject.CreateInstance<CrossbowWeaponConfig>());
        }

        private static EnemyConfig EnsureEnemyConfigAsset()
        {
            return EnsureAsset(EnemyConfigPath, () => ScriptableObject.CreateInstance<EnemyConfig>());
        }

        private static GameObject EnsureBoltPrefabAsset()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(BoltPrefabPath);
            if (existing != null)
                return existing;

            EnsureFolder("Assets/Content/Modules/InterviewArena", "Prefabs");

            GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bolt.name = "InterviewArena_CrossbowBolt";
            bolt.transform.localScale = new Vector3(0.08f, 0.08f, 0.28f);
            Collider collider = bolt.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);

            Renderer renderer = bolt.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = new Color(0.55f, 0.42f, 0.28f, 1f);

            bolt.AddComponent<Projectile>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bolt, BoltPrefabPath);
            UnityEngine.Object.DestroyImmediate(bolt);
            return prefab;
        }

        private static T EnsureAsset<T>(string path, System.Func<T> factory) where T : UnityEngine.Object
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;

            EnsureFolder("Assets/Content/Modules/InterviewArena", "Data");
            T asset = factory();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void BuildCombatTrainingDummies(Transform parent)
        {
            CreateTrainingDummy(parent, "TrainingDummy_North", new Vector3(0f, 1.05f, 5.5f));
        }

        private static void BuildCombatEnemies(Transform parent)
        {
            EnemyConfig config = EnsureEnemyConfigAsset();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);

            CreateEnemyInstance(parent, prefab, config, "HollowSoldier_A", new Vector3(3.8f, 1.05f, 2.8f));
            CreateEnemyInstance(parent, prefab, config, "HollowSoldier_B", new Vector3(-3.6f, 1.05f, 3.2f));
        }

        private static void CreateEnemyInstance(
            Transform parent,
            GameObject prefab,
            EnemyConfig config,
            string name,
            Vector3 position)
        {
            GameObject enemy = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : BuildEnemyGameObject(config);

            enemy.name = name;
            enemy.transform.SetParent(parent, false);
            enemy.transform.position = position;
            Transform facingPivot = enemy.transform.Find("FacingPivot");
            if (config != null && enemy.GetComponent<EnemyBrain>() is EnemyBrain brain)
                brain.ApplyConfig(config, facingPivot);

            if (enemy.TryGetComponent<EnemyRespawnController>(out EnemyRespawnController respawn))
            {
                respawn.ApplyConfig(config);
                respawn.CaptureSpawnPose();
            }
        }

        private static GameObject BuildEnemyGameObject(EnemyConfig config)
        {
            config = config != null ? config : EnsureEnemyConfigAsset();
            CrossbowWeaponConfig enemyCrossbow = EnsureEnemyCrossbowConfigAsset();
            GameObject boltPrefab = EnsureBoltPrefabAsset();
            WireEnemyConfigAsset(config, enemyCrossbow);

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "InterviewArena_HollowSoldier";
            root.transform.localScale = new Vector3(0.92f, 1.1f, 0.92f);
            SetGameObjectLayer(root, InterviewArenaPhysicsLayers.EnemyLayer);

            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null)
                body = root.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = new Color(0.16f, 0.12f, 0.2f, 1f);

            GameObject facingPivot = new GameObject("FacingPivot");
            facingPivot.transform.SetParent(root.transform, false);
            facingPivot.transform.localPosition = new Vector3(0f, 0.35f, 0f);

            GameObject crossbowMuzzle = new GameObject("CrossbowMuzzle");
            crossbowMuzzle.transform.SetParent(facingPivot.transform, false);
            crossbowMuzzle.transform.localPosition = new Vector3(0f, 0.05f, 0.5f);

            GameObject boltPoolHost = new GameObject("EnemyCrossbowBoltPool");
            boltPoolHost.transform.SetParent(root.transform, false);
            ProjectilePool boltPool = boltPoolHost.AddComponent<ProjectilePool>();
            int poolSize = config != null ? config.CrossbowPoolSize : 12;
            boltPool.ApplyConfig(boltPrefab, poolSize, enemyCrossbow, CombatTeam.Enemy, 0);

            Health health = root.AddComponent<Health>();
            root.AddComponent<KnockbackReceiver>();
            EnemySensor sensor = root.AddComponent<EnemySensor>();
            EnemyMotor motor = root.AddComponent<EnemyMotor>();
            EnemyMeleeAttack melee = root.AddComponent<EnemyMeleeAttack>();
            EnemyCrossbowAttack crossbow = root.AddComponent<EnemyCrossbowAttack>();
            crossbow.ApplyConfig(enemyCrossbow, crossbowMuzzle.transform, boltPool, CombatTeam.Enemy);
            EnemyRespawnController respawn = root.AddComponent<EnemyRespawnController>();
            respawn.ApplyConfig(config);
            EnemyBrain brain = root.AddComponent<EnemyBrain>();
            brain.ApplyConfig(config, facingPivot.transform);

            SerializedObject sensorSerialized = new SerializedObject(sensor);
            sensorSerialized.FindProperty("awarenessOrigin").objectReferenceValue = facingPivot.transform;
            sensorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject meleeSerialized = new SerializedObject(melee);
            meleeSerialized.FindProperty("strikeOrigin").objectReferenceValue = facingPivot.transform;
            meleeSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject motorSerialized = new SerializedObject(motor);
            motorSerialized.FindProperty("facingPivot").objectReferenceValue = facingPivot.transform;
            motorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject poolSerialized = new SerializedObject(boltPool);
            poolSerialized.FindProperty("boltPrefab").objectReferenceValue = boltPrefab;
            poolSerialized.FindProperty("poolSize").intValue = poolSize;
            poolSerialized.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static void WireEnemyConfigAsset(EnemyConfig config, CrossbowWeaponConfig crossbow)
        {
            if (config == null || crossbow == null)
                return;

            SerializedObject serialized = new SerializedObject(config);
            serialized.FindProperty("crossbowConfig").objectReferenceValue = crossbow;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CrossbowWeaponConfig EnsureEnemyCrossbowConfigAsset()
        {
            CrossbowWeaponConfig existing = AssetDatabase.LoadAssetAtPath<CrossbowWeaponConfig>(EnemyCrossbowConfigPath);
            if (existing != null)
                return existing;

            CrossbowWeaponConfig asset = ScriptableObject.CreateInstance<CrossbowWeaponConfig>();
            AssetDatabase.CreateAsset(asset, EnemyCrossbowConfigPath);
            return asset;
        }

        private static PlayerIframeHud CreatePlayerIframeHud(Transform canvasParent)
        {
            GameObject host = new GameObject("Container - PlayerIframeHud", typeof(RectTransform), typeof(CanvasGroup), typeof(PlayerIframeHud));
            host.transform.SetParent(canvasParent, false);

            RectTransform hostRect = host.GetComponent<RectTransform>();
            hostRect.anchorMin = new Vector2(0f, 1f);
            hostRect.anchorMax = new Vector2(0f, 1f);
            hostRect.pivot = new Vector2(0f, 1f);
            hostRect.anchoredPosition = new Vector2(28f, -148f);
            hostRect.sizeDelta = new Vector2(220f, 18f);

            CanvasGroup group = host.GetComponent<CanvasGroup>();

            GameObject background = new GameObject("Image - Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(host.transform, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.75f);

            GameObject fill = new GameObject("Image - Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(host.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = ShowcasePalette.AccentSoft(0.95f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            TextMeshProUGUI label = CreateLabel(
                host.transform,
                "Text - Label",
                12,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 20f),
                new Vector2(220f, 18f));
            label.alignment = TextAlignmentOptions.Left;
            label.text = "I-Frames";

            PlayerIframeHud hud = host.GetComponent<PlayerIframeHud>();
            SerializedObject hudSerialized = new SerializedObject(hud);
            hudSerialized.FindProperty("fillImage").objectReferenceValue = fillImage;
            hudSerialized.FindProperty("canvasGroup").objectReferenceValue = group;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static void CreateTrainingDummy(Transform parent, string name, Vector3 position)
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = name;
            dummy.transform.SetParent(parent, false);
            dummy.transform.position = position;
            dummy.transform.localScale = new Vector3(0.95f, 1.15f, 0.95f);
            SetGameObjectLayer(dummy, InterviewArenaPhysicsLayers.EnemyLayer);

            Rigidbody body = dummy.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            Health health = dummy.AddComponent<Health>();
            health.ApplyConfig(CombatTeam.Enemy, 120f, 0.25f);
            dummy.AddComponent<KnockbackReceiver>();
            dummy.AddComponent<CombatTrainingDummy>();

            Renderer renderer = dummy.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = new Color(0.72f, 0.2f, 0.24f, 0.95f);
        }

        private static void BuildPrototypeObstacles(Transform parent)
        {
            CreatePhysicsCube(parent, "Cube_A", new Vector3(-3f, 0.55f, 2f), new Vector3(1.2f, 1.1f, 1.2f));
            CreatePhysicsCube(parent, "Cube_B", new Vector3(2.5f, 0.75f, 1f), new Vector3(1.6f, 1.5f, 1f));
            CreatePhysicsCube(parent, "Cube_C", new Vector3(0f, 0.35f, 4.5f), new Vector3(2.2f, 0.7f, 0.8f));

            CreatePhysicsCube(parent, "Cube_D", new Vector3(-2.2f, 0.55f, -2.4f), new Vector3(1f, 1f, 1f));

            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Slope";
            ramp.transform.SetParent(parent, false);
            ramp.transform.position = new Vector3(-4.5f, 0.45f, -1.5f);
            ramp.transform.rotation = Quaternion.Euler(0f, 25f, 18f);
            ramp.transform.localScale = new Vector3(2.8f, 0.25f, 1.6f);
            SetGameObjectLayer(ramp, InterviewArenaPhysicsLayers.GroundLayer);
            EnsureRigidbody(ramp, false);
            ApplyLocomotionMaterial(ramp);
        }

        private static void BuildBuffPickups(Transform pickupsRoot)
        {
            if (pickupsRoot == null)
                return;

            BuffPickupConfig speed = EnsureBuffSpeedConfigAsset();
            BuffPickupConfig melee = EnsureBuffMeleeConfigAsset();

            CreateBuffPickup(pickupsRoot, speed, "BuffPickup_Speed", new Vector3(-2.4f, 0.55f, -1.2f));
            CreateBuffPickup(pickupsRoot, melee, "BuffPickup_Melee", new Vector3(2.6f, 0.55f, -1.4f));
        }

        private static void CreateBuffPickup(
            Transform parent,
            BuffPickupConfig config,
            string name,
            Vector3 position)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.55f, 0.12f, 0.55f);
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                UnityEngine.Object.DestroyImmediate(visualCollider);

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && config != null)
                renderer.sharedMaterial.color = config.WorldColor;

            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = config != null ? config.PickupRadius : 1f;

            BuffPickup pickup = root.AddComponent<BuffPickup>();
            pickup.ApplyConfig(config);

            SerializedObject serialized = new SerializedObject(pickup);
            serialized.FindProperty("pickupTrigger").objectReferenceValue = trigger;
            serialized.FindProperty("visualRoot").objectReferenceValue = visual.transform;
            serialized.FindProperty("visualRenderer").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static BuffPickupConfig EnsureBuffSpeedConfigAsset()
        {
            BuffPickupConfig existing = AssetDatabase.LoadAssetAtPath<BuffPickupConfig>(BuffSpeedConfigPath);
            if (existing != null)
                return existing;

            BuffPickupConfig asset = ScriptableObject.CreateInstance<BuffPickupConfig>();
            AssetDatabase.CreateAsset(asset, BuffSpeedConfigPath);
            ConfigureBuffAsset(asset, BuffKind.MoveSpeed, 1.35f, 10f, new Color(0.35f, 0.92f, 1f, 0.95f));
            return asset;
        }

        private static BuffPickupConfig EnsureBuffMeleeConfigAsset()
        {
            BuffPickupConfig existing = AssetDatabase.LoadAssetAtPath<BuffPickupConfig>(BuffMeleeConfigPath);
            if (existing != null)
                return existing;

            BuffPickupConfig asset = ScriptableObject.CreateInstance<BuffPickupConfig>();
            AssetDatabase.CreateAsset(asset, BuffMeleeConfigPath);
            ConfigureBuffAsset(asset, BuffKind.MeleeDamage, 1.45f, 10f, new Color(1f, 0.62f, 0.28f, 0.95f));
            return asset;
        }

        private static void ConfigureBuffAsset(
            BuffPickupConfig asset,
            BuffKind kind,
            float multiplier,
            float duration,
            Color color)
        {
            SerializedObject serialized = new SerializedObject(asset);
            serialized.FindProperty("kind").enumValueIndex = (int)kind;
            serialized.FindProperty("multiplier").floatValue = multiplier;
            serialized.FindProperty("duration").floatValue = duration;
            serialized.FindProperty("holdDuration").floatValue = 0.55f;
            serialized.FindProperty("pickupRadius").floatValue = 1.1f;
            serialized.FindProperty("respawnDelay").floatValue = 18f;
            serialized.FindProperty("worldColor").colorValue = color;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePhysicsCube(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            SetGameObjectLayer(cube, InterviewArenaPhysicsLayers.GroundLayer);
            EnsureRigidbody(cube, false);
            ApplyLocomotionMaterial(cube);
        }

        private static void ApplyLocomotionMaterial(GameObject target)
        {
            if (target == null)
                return;

            PhysicsMaterial material = InterviewArenaPhysicsMaterials.EnsureLocomotionMaterialAsset();
            Collider collider = target.GetComponent<Collider>();
            InterviewArenaPhysicsMaterials.ApplyLocomotionMaterial(collider, material);
        }

        private static void EnsureRigidbody(GameObject target, bool useGravity)
        {
            Rigidbody body = target.GetComponent<Rigidbody>();
            if (body == null)
                body = target.AddComponent<Rigidbody>();

            body.isKinematic = !useGravity;
            body.useGravity = useGravity;
        }

        private static Transform CreateSpawnPoint(Transform parent, Collider platformCollider)
        {
            GameObject spawn = new GameObject("SpawnPoint_Player_01");
            spawn.transform.SetParent(parent, false);
            spawn.transform.position = InterviewArenaPlatformLayout.ResolveSpawnPosition(
                new Vector3(0f, 0f, -4f),
                1f,
                platformCollider);
            return spawn.transform;
        }

        private static void ConfigurePreviewPlatformCollider(GameObject platform)
        {
            if (platform == null)
                return;

            CapsuleCollider capsule = platform.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                MeshCollider meshCollider = platform.GetComponent<MeshCollider>();
                if (meshCollider != null)
                    UnityEngine.Object.DestroyImmediate(meshCollider);

                BoxCollider boxCollider = platform.GetComponent<BoxCollider>();
                if (boxCollider != null)
                    UnityEngine.Object.DestroyImmediate(boxCollider);

                capsule = platform.AddComponent<CapsuleCollider>();
            }

            capsule.isTrigger = false;
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.direction = 1;
            capsule.center = Vector3.zero;
            ApplyLocomotionMaterial(platform);
        }

        private static Collider ResolvePreviewPlatformCollider(InterviewArenaRuntimeContext context)
        {
            if (context == null)
                return null;

            SerializedObject serialized = new SerializedObject(context);
            Transform levelRoot = serialized.FindProperty("levelRoot").objectReferenceValue as Transform;
            if (levelRoot == null)
                levelRoot = serialized.FindProperty("arenaRoot").objectReferenceValue as Transform;
            if (levelRoot == null)
                return null;

            Transform platform = levelRoot.Find("Static/PreviewPlatform");
            if (platform == null)
                platform = levelRoot.Find("PreviewPlatform");

            return platform != null ? platform.GetComponent<Collider>() : null;
        }

        [MenuItem("Learning Architect/Interview Arena/Migrate Scene Composition (Pools Off Player)")]
        public static void MigrateSceneComposition()
        {
            Scene scene = EditorSceneManager.OpenScene(ArenaScenePath, OpenSceneMode.Single);
            InterviewArenaRuntimeContext context = UnityEngine.Object.FindFirstObjectByType<InterviewArenaRuntimeContext>();
            if (context == null)
            {
                Debug.LogError("[InterviewArena] Scene has no InterviewArenaRuntimeContext.");
                return;
            }

            SceneCompositionRoots roots = CreateSceneCompositionRoots(scene);
            RestructureLegacySceneObjects(scene, context, roots);

            SerializedObject serializedContext = new SerializedObject(context);
            serializedContext.FindProperty("levelRoot").objectReferenceValue = roots.Level;
            serializedContext.FindProperty("arenaRoot").objectReferenceValue = roots.Level;
            serializedContext.FindProperty("actorsRoot").objectReferenceValue = roots.Actors;
            serializedContext.FindProperty("runtimeRoot").objectReferenceValue = roots.Runtime;
            serializedContext.FindProperty("projectilesRoot").objectReferenceValue = roots.Projectiles;
            serializedContext.FindProperty("vfxRoot").objectReferenceValue = roots.Vfx;
            if (serializedContext.FindProperty("canvasHudDynamic").objectReferenceValue == null)
            {
                Transform hudRoot = roots.Ui.Find("Canvas_HUD_Dynamic");
                if (hudRoot != null)
                    serializedContext.FindProperty("canvasHudDynamic").objectReferenceValue = hudRoot;
            }

            if (serializedContext.FindProperty("arenaFloorCollider").objectReferenceValue == null)
                serializedContext.FindProperty("arenaFloorCollider").objectReferenceValue =
                    ResolvePreviewPlatformCollider(context);

            if (serializedContext.FindProperty("playerIframeHud").objectReferenceValue == null)
            {
                Transform hudCanvas = roots.Ui.Find("Canvas_HUD_Dynamic");
                if (hudCanvas != null)
                {
                    PlayerIframeHud hud = hudCanvas.GetComponentInChildren<PlayerIframeHud>(true);
                    serializedContext.FindProperty("playerIframeHud").objectReferenceValue = hud;
                }
            }

            if (serializedContext.FindProperty("playerBuffHud").objectReferenceValue == null)
            {
                Transform hudCanvas = roots.Ui.Find("Canvas_HUD_Dynamic");
                if (hudCanvas != null)
                {
                    PlayerBuffHud hud = hudCanvas.GetComponentInChildren<PlayerBuffHud>(true);
                    serializedContext.FindProperty("playerBuffHud").objectReferenceValue = hud;
                }
            }

            Transform bootstrap = context.transform;
            if (bootstrap.parent != roots.Gameplay)
                bootstrap.SetParent(roots.Gameplay, true);

            InterviewArenaCombatServices combatServices = ResolveOrCreateSceneCombatServices(context, roots);
            combatServices.ConfigurePlayerProjectilePool(roots.Projectiles);
            ReparentPlayerBoltInstancesToProjectilesRoot(combatServices, roots.Projectiles);

            PlayerMotor player = context.Player;
            if (player != null)
            {
                StripEmbeddedPlayerBoltPool(player.gameObject);
                ParentPlayerUnderActorsRoot(player.gameObject, context);
                combatServices.WirePlayerCrossbow(player);
            }

            ReparentDefaultCameraAndLight(roots.Cameras, roots.Lighting, out _);
            serializedContext.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[InterviewArena] Scene composition migrated to SCENE_COMPOSITION layout.");
        }

        private static InterviewArenaCombatServices CreateSceneCombatServices(
            Transform gameplayRoot,
            Transform projectilesRoot,
            CrossbowWeaponConfig crossbowConfig,
            GameObject boltPrefab)
        {
            Transform servicesParent = gameplayRoot.Find("ArenaCombatServices");
            GameObject servicesObject = servicesParent != null
                ? servicesParent.gameObject
                : new GameObject("ArenaCombatServices");
            if (servicesParent == null)
                servicesObject.transform.SetParent(gameplayRoot, false);

            InterviewArenaCombatServices services = servicesObject.GetComponent<InterviewArenaCombatServices>();
            if (services == null)
                services = servicesObject.AddComponent<InterviewArenaCombatServices>();

            CombatHitFeedback feedback = servicesObject.GetComponent<CombatHitFeedback>();
            if (feedback == null)
                feedback = servicesObject.AddComponent<CombatHitFeedback>();

            Transform poolParent = servicesObject.transform.Find("PlayerCrossbowBoltPool");
            GameObject poolHost = poolParent != null
                ? poolParent.gameObject
                : new GameObject("PlayerCrossbowBoltPool");
            if (poolParent == null)
                poolHost.transform.SetParent(servicesObject.transform, false);

            ProjectilePool pool = poolHost.GetComponent<ProjectilePool>();
            if (pool == null)
                pool = poolHost.AddComponent<ProjectilePool>();

            pool.SetProjectilesRoot(projectilesRoot);
            pool.ApplyConfig(boltPrefab, 24, crossbowConfig, CombatTeam.Player, 0);

            SerializedObject serialized = new SerializedObject(services);
            serialized.FindProperty("playerCrossbowConfig").objectReferenceValue = crossbowConfig;
            serialized.FindProperty("playerCrossbowPool").objectReferenceValue = pool;
            serialized.FindProperty("combatFeedback").objectReferenceValue = feedback;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return services;
        }

        private static InterviewArenaCombatServices ResolveOrCreateSceneCombatServices(
            InterviewArenaRuntimeContext context,
            SceneCompositionRoots roots = default)
        {
            SerializedObject serialized = new SerializedObject(context);
            var services = serialized.FindProperty("combatServices").objectReferenceValue as InterviewArenaCombatServices;
            if (services != null)
                return services;

            services = UnityEngine.Object.FindFirstObjectByType<InterviewArenaCombatServices>();
            if (services != null)
            {
                serialized.FindProperty("combatServices").objectReferenceValue = services;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                return services;
            }

            Transform gameplay = roots.Gameplay != null ? roots.Gameplay : context.transform.parent;
            Transform projectiles = roots.Projectiles;
            if (gameplay == null)
                gameplay = context.transform;

            if (projectiles == null)
            {
                Transform runtime = SceneRootTransform(context.gameObject.scene, "Runtime");
                projectiles = runtime != null ? runtime.Find("Projectiles") : null;
            }

            services = CreateSceneCombatServices(
                gameplay,
                projectiles,
                EnsureCrossbowWeaponConfigAsset(),
                EnsureBoltPrefabAsset());
            serialized.FindProperty("combatServices").objectReferenceValue = services;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return services;
        }

        private static void StripEmbeddedPlayerBoltPool(GameObject playerInstance)
        {
            if (playerInstance == null)
                return;

            Transform pool = playerInstance.transform.Find("CrossbowBoltPool");
            if (pool != null)
                UnityEngine.Object.DestroyImmediate(pool.gameObject);
        }

        private static void ParentPlayerUnderActorsRoot(GameObject playerInstance, InterviewArenaRuntimeContext context)
        {
            SerializedObject serialized = new SerializedObject(context);
            Transform actorsRoot = serialized.FindProperty("actorsRoot").objectReferenceValue as Transform;
            if (actorsRoot == null)
            {
                Transform levelRoot = serialized.FindProperty("levelRoot").objectReferenceValue as Transform;
                if (levelRoot == null)
                    levelRoot = serialized.FindProperty("arenaRoot").objectReferenceValue as Transform;
                actorsRoot = levelRoot;
            }

            if (actorsRoot == null)
                actorsRoot = context.transform;

            playerInstance.transform.SetParent(actorsRoot, true);
        }

        private static void CreateFinishPortal(Transform parent)
        {
            GameObject portal = new GameObject("FinishPortal");
            portal.transform.SetParent(parent, false);
            portal.transform.position = new Vector3(0f, 1.2f, 7.5f);
            portal.transform.localScale = new Vector3(2f, 2.2f, 1.2f);

            BoxCollider trigger = portal.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            portal.AddComponent<FinishPortal>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "PortalVisual";
            visual.transform.SetParent(portal.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                UnityEngine.Object.DestroyImmediate(visualCollider);

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = new Color(0.2f, 0.95f, 0.55f, 0.85f);
        }

        [MenuItem("Learning Architect/Interview Arena/Register Build Scenes")]
        public static void RegisterBuildScenes()
        {
            EditorBuildSettingsScene[] scenes =
            {
                new EditorBuildSettingsScene(ShowcaseScenePath, true),
                new EditorBuildSettingsScene(ArenaScenePath, true)
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[InterviewArena] Build settings: ArchitectureShowcase (0), InterviewArena (1).");
        }

        [MenuItem("Learning Architect/Interview Arena/Add Launch Dock To Hub Prefab")]
        public static void AddLaunchDockToHubPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(HubPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[InterviewArena] Could not load hub prefab at {HubPrefabPath}");
                return;
            }

            Canvas canvas = prefabRoot.GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                Debug.LogError("[InterviewArena] Hub prefab has no Canvas.");
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return;
            }

            InterviewArenaLaunchDock existing = canvas.GetComponentInChildren<InterviewArenaLaunchDock>(true);
            if (existing == null)
            {
                GameObject host = new GameObject("InterviewArenaLaunchDockHost", typeof(RectTransform));
                host.transform.SetParent(canvas.transform, false);
                host.AddComponent<InterviewArenaLaunchDock>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, HubPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            Debug.Log("[InterviewArena] Launch dock added to ArchitectureShowcaseHub.prefab.");
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            float fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = ShowcasePalette.TextPrimary;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = buttonObject.GetComponent<Image>();
            image.color = ShowcasePalette.AccentSoft(0.35f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            GameObject labelObject = new GameObject("Text - Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 15f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = ShowcasePalette.TextPrimary;
            label.raycastTarget = false;
            return button;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        private struct SceneCompositionRoots
        {
            public Transform Level;
            public Transform Static;
            public Transform Dynamic;
            public Transform SpawnPoints;
            public Transform Gameplay;
            public Transform Actors;
            public Transform Runtime;
            public Transform Projectiles;
            public Transform Vfx;
            public Transform Ui;
            public Transform Cameras;
            public Transform Lighting;
        }

        private static SceneCompositionRoots CreateSceneCompositionRoots(Scene scene)
        {
            SceneCompositionRoots roots = new SceneCompositionRoots();
            roots.Level = EnsureRootTransform(scene, "Level");
            roots.Static = EnsureChildTransform(roots.Level, "Static");
            roots.Dynamic = EnsureChildTransform(roots.Level, "Dynamic");
            roots.SpawnPoints = EnsureChildTransform(roots.Level, "SpawnPoints");
            roots.Gameplay = EnsureRootTransform(scene, "Gameplay");
            roots.Actors = EnsureRootTransform(scene, "Actors");
            roots.Runtime = EnsureRootTransform(scene, "Runtime");
            roots.Projectiles = EnsureChildTransform(roots.Runtime, "Projectiles");
            roots.Vfx = EnsureChildTransform(roots.Runtime, "VFX");
            EnsureChildTransform(roots.Runtime, "Pickups");
            EnsureChildTransform(roots.Runtime, "TemporaryObjects");
            roots.Ui = EnsureRootTransform(scene, "UI");
            roots.Cameras = EnsureRootTransform(scene, "Cameras");
            roots.Lighting = EnsureRootTransform(scene, "Lighting");
            return roots;
        }

        private static Transform EnsureRootTransform(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name)
                    return roots[i].transform;
            }

            GameObject created = new GameObject(name);
            return created.transform;
        }

        private static Transform EnsureChildTransform(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                return existing;

            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            return created.transform;
        }

        private static Transform SceneRootTransform(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name)
                    return roots[i].transform;
            }

            return null;
        }

        private static void BuildSceneUi(
            Transform uiRoot,
            out Transform hudDynamicRoot,
            out PlayerIframeHud playerIframeHud,
            out PlayerBuffHud playerBuffHud,
            out GameObject popupsCanvasObject)
        {
            GameObject staticCanvasObject = CreateOverlayCanvas(uiRoot, "Canvas_Static", 0);
            GameObject hudCanvasObject = CreateOverlayCanvas(uiRoot, "Canvas_HUD_Dynamic", 10);
            GameObject popupsCanvas = CreateOverlayCanvas(uiRoot, "Canvas_Popups", 20);
            GameObject debugCanvasObject = CreateOverlayCanvas(uiRoot, "Canvas_Debug", 100);
            debugCanvasObject.SetActive(false);

            GameObject staticPanel = new GameObject("Container - InterviewArenaHudStatic", typeof(RectTransform));
            staticPanel.transform.SetParent(staticCanvasObject.transform, false);
            StretchRect(staticPanel.GetComponent<RectTransform>());

            InterviewArenaSceneUI sceneUi = staticCanvasObject.AddComponent<InterviewArenaSceneUI>();

            TextMeshProUGUI title = CreateLabel(
                staticPanel.transform,
                "Text - Title",
                28,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -48f),
                new Vector2(900f, 48f));
            TextMeshProUGUI subtitle = CreateLabel(
                staticPanel.transform,
                "Text - Subtitle",
                16,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -108f),
                new Vector2(960f, 72f));
            subtitle.color = ShowcasePalette.TextSecondary;

            GameObject popupPanel = new GameObject("Container - InterviewArenaPopups", typeof(RectTransform));
            popupPanel.transform.SetParent(popupsCanvas.transform, false);
            StretchRect(popupPanel.GetComponent<RectTransform>());

            Button backButton = CreateButton(
                popupPanel.transform,
                "Container - BackButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 56f),
                new Vector2(360f, 40f));
            TextMeshProUGUI backLabel = backButton.GetComponentInChildren<TextMeshProUGUI>();

            SerializedObject serializedUi = new SerializedObject(sceneUi);
            serializedUi.FindProperty("titleLabel").objectReferenceValue = title;
            serializedUi.FindProperty("subtitleLabel").objectReferenceValue = subtitle;
            serializedUi.FindProperty("backButton").objectReferenceValue = backButton;
            serializedUi.FindProperty("backButtonLabel").objectReferenceValue = backLabel;
            serializedUi.ApplyModifiedPropertiesWithoutUndo();

            playerIframeHud = CreatePlayerIframeHud(hudCanvasObject.transform);
            playerBuffHud = CreatePlayerBuffHud(hudCanvasObject.transform);
            hudDynamicRoot = hudCanvasObject.transform;
            popupsCanvasObject = popupsCanvas;
        }

        private static BackendApiConfig EnsureBackendApiConfigAsset()
        {
            BackendApiConfig existing = AssetDatabase.LoadAssetAtPath<BackendApiConfig>(BackendApiConfigPath);
            if (existing != null)
                return existing;

            EnsureFolder("Assets/Content/Modules/InterviewArena", "Data");
            BackendApiConfig asset = ScriptableObject.CreateInstance<BackendApiConfig>();
            AssetDatabase.CreateAsset(asset, BackendApiConfigPath);
            return asset;
        }

        private static InterviewArenaOnlineFlowController WireOnlineFlow(
            Transform popupsCanvas,
            InterviewArenaRuntimeContext context,
            BackendApiConfig backendConfig)
        {
            GameObject host = new GameObject("InterviewArenaOnlineFlow", typeof(RectTransform), typeof(InterviewArenaOnlineFlowController));
            host.transform.SetParent(popupsCanvas, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            StretchRect(rect);

            InterviewArenaOnlineFlowController flow = host.GetComponent<InterviewArenaOnlineFlowController>();
            SerializedObject serialized = new SerializedObject(flow);
            serialized.FindProperty("context").objectReferenceValue = context;
            serialized.FindProperty("backendConfig").objectReferenceValue = backendConfig;
            serialized.FindProperty("panelRoot").objectReferenceValue = rect;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return flow;
        }

        private static PlayerBuffHud CreatePlayerBuffHud(Transform canvasParent)
        {
            GameObject host = new GameObject("Container - PlayerBuffHud", typeof(RectTransform), typeof(PlayerBuffHud));
            host.transform.SetParent(canvasParent, false);

            RectTransform hostRect = host.GetComponent<RectTransform>();
            hostRect.anchorMin = new Vector2(0f, 1f);
            hostRect.anchorMax = new Vector2(0f, 1f);
            hostRect.pivot = new Vector2(0f, 1f);
            hostRect.anchoredPosition = new Vector2(28f, -188f);
            hostRect.sizeDelta = new Vector2(220f, 56f);

            GameObject background = new GameObject("Image - Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(host.transform, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.65f);

            // Row 1: speed
            Image speedIcon = CreateBuffIcon(host.transform, "Image - SpeedIcon", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -14f), new Vector2(18f, 18f), new Color(0.35f, 0.92f, 1f, 0.95f));
            TextMeshProUGUI speedTimer = CreateLabel(host.transform, "Text - SpeedTimer", 13, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -14f), new Vector2(60f, 18f));
            speedTimer.alignment = TextAlignmentOptions.Left;

            // Row 2: melee
            Image meleeIcon = CreateBuffIcon(host.transform, "Image - MeleeIcon", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -38f), new Vector2(18f, 18f), new Color(1f, 0.62f, 0.28f, 0.95f));
            TextMeshProUGUI meleeTimer = CreateLabel(host.transform, "Text - MeleeTimer", 13, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -38f), new Vector2(60f, 18f));
            meleeTimer.alignment = TextAlignmentOptions.Left;

            // Hold E fill (thin bar at bottom)
            GameObject holdFillObject = new GameObject("Image - PickupHoldFill", typeof(RectTransform), typeof(Image));
            holdFillObject.transform.SetParent(host.transform, false);
            RectTransform holdRect = holdFillObject.GetComponent<RectTransform>();
            holdRect.anchorMin = new Vector2(0f, 0f);
            holdRect.anchorMax = new Vector2(1f, 0f);
            holdRect.pivot = new Vector2(0.5f, 0f);
            holdRect.anchoredPosition = new Vector2(0f, 6f);
            holdRect.sizeDelta = new Vector2(-20f, 6f);

            Image holdFill = holdFillObject.GetComponent<Image>();
            holdFill.color = ShowcasePalette.AccentSoft(0.9f);
            holdFill.type = Image.Type.Filled;
            holdFill.fillMethod = Image.FillMethod.Horizontal;
            holdFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            holdFill.fillAmount = 0f;
            holdFill.enabled = false;

            TextMeshProUGUI holdLabel = CreateLabel(host.transform, "Text - PickupHoldLabel", 11, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-12f, 20f), new Vector2(90f, 18f));
            holdLabel.alignment = TextAlignmentOptions.Right;
            holdLabel.text = "Hold E";
            holdLabel.enabled = false;

            PlayerBuffHud hud = host.GetComponent<PlayerBuffHud>();
            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("speedIcon").objectReferenceValue = speedIcon;
            serializedHud.FindProperty("speedTimerLabel").objectReferenceValue = speedTimer;
            serializedHud.FindProperty("meleeIcon").objectReferenceValue = meleeIcon;
            serializedHud.FindProperty("meleeTimerLabel").objectReferenceValue = meleeTimer;
            serializedHud.FindProperty("pickupHoldFill").objectReferenceValue = holdFill;
            serializedHud.FindProperty("pickupHoldLabel").objectReferenceValue = holdLabel;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            // Start hidden until wired.
            speedIcon.enabled = false;
            meleeIcon.enabled = false;
            speedTimer.enabled = false;
            meleeTimer.enabled = false;

            return hud;
        }

        private static Image CreateBuffIcon(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            GameObject iconObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(parent, false);

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = iconObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static GameObject CreateOverlayCanvas(Transform parent, string name, int sortOrder)
        {
            GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return canvasObject;
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ReparentDefaultCameraAndLight(
            Transform camerasRoot,
            Transform lightingRoot,
            out InterviewArenaCameraFollow cameraFollow)
        {
            cameraFollow = null;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.name = "MainCamera";
                mainCamera.transform.SetParent(camerasRoot, true);
                cameraFollow = mainCamera.GetComponent<InterviewArenaCameraFollow>();
                if (cameraFollow == null)
                    cameraFollow = mainCamera.gameObject.AddComponent<InterviewArenaCameraFollow>();
            }

            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light.type != LightType.Directional)
                    continue;

                if (light.transform.parent == lightingRoot)
                    continue;

                light.gameObject.name = "Directional Light";
                light.transform.SetParent(lightingRoot, true);
            }
        }

        private static void RestructureLegacySceneObjects(
            Scene scene,
            InterviewArenaRuntimeContext context,
            SceneCompositionRoots roots)
        {
            Transform arenaRoot = SceneRootTransform(scene, "ArenaRoot");
            if (arenaRoot == null)
            {
                SerializedObject serialized = new SerializedObject(context);
                Transform legacyRoot = serialized.FindProperty("arenaRoot").objectReferenceValue as Transform;
                if (legacyRoot != null && legacyRoot.name == "ArenaRoot")
                    arenaRoot = legacyRoot;
            }

            if (arenaRoot != null)
            {
                for (int i = arenaRoot.childCount - 1; i >= 0; i--)
                    MoveTransformToCompositionSection(arenaRoot.GetChild(i), roots);

                if (arenaRoot.childCount == 0)
                    UnityEngine.Object.DestroyImmediate(arenaRoot.gameObject);
            }

            PlayerMotor player = context.Player;
            if (player != null)
                player.transform.SetParent(roots.Actors, true);

            Transform legacySpawn = roots.SpawnPoints.Find("PlayerSpawn");
            if (legacySpawn != null)
                legacySpawn.name = "SpawnPoint_Player_01";

            InterviewArenaCombatServices services = context.GetComponentInParent<InterviewArenaCombatServices>();
            if (services == null)
                services = UnityEngine.Object.FindFirstObjectByType<InterviewArenaCombatServices>();
            if (services != null && services.transform.parent != roots.Gameplay)
                services.transform.SetParent(roots.Gameplay, true);

            GameObject legacyCanvas = GameObject.Find("Container - InterviewArenaCanvas");
            if (legacyCanvas != null)
                UnityEngine.Object.DestroyImmediate(legacyCanvas);
        }

        private static void MoveTransformToCompositionSection(Transform target, SceneCompositionRoots roots)
        {
            if (target == null)
                return;

            string name = target.name;
            if (name == "PreviewPlatform" || name.StartsWith("Cube_"))
            {
                target.SetParent(roots.Static, true);
                return;
            }

            if (name.StartsWith("TrainingDummy") || name.StartsWith("HollowSoldier") || name == "InterviewArenaPlayer")
            {
                target.SetParent(roots.Actors, true);
                return;
            }

            if (name == "FinishPortal")
            {
                target.SetParent(roots.Dynamic, true);
                return;
            }

            if (name == "PlayerSpawn" || name.StartsWith("SpawnPoint_"))
            {
                target.SetParent(roots.SpawnPoints, true);
                return;
            }

            target.SetParent(roots.Static, true);
        }

        private static void ReparentPlayerBoltInstancesToProjectilesRoot(
            InterviewArenaCombatServices services,
            Transform projectilesRoot)
        {
            if (services == null || projectilesRoot == null)
                return;

            ProjectilePool pool = services.PlayerCrossbowPool;
            if (pool == null)
                return;

            pool.SetProjectilesRoot(projectilesRoot);
            Transform poolHost = pool.transform;
            for (int i = poolHost.childCount - 1; i >= 0; i--)
            {
                Transform bolt = poolHost.GetChild(i);
                if (bolt.name.StartsWith("CrossbowBolt"))
                    bolt.SetParent(projectilesRoot, true);
            }
        }

        private static void AddUiInputModule(GameObject eventSystem)
        {
            Type inputSystemModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null)
                eventSystem.AddComponent(inputSystemModule);
            else
                eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
