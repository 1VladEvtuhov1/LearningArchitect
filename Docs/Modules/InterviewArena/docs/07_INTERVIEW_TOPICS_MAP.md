# 07. Interview Topics Map

This file maps project features to interview questions.

## Player Movement

Feature:

- Rigidbody-based movement
- jump
- dash
- slope handling
- ground detection

Questions covered:

- Why read input in `Update`?
- Why move physics objects in `FixedUpdate`?
- Difference between `Rigidbody` and `CharacterController`?
- What is `ForceMode.Impulse`?
- Why can direct `transform.position` changes be problematic for physics?

## Physics

Feature:

- Raycast/SphereCast ground check
- triggers
- collision callbacks
- pushable objects
- NonAlloc overlap query

Questions covered:

- Difference between `Collider` and `Rigidbody`?
- Difference between trigger and collision?
- What is a `LayerMask`?
- Why use `Physics.NonAlloc` methods?
- How can physics cause GC allocations?

## Vector Math

Feature:

- AI visibility using dot product
- side detection using cross product
- projectile reflection
- slope movement using projection

Questions covered:

- What does `Vector3.Dot` mean?
- What does `Vector3.Cross` mean?
- Why normalize vectors?
- Difference between `magnitude` and `sqrMagnitude`?
- How do you move relative to camera direction?

## C# Core

Feature:

- event-based health/UI updates
- interfaces for damage
- generic object pool
- coroutines for delayed respawn
- async backend requests

Questions covered:

- Difference between `Action` and `Func`?
- What is a closure?
- What is an interface used for?
- What are generics?
- How does coroutine work internally?
- Difference between coroutine and thread?
- What is async/await used for?

## Multiplayer

Feature:

- username session
- lobby
- ready state
- host starts match
- server-owned scoring

Questions covered:

- What is server authority?
- What is ownership?
- What should the client validate?
- What should the server validate?
- How do you synchronize state?
- What happens when a player disconnects?
- How do you reduce cheating?

## WebGL

Feature:

- browser build
- server connection
- WebSocket-compatible networking
- loading screen
- memory notes

Questions covered:

- What are WebGL limitations?
- Why are normal sockets problematic in WebGL?
- Why care about build size?
- Why test in browser separately from Editor?
- What APIs can break in WebGL?

## Architecture

Feature:

- modules
- asmdef
- interfaces
- ScriptableObject configs
- docs

Questions covered:

- Why split code into assemblies?
- Why use ScriptableObjects?
- Why avoid god managers?
- How do you structure a Unity project?
- How do you keep code testable?
- How do you document trade-offs?

## Optimization

Feature:

- object pooling
- cached references
- NonAlloc queries
- profiler pass

Questions covered:

- What causes GC spikes?
- Why use pooling?
- What should you look at in Profiler?
- How can UI cause performance issues?
- How can physics queries be optimized?
