# Roadmap

## Done (Unity client — local)

- [x] Repository workspace layout: `UnityClient/`, `Backend/` scaffold, root `Docs/`, `Site/`
- [x] LearningArchitect showcase hub (7 modules, 16 variants)
- [x] Interview Arena separate scene: locomotion, combat, AI FSM, buffs, scene composition
- [x] WebGL-oriented defaults (layers, pooling, Brotli notes)
- [x] EditMode tests for core Arena logic
- [x] Portfolio shell in `Site/`

## Next — Unity client hygiene

- [ ] Re-open `UnityClient/` in Unity Hub; let Editor regenerate `.sln` / `.csproj` under `UnityClient/`
- [ ] Run **Learning Architect → Interview Arena → Setup Complete**
- [ ] Run **Validate Scene Composition** (0 errors)
- [ ] Play Mode smoke: showcase + Arena + buff HUD/VFX
- [ ] WebGL build → `Site/webgl/` → `Site/prepare-webgl-site.ps1`

## Phase — Backend scaffold (no business logic yet)

- [ ] Create ASP.NET Core solution under `Backend/src/`
- [ ] Health check endpoint only
- [ ] Docker compose integration test (Postgres reachable)
- [ ] CI build backend + run unit tests

## Phase — Auth + lobby (MVP2)

- [ ] Login by username (or pin)
- [ ] Session token issue/validate
- [ ] Lobby CRUD + ready-check
- [ ] Unity client mock → real HTTP client swap

## Phase — Match + results (MVP3)

- [ ] Match lifecycle API
- [ ] Result persistence (PostgreSQL)
- [ ] Run history + leaderboard read API
- [ ] Multiplayer sync strategy (TBD: dedicated server vs relay)

## Backlog

- Hub experience controller (single-scene mode switch) — see `UnityClient/Docs/Modules/InterviewArena/INTEGRATION_ASSESSMENT.md`
- Localization for Arena UI via `ShowcaseContent`
- Backend stack final choice: `UnityClient/Docs/Modules/InterviewArena/docs/11_BACKEND_STACK_OPTIONS.md`

## Where to Track Active Work

- Unity showcase threads: `UnityClient/Docs/OpenThreads.md`
- Arena backlog milestones: `UnityClient/Docs/Modules/InterviewArena/docs/04_BACKLOG.md`
