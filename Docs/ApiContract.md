# API Contract (Draft)

Product-level API summary. **Auth MVP** in `Backend/src/ExtractionRpg.Api/`.

Canonical detailed draft: `UnityClient/Docs/Modules/InterviewArena/docs/09_SERVER_API_DRAFT.md`.

Base URL (local): `http://localhost:5000`

Health: `GET /health`, `GET /health/ready` (Postgres when `docker compose` is up).

**Implemented:** auth, profile, lobby list/create/join/ready/**start** (in-memory match record).

## Auth

### `POST /api/auth/login-by-name`

Request:

```json
{ "username": "AsdaRunner" }
```

Response:

```json
{
  "userId": "u_123",
  "username": "AsdaRunner",
  "sessionToken": "token_value",
  "expiresAt": "2026-05-25T12:00:00Z"
}
```

## Profile

### `GET /api/profile/me`

Header: `Authorization: Bearer {sessionToken}`

## Lobbies

### `GET /api/lobbies`

List open lobbies.

### `POST /api/lobbies`

Create lobby (authenticated).

### `POST /api/lobbies/{lobbyId}/join`

Join lobby.

### `POST /api/lobbies/{lobbyId}/ready`

Set ready flag.

### `POST /api/lobbies/{lobbyId}/start`

Host-only. All members must be ready. Response:

```json
{
  "matchId": "match_100",
  "connectUrl": "wss://localhost:5001/matches/match_100"
}
```

Errors: `HOST_ONLY`, `PLAYER_NOT_READY`, `MATCH_ALREADY_STARTED`.

## Match

### `POST /api/matches/start`

Host-triggered start when ready-check passes (exact shape TBD).

### `POST /api/matches/{matchId}/result`

Persist extraction/death outcome and stats.

## Errors

Structured error body:

```json
{
  "errorCode": "USERNAME_INVALID",
  "message": "Human-readable message."
}
```

## WebSocket (planned)

Lobby presence and match events — see `UnityClient/Docs/Modules/InterviewArena/docs/02_LOBBY_AND_ACCOUNTS.md`.

## Implementation Notes

- Pin-code login may replace or supplement username login; update this doc when chosen.
- Session tokens are **opaque** (server-side), not JWT — see `Docs/Decisions.md` D-WS-005.
- Users/sessions are **in-memory** until PostgreSQL persistence lands (MVP3+).
