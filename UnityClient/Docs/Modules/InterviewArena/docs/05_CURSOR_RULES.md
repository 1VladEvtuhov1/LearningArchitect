# 05. Cursor Rules

This document describes how Cursor should help with the project.

A machine-readable rule file is also available in:

```text
.cursor/rules/interview-arena.mdc
```

## General Rules

When generating code for this project:

1. Prefer small focused classes.
2. Avoid god objects and giant managers.
3. Use clear namespaces under `InterviewArena`.
4. Keep MonoBehaviours thin when possible.
5. Put game data in ScriptableObjects.
6. Put external communication behind interfaces.
7. Avoid hidden dependencies.
8. Avoid unnecessary allocations in gameplay loops.
9. Avoid using `FindObjectOfType` in runtime gameplay code.
10. Avoid hardcoded magic values in gameplay systems.
11. Add comments only when they explain intent or non-obvious trade-offs.

## Code Style

Use this style:

```csharp
namespace InterviewArena.Player
{
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private PlayerConfig config;

        private Vector2 _moveInput;

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }
    }
}
```

## Naming

Use:

- `PlayerMotor`
- `GroundDetector`
- `LobbyController`
- `LobbyRoomView`
- `IAuthService`
- `AuthServiceHttp`
- `ProjectilePool`

Avoid:

- `Manager` unless it truly coordinates a high-level system
- `Script` suffix
- vague names like `Handler`, `Thing`, `Stuff`, `Controller2`

## Unity Lifecycle

Use lifecycle methods intentionally:

- `Awake`: local component references
- `OnEnable`: subscribe to events
- `Start`: initialization that needs other objects ready
- `Update`: input and visual updates
- `FixedUpdate`: physics movement
- `OnDisable`: unsubscribe from events

## Async Rules

For HTTP/backend calls:

- use `async Task`
- do not use `async void` except Unity event handlers
- handle errors explicitly
- disable UI buttons while request is pending
- surface user-friendly error messages

## Physics Rules

For gameplay physics:

- cache component references
- use `LayerMask`
- use `NonAlloc` queries in repeated checks
- avoid allocations in `Update`/`FixedUpdate`
- keep input reading separate from physics movement

## Multiplayer Rules

For networked gameplay:

- client sends intent
- server validates important actions
- server owns match result
- lobby state comes from server
- do not trust client score
- design for reconnect later

## Documentation Rule

When adding a major system, update at least one of:

- `README.md`
- `docs/01_ARCHITECTURE.md`
- `docs/04_BACKLOG.md`
- `docs/07_INTERVIEW_TOPICS_MAP.md`
- `docs/08_TECHNICAL_DECISIONS.md`
