# 04. Backlog

## Milestone 0: Repository Setup

- [ ] Create Unity project
- [ ] Create GitHub repository
- [ ] Add Unity `.gitignore`
- [ ] Add README
- [ ] Add docs folder
- [ ] Add Cursor rules
- [ ] Create base folder structure
- [ ] Add first commit

Definition of done:

- Project opens in Unity
- Repo has clear documentation
- Cursor can read project context

## Milestone 1: Local Player Prototype

- [ ] Create test arena
- [ ] Add player capsule/model
- [ ] Implement `PlayerInputReader`
- [ ] Implement `PlayerMotor`
- [ ] Implement camera follow
- [ ] Implement jump
- [ ] Implement dash
- [ ] Implement ground check
- [ ] Add physics cubes
- [ ] Add slopes

Interview topics:

- Update vs FixedUpdate
- Rigidbody movement
- ground detection
- vector direction
- physics interactions

## Milestone 2: Physics and Combat

- [ ] Add projectile prefab
- [ ] Add `ProjectilePool`
- [ ] Add `IDamageable`
- [ ] Add `Health`
- [ ] Add damage trigger
- [ ] Add knockback
- [ ] Add `Physics.OverlapSphereNonAlloc`
- [ ] Add object lifetime handling

Interview topics:

- pooling
- GC allocations
- collisions/triggers
- NonAlloc APIs
- interfaces

## Milestone 3: AI

- [ ] Add enemy prefab
- [ ] Add patrol state
- [ ] Add chase state
- [ ] Add attack state
- [ ] Add visibility check using dot product
- [ ] Add side check using cross product
- [ ] Add enemy config ScriptableObject

Interview topics:

- state machine
- vector math
- AI sensing
- ScriptableObject configs

## Milestone 4: Backend Prototype

- [ ] Choose backend stack
- [ ] Create auth endpoint
- [ ] Create login by username
- [ ] Create session token
- [ ] Create lobby endpoints
- [ ] Create in-memory lobby store
- [ ] Add WebSocket lobby events
- [ ] Add basic server logging

Suggested backend stacks:

- ASP.NET Core
- Node.js + TypeScript
- Go

Pick one and keep it simple.

## Milestone 5: Unity Auth and Lobby Client

- [ ] Add login scene/panel
- [ ] Implement `IAuthService`
- [ ] Implement session storage
- [ ] Add lobby browser
- [ ] Implement lobby create/join
- [ ] Implement lobby room view
- [ ] Implement ready state
- [ ] Implement ability selection
- [ ] Subscribe to lobby events

Interview topics:

- async/await
- DTOs
- HTTP
- WebSocket
- UI state
- service interfaces

## Milestone 6: Multiplayer Match

- [ ] Create match room on server
- [ ] Send `Match.Starting`
- [ ] Load game scene
- [ ] Spawn players from lobby data
- [ ] Sync basic player transforms
- [ ] Add server-owned score
- [ ] Add result screen

Interview topics:

- authority
- ownership
- synchronization
- scene loading
- match lifecycle

## Milestone 7: WebGL Build

- [ ] Configure WebGL build
- [ ] Add loading screen
- [ ] Test in browser
- [ ] Test server connection
- [ ] Check compression settings
- [ ] Check memory usage
- [ ] Record known WebGL limitations

Interview topics:

- browser platform constraints
- build size
- memory
- WebSocket compatibility

## Milestone 8: Architecture Polish

- [ ] Add asmdef files
- [ ] Split namespaces
- [ ] Add ScriptableObject configs
- [ ] Add Addressables for selected assets
- [ ] Add profiler notes
- [ ] Add architecture diagram
- [ ] Add technical decisions doc
- [ ] Add short gameplay video/gif later

Interview topics:

- modular architecture
- assembly references
- asset loading
- profiling
- documentation

## Priority Labels

Use GitHub labels:

```text
type:feature
type:bug
type:refactor
type:docs
type:tech-debt
area:player
area:physics
area:lobby
area:backend
area:webgl
area:multiplayer
area:ui
area:ai
priority:high
priority:medium
priority:low
```
