# Interview Arena — Code Architecture

Scene authoring rules: **`SCENE_COMPOSITION.md`** (hierarchy, naming, runtime roots, UI).

This document is the source of truth for **logic, layering, and optimization choices**.

## Principles

1. **Prefab + scene references** — no runtime player spawn (`InterviewArenaRuntimeContext.Player`). **Pooled combat objects live in the scene** (`ArenaCombatServices`), not inside actor prefabs.
2. **Input in Update, physics in FixedUpdate** — jump/dash buffered (`PlayerInputReader.Consume*`).
3. **Zero-allocation queries** — `PhysicsQueryService` uses `SphereCastNonAlloc`.
4. **Data-driven tuning** — `PlayerConfig` ScriptableObject only.
5. **Explicit game flow** — `GameStateMachine` for Login → Lobby → Match (wired incrementally).
6. **WebGL-friendly defaults** — NonAlloc queries, pooled projectiles, Brotli compression, dedicated physics layers (layers 6–9).

## Layer Map

```text
Assets/Content/Modules/InterviewArena/Scripts/
├── Core/
│   ├── GameState / GameStateMachine
│   ├── InterviewArenaRuntimeContext   ← scene composition root
│   ├── InterviewArenaCombatServices   ← scene pools + hit feedback
│   ├── InterviewArenaRunSession       ← finish / result signals
│   ├── ISceneNavigation / SceneNavigationGateway
│   └── Result
├── Physics/
│   ├── PhysicsQueryService            ← NonAlloc casts
│   └── InterviewArenaPhysicsLayers
├── Player/
│   ├── PlayerConfig
│   ├── PlayerInputReader
│   ├── GroundDetector
│   ├── PlayerMotor
│   ├── PlayerLocomotionMath           ← pure helpers + tests
│   ├── InterviewArenaCameraFollow
│   └── FinishPortal
├── Combat/
│   ├── DamageInfo / IDamageable / Health / CombatRules
│   ├── InvulnerabilityTimer / KnockbackReceiver
│   ├── MeleeWeaponConfig / CrossbowWeaponConfig
│   ├── CombatStrikeUtility / MeleeStrikeController
│   ├── CrossbowWeaponController
│   ├── Projectile / ProjectilePool
│   ├── PlayerCombat
│   └── CombatTrainingDummy
├── AI/
│   ├── EnemyConfig / EnemyState / EnemyFsmLogic
│   ├── EnemySensor / EnemyMotor / EnemyMeleeAttack
│   └── EnemyBrain                     ← Patrol → Chase → Attack
├── UI/
│   └── InterviewArenaSceneUI
└── InterviewArenaBootstrap            ← thin Awake wiring
```

## Scene Hierarchy (authoring)

```text
InterviewArenaBootstrap          ← InterviewArenaRuntimeContext + Bootstrap
├── ArenaRoot                    ← geometry, spawn, player instance
│   ├── PreviewPlatform
│   ├── obstacles / dummies / enemies
│   ├── PlayerSpawn
│   └── InterviewArenaPlayer     ← prefab instance (locomotion + combat components only)
├── ArenaCombatServices          ← InterviewArenaCombatServices (scene-owned)
│   ├── PlayerCrossbowBoltPool   ← ProjectilePool (pooled bolts)
│   └── CombatHitFeedback
└── InterviewArenaCanvas / Main Camera / EventSystem
```

Player prefab: **no** `CrossbowBolt_*` children. `InterviewArenaRuntimeContext.TryWirePlayerAndCamera` binds the scene pool to `CrossbowWeaponController`.

## Player Locomotion Pipeline

```text
Update:
  PlayerInputReader → move axis + buffer jump/dash
  PlayerMotor       → coyote timer, dash cooldown, camera basis cache

FixedUpdate:
  GroundDetector    → SphereCastNonAlloc, walkable normal
  PlayerMotor       → consume jump/dash, planar velocity on slope, arena clamp
```

### Mechanics (MVP1)

