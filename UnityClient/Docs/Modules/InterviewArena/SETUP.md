# Interview Arena — Unity setup

One-time steps after pulling the repo:

1. Open the project in Unity.
2. Run **Learning Architect → Interview Arena → Setup Interview Arena Scenes**.
   - Creates/updates `Assets/Content/Modules/InterviewArena/Scenes/InterviewArena.unity`
   - Registers build scenes: `ArchitectureShowcase` (index 0), `InterviewArena` (index 1)
   - Adds `InterviewArenaLaunchDock` to `ArchitectureShowcaseHub.prefab`
3. Optional: **Learning Architect → Localization → Setup Showcase Localization** (adds UI keys for the dock).

## Play flow

- **ArchitectureShowcase** — default entry; 7 modules as today.
- Bottom button **Interview Arena** — loads the game scene.
- **Back to architecture demos** — returns to the showcase scene.

### MVP1 controls (Interview Arena scene)

- **WASD / arrows** — move (camera-relative)
- **Space** — jump (release early for shorter hop)
- **Left Shift** — dash
- **E** (hold near pickup) — collect buff pickup
- **J / LMB** — melee
- **R / RMB** — crossbow
- Reach the **green finish portal** at the far end of the platform

### Buff pickups

Cyan disc — **+35% move speed** for 10s. Orange disc — **+45% melee damage** for 10s. Hold **E** ~0.5s while standing in the pickup trigger.

## Player authoring (no runtime spawn)

MVP1 expects a **prefab + scene instance**, not `CreatePrimitive` at runtime:

1. **Learning Architect → Interview Arena → Create Player Prefab Asset** (optional, creates `Prefabs/InterviewArenaPlayer.prefab`)
2. Drag prefab into `InterviewArena` scene **or** run **Wire Scene Player From Prefab**
3. Assign **Player** on `InterviewArenaBootstrap` (can stay empty until you wire it)

`playerPrefab` on bootstrap is for reference only in MVP1 — not instantiated in play mode.

## Scene composition

See **`SCENE_COMPOSITION.md`** — hierarchy (`Level` / `Gameplay` / `Actors` / `Runtime` / `UI`), UI naming (`Container -`, `Image -`, `Text -`), pools off player prefab.

## Code map

| Piece | Location |
|-------|----------|
| Scene names / paths | `ShowcaseSceneNames` |
| `SceneManager.LoadScene` | `ShowcaseSceneLoader` |
| Hub button | `InterviewArenaLaunchDock` |
| Arena scene root | `InterviewArenaBootstrap` |
| Arena HUD | `InterviewArenaSceneUI` |
| Player prefab | `Prefabs/InterviewArenaPlayer.prefab` (authoring) |
