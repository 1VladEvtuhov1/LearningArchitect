# Client–Server Flow

High-level flow for ExtractionRPG / Interview Arena multiplayer loop. Backend **MVP2/MVP3 in-memory** paths are implemented (auth, lobby, match start/result); PostgreSQL persistence and authoritative multiplayer sync remain open.

## End-to-End Flow

```text
┌─────────────┐     HTTPS/WSS      ┌─────────────┐
│ UnityClient │ ◄────────────────► │   Backend   │
│  (WebGL)    │                    │  (ASP.NET)  │
└─────────────┘                    └──────┬──────┘
                                          │
                                   PostgreSQL
                                   Redis (cache)
```

## Phases

### 1. Login (pin / username)

1. Player opens WebGL build (via `Site/` or direct build URL).
2. Client shows login (username or pin — product TBD).
3. Client `POST /api/auth/login-by-name` (or pin variant).
4. Server returns `sessionToken`, `userId`, profile.
5. Client stores token (memory + optional secure storage pattern for WebGL).

### 2. Lobby

1. Client `GET /api/lobbies` — browse open lobbies.
2. Create or join lobby; WebSocket or polling for member updates.
3. Host configures match parameters; members see ready state.

### 3. Ready-check

1. Each client sets ready via lobby API.
2. Server validates all members ready before match start.
3. Server transitions lobby → match instance.

### 4. Match

1. Server assigns match id; clients load arena scene (already in `UnityClient`).
2. Gameplay authoritative rules TBD (server sim vs client + validation).
3. Client sends intent; server validates scoring/extraction/death events.

### 5. Result persistence

1. Match end: extraction success or death.
2. Server persists run summary (duration, outcome, stats).
3. Client shows result screen; optional leaderboard fetch.

## Current State

| Phase | Unity client | Backend |
|-------|--------------|---------|
| Local arena | ✅ Interview Arena scene, local play | — |
| Login / lobby | ✅ HTTP client + online flow controller | ✅ in-memory auth + lobbies |
| Match start / result submit | ✅ via `IBackendApiClient` | ✅ in-memory match records |
| Result persistence / leaderboard | — | ⏳ PostgreSQL |
| Multiplayer sync (authority) | — | ⏳ TBD |

Workspace map: [[LearningArchitect/docs/Architecture]]. Status detail: [[LearningArchitect/docs/Roadmap]].

## Related Docs

- API shapes: [[LearningArchitect/docs/ApiContract]]
- Arena client design: `UnityClient/docs/Modules/InterviewArena/docs/02_LOBBY_AND_ACCOUNTS.md`
- Detailed API draft: `UnityClient/docs/Modules/InterviewArena/docs/09_SERVER_API_DRAFT.md`
