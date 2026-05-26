# 08. Technical Decisions

This document records major project decisions.

## TD-001: Use WebGL as Main Client Platform

### Decision

The project targets Unity WebGL as the main playable client.

### Reason

WebGL is easy to share and useful for demonstrating browser-specific Unity knowledge.

### Consequences

Pros:

- easy to show from a browser
- strong interview topic
- deployment is visible and practical

Cons:

- networking constraints
- memory constraints
- browser compatibility concerns
- harder debugging than Editor

## TD-002: Use Personal Server for Lobby and Match State

### Decision

The WebGL client connects to a personal server.

### Reason

This demonstrates real client-server architecture and avoids relying on local host-only multiplayer.

### Consequences

Pros:

- better interview value
- server authority
- realistic lobby flow
- clearer security discussion

Cons:

- more infrastructure
- backend must be maintained
- local testing needs server setup

## TD-003: Use Unique Username Login for Prototype

### Decision

Players enter by unique username.

### Reason

It keeps authentication simple and lets development focus on lobby/gameplay.

### Consequences

Pros:

- fast to implement
- easy to test
- low UI complexity

Cons:

- weak identity
- no strong account recovery
- not production-grade security

Future upgrade:

- username + PIN
- password
- OAuth
- email magic link

## TD-004: Server Is Source of Truth

### Decision

Server validates lobby and match-critical actions.

### Reason

The client should not decide match result, score, or important state transitions.

### Consequences

Pros:

- less cheating
- clearer state
- better reconnect model

Cons:

- more synchronization work
- possible latency
- more backend logic

## TD-005: Use ScriptableObject Configs

### Decision

Gameplay tunable values are stored in ScriptableObject assets.

### Reason

Unity inspectors make balancing easier and demonstrate common production workflow.

### Examples

- PlayerConfig
- AbilityConfig
- EnemyConfig
- ArenaConfig

## TD-006: Use Object Pooling for Repeated Runtime Objects

### Decision

Projectiles, effects, and possibly enemies use pools.

### Reason

Pooling reduces allocations and object creation overhead during gameplay.

### Examples

- ProjectilePool
- HitEffectPool
- EnemyPool later

## TD-007: Split Code with Assembly Definitions

### Decision

Use asmdef files after initial prototype stabilizes.

### Reason

Assembly definitions improve compile boundaries and make architecture more explicit.

### Timing

Do not split too early. Add asmdef when folder structure becomes stable.

## TD-008: Keep MVP Small

### Decision

The project starts as one compact arena.

### Reason

The goal is a finished technical prototype, not a large game.

### Consequences

Pros:

- higher chance of finishing
- easier to polish
- easier to explain

Cons:

- less content variety
- fewer player retention systems
