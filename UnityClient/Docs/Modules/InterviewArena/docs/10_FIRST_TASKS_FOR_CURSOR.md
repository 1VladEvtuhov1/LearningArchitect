# 10. First Tasks for Cursor

Use this file as the first practical task list inside Cursor.

## Task 1: Create Unity Folder Structure

Create folders:

```text
Assets/InterviewArena/
├── Art
├── Audio
├── Materials
├── Prefabs
├── Scenes
├── ScriptableObjects
└── Scripts
    ├── Core
    ├── Auth
    ├── Lobby
    ├── Multiplayer
    ├── Player
    ├── Physics
    ├── Combat
    ├── AI
    ├── UI
    └── Infrastructure
```

## Task 2: Create Core Namespaces

Create placeholder C# files:

```text
Assets/InterviewArena/Scripts/Core/GameState.cs
Assets/InterviewArena/Scripts/Core/GameStateMachine.cs
Assets/InterviewArena/Scripts/Core/SceneLoader.cs
Assets/InterviewArena/Scripts/Core/Result.cs
```

## Task 3: Create Player Prototype Classes

Create:

```text
PlayerInputReader
PlayerMotor
GroundDetector
PlayerConfig
```

Keep them simple. The first goal is movement in a test scene.

## Task 4: Create Login/Lobby DTOs

Create:

```text
LoginByNameRequest
LoginByNameResponse
UserProfileDto
LobbyInfoDto
LobbyPlayerDto
```

These can be plain serializable classes.

## Task 5: Create First Scene

Create:

```text
BootScene
PrototypeArenaScene
```

At first, `BootScene` can load `PrototypeArenaScene`.

## Task 6: Add README Progress Section

Add a short progress checklist to README:

```md
## Current Progress

- [ ] Unity project created
- [ ] Player movement prototype
- [ ] Backend auth prototype
- [ ] Lobby prototype
- [ ] WebGL build
```
