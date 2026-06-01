# API Contract (Draft)

Product-level API summary. **Not implemented** in `Backend/` yet.

Canonical detailed draft: `UnityClient/Docs/Modules/InterviewArena/docs/09_SERVER_API_DRAFT.md`.

Base URL (local): `http://localhost:5000` (TBD when API project is created).

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
- JWT vs opaque session tokens — decide in first backend milestone.
- Do not implement endpoints in the migration PR; scaffold only under `Backend/`.