| Mechanic | Implementation |
|----------|----------------|
| Move | Camera-relative planar velocity, separate ground/air acceleration |
| Slope | `Vector3.ProjectOnPlane` on walkable normals |
| Jump | Impulse + coyote time |
| Dash | Timed velocity override + cooldown |
| Bounds | `InterviewArenaPlatformLayout.ClampToSurface` (min **body center** Y from capsule + platform top) |
| Spawn height | `ResolveSpawnPosition` uses `Collider.bounds.max.y` (supports resized/trigger floor) |

## Combat (dark fantasy MVP2)

| Mechanic | Input | Implementation |
|----------|-------|----------------|
| Melee strike | `J` / LMB | `MeleeStrikeController` overlap at view pivot forward offset |
| Crossbow bolt | `R` / RMB | Scene `ArenaCombatServices/PlayerCrossbowBoltPool` → `Projectile` sphere cast |
| Practice targets | — | `CombatTrainingDummy` + `Health` (Enemy team), respawn after death |

Friendly fire is blocked via `CombatRules.CanDamage`. Queries stay NonAlloc (`PhysicsQueryService`).

## Combat polish (MVP3)

| Mechanic | Implementation |
|----------|----------------|
| i-frames | `InvulnerabilityTimer` inside `Health` after each confirmed hit |
| Dash i-frames | Middle 85% of `dashDuration` via `DashIframeRules` + `PlayerDashIframeGuard` |
| Enemy hitstun | `EnemyHealthHitstun` cancels melee windup; `EnemyBrain` does not start attacks while stunned |
| Knockback | `DamageInfo.KnockbackImpulse` + `KnockbackReceiver` on `Rigidbody` |
| FSM enemies | `EnemyBrain` — Patrol / Chase / Attack / **Ranged** (enemy crossbow) |
| Enemy respawn | `EnemyRespawnController` — disable on death, restore at spawn pose |
| Player death (local) | `PlayerDeathController` — short plant, restore at spawn; online match stays with `MatchReporter` |
| HP UI | `PlayerHealthHud` — always-visible screen bar (`CurrentHealth` / `MaxHealth`) |
| i-frames UI | `PlayerIframeHud` + `InvulnerabilityWorldIndicator` on player |

## Extension Hooks (next milestones)

| Area | Planned types |
|------|----------------|
| Auth | `IAuthService`, DTOs, `SessionStorage` |
| Lobby | `ILobbyService`, `LobbyController`, views |
| Combat polish | ability SO selection in lobby, enemy ranged archetypes |
| Multiplayer | `NetworkPlayer`, server-validated state |

## Testing

- `InterviewArenaPlatformLayoutTests` — arena bounds
- `PlayerLocomotionMathTests` — slope projection / walkable angle
- `CombatRulesTests` — team damage matrix
- `EnemyFsmLogicTests` / `InvulnerabilityTimerTests`
- `DashIframeRulesTests` / `EnemyMeleeAttackTests` / `EnemyHealthHitstunTests`

EditMode tests avoid requiring a authored scene.

## WebGL checklist

| Item | Status |
|------|--------|
| `PhysicsQueryService` NonAlloc + closest ground hit | Implemented |
| Melee knockback through `CombatHitResolver` | Implemented |
| Projectile pool (no per-shot `Instantiate`) | Implemented |
| Physics layers `InterviewArena_*` (6–9) + masks on SO | Implemented via editor setup |
| Build compression Brotli (`webGLCompressionFormat: 2`) | ProjectSettings |
| Player HP HUD wired on `TryWirePlayerAndCamera` | Implemented |
| Player i-frame HUD wired on `TryWirePlayerAndCamera` | Implemented |

After clone: **Learning Architect → Interview Arena → Setup Complete**.

## Interview Topics Covered

- Update vs FixedUpdate separation
- Input buffering before physics
- Rigidbody velocity control (not Transform teleport)
- Physics.NonAlloc queries
- ScriptableObject config
- Composition root without service locator soup
