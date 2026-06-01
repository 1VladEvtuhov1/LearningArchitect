# 01. Architecture

## High-Level Architecture

```text
Unity WebGL Client
        |
        | HTTPS / WebSocket
        v
Personal Server
        |
        ├── Auth Service
        ├── Lobby Service
        ├── Match Service
        └── Persistence Layer
```

## Client Responsibility

The Unity client is responsible for:

- rendering the game
- reading player input
- displaying UI
- sending player intent to the server
- predicting or smoothing local movement where needed
- reacting to lobby and match state updates
- loading scenes
- playing VFX/SFX

The client should not be trusted for final match results.

## Server Responsibility

The server is responsible for:

- username uniqueness
- session issuing
- lobby ownership
- lobby state validation
- ready checks
- match creation
- authoritative scoring
- basic reconnect state
- closing inactive lobbies

## Unity Project Layers

```text
Core
├── Bootstrap
├── GameStateMachine
├── SceneLoader
└── ServiceRegistration

Auth
├── IAuthService
├── AuthServiceHttp
├── UserProfile
└── SessionStorage

Lobby
├── ILobbyService
├── LobbyServiceHttp
├── LobbySocketClient
├── LobbyController
├── LobbyView
└── LobbyModels

Player
├── PlayerInputReader
├── PlayerMotor
├── GroundDetector
├── PlayerCombat
└── PlayerAnimationController

Physics
├── PhysicsQueryService
├── PushableObject
├── MovingPlatform
├── DamageTrigger
└── PhysicsLayers

Combat
├── IDamageable
├── Health
├── Projectile
├── ProjectilePool
└── AbilitySystem

AI
├── EnemyBrain
├── EnemySensor
├── EnemyStateMachine
└── EnemyMovement

Multiplayer
├── NetworkPlayer
├── MatchNetworkState
├── NetworkTransformAdapter
└── NetworkProjectileSpawner

UI
├── LoginView
├── LobbyBrowserView
├── LobbyRoomView
├── MatchHudView
└── ResultView

Infrastructure
├── HttpClientWrapper
├── WebSocketClient
├── Logger
└── Result
```

## Scene Flow

```text
BootScene
    |
    v
LoginScene
    |
    v
LobbyScene
    |
    v
GameScene
    |
    v
ResultScene
```

For a smaller implementation, `LoginScene` and `LobbyScene` can be one scene with separate UI panels.

## Game State Machine

```csharp
public enum GameState
{
    Boot,
    Login,
    LobbyBrowser,
    LobbyRoom,
    LoadingMatch,
    InMatch,
    MatchResult,
    Error
}
```

The state machine should control major transitions:

- boot to login
- login success to lobby browser
- lobby joined to lobby room
- match starting to game scene
- match finished to result screen

## Dependency Direction

Preferred dependency direction:

```text
UI -> Controllers -> Services -> Infrastructure
Gameplay -> Interfaces/Data
Infrastructure -> External APIs
```

UI should not directly know about low-level networking details.

## Assembly Definition Plan

Suggested asmdef split:

```text
InterviewArena.Core
InterviewArena.Auth
InterviewArena.Lobby
InterviewArena.Gameplay
InterviewArena.Multiplayer
InterviewArena.UI
InterviewArena.Infrastructure
```

Dependency idea:

```text
UI depends on Core, Auth, Lobby, Gameplay
Lobby depends on Core, Infrastructure
Auth depends on Core, Infrastructure
Gameplay depends on Core
Multiplayer depends on Core, Gameplay
Infrastructure depends on Core
```

## Naming Rules

Use clear names:

- `PlayerMotor`, not `PlayerMovementScript`
- `LobbyController`, not `LobbyManager2`
- `IAuthService`, not `AuthStuff`
- `ProjectilePool`, not `BulletSpawnerThing`

## Important Architecture Trade-Offs

### Server Authority

Server authority gives better validation and clearer ownership. It costs more development time and requires careful synchronization.

### Username Login

Username login is fast for a pet project. It should be documented as a prototype-level identity system.

### WebGL Client

WebGL gives easy sharing through a browser. It has stricter networking and memory constraints compared to desktop builds.

### Small Scene Count

Fewer scenes are easier to finish. More scenes make state transitions clearer. Start with fewer scenes and split later if needed.
