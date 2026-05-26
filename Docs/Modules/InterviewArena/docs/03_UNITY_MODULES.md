# 03. Unity Modules

## Core

Responsible for app-level flow.

Classes:

```text
Bootstrapper
GameStateMachine
SceneLoader
ServiceRegistry
GameConfig
```

Topics:

- initialization order
- service interfaces
- scene transitions
- dependency direction
- async loading

## Auth

Responsible for login and current user session.

Classes:

```text
IAuthService
AuthServiceHttp
UserProfile
SessionData
SessionStorage
LoginController
LoginView
```

Topics:

- async/await
- DTOs
- HTTP requests
- token storage
- input validation
- error handling

## Lobby

Responsible for pre-match preparation.

Classes:

```text
ILobbyService
LobbyServiceHttp
LobbySocketClient
LobbyController
LobbyBrowserView
LobbyRoomView
LobbyPlayerView
LobbyModels
```

Topics:

- WebSocket events
- observer pattern
- UI update flow
- DTO mapping
- server validation
- host authority

## Player

Responsible for player control.

Classes:

```text
PlayerInputReader
PlayerMotor
GroundDetector
PlayerCombat
PlayerCameraTarget
PlayerAnimationController
```

Topics:

- input in Update
- physics in FixedUpdate
- Rigidbody velocity
- jump
- dash
- slopes
- ground check
- camera-relative movement

## Physics

Responsible for physics interactions.

Classes:

```text
PhysicsQueryService
PushableObject
MovingPlatform
DamageTrigger
PhysicsLayers
```

Topics:

- Rigidbody
- Collider
- Trigger
- Collision
- Raycast
- SphereCast
- LayerMask
- NonAlloc queries
- ForceMode

## Combat

Responsible for abilities and damage.

Classes:

```text
IDamageable
Health
DamageInfo
Projectile
ProjectilePool
AbilityConfig
AbilitySystem
```

Topics:

- interfaces
- events
- object pooling
- ScriptableObject configs
- server-validated actions
- projectile lifetime
- collision callbacks

## AI

Responsible for enemy behavior.

Classes:

```text
EnemyBrain
EnemySensor
EnemyStateMachine
EnemyMovement
EnemyAttack
EnemyConfig
```

Topics:

- state machine
- vector math
- dot product visibility
- distance checks
- physics queries
- target selection

## Multiplayer

Responsible for network representation.

Classes:

```text
NetworkPlayer
NetworkPlayerInput
NetworkTransformAdapter
MatchNetworkState
NetworkProjectileSpawner
NetworkScoreSystem
```

Topics:

- server authority
- ownership
- synchronization
- RPC/event flow
- spawn/despawn
- interpolation
- reconciliation later

## UI

Responsible for user-facing screens.

Classes:

```text
LoginView
LobbyBrowserView
LobbyRoomView
MatchHudView
ResultView
ErrorPopupView
LoadingView
```

Topics:

- View/Controller separation
- UnityEvents
- Action events
- async button states
- loading indicators
- error messages

## Data

Responsible for configs.

Assets:

```text
PlayerConfig
AbilityConfig
EnemyConfig
ArenaConfig
NetworkConfig
```

Topics:

- ScriptableObject
- serialization
- inspector tooling
- balancing
- runtime vs editor data

## Infrastructure

Responsible for low-level external communication.

Classes:

```text
HttpClientWrapper
WebSocketClient
JsonSerializerAdapter
Logger
Result
Clock
```

Topics:

- adapter pattern
- testability
- error handling
- serialization
- platform-specific implementation
