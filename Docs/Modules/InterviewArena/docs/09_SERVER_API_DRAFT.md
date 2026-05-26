# 09. Server API Draft

This document is a first draft. It can change after backend stack selection.

## Base Concepts

The backend manages:

- users
- sessions
- lobbies
- lobby members
- matches

## Auth

### POST /api/auth/login-by-name

Request:

```json
{
  "username": "AsdaRunner"
}
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

Errors:

```json
{
  "errorCode": "USERNAME_INVALID",
  "message": "Username must be 3-16 characters and contain only letters, numbers or underscore."
}
```

## Profile

### GET /api/profile/me

Headers:

```http
Authorization: Bearer token_value
```

Response:

```json
{
  "userId": "u_123",
  "username": "AsdaRunner",
  "createdAt": "2026-05-24T12:00:00Z",
  "lastSeenAt": "2026-05-24T12:10:00Z"
}
```

## Lobbies

### GET /api/lobbies

Response:

```json
{
  "items": [
    {
      "lobbyId": "lobby_42",
      "name": "Forest Run",
      "playerCount": 2,
      "maxPlayers": 4,
      "state": "Preparing"
    }
  ]
}
```

### POST /api/lobbies

Request:

```json
{
  "name": "Forest Run",
  "maxPlayers": 4
}
```

Response:

```json
{
  "lobbyId": "lobby_42",
  "name": "Forest Run",
  "hostUserId": "u_123",
  "maxPlayers": 4,
  "state": "Preparing",
  "players": []
}
```

### POST /api/lobbies/{lobbyId}/join

Response:

```json
{
  "lobbyId": "lobby_42",
  "state": "Preparing",
  "players": []
}
```

### POST /api/lobbies/{lobbyId}/ready

Request:

```json
{
  "isReady": true
}
```

### POST /api/lobbies/{lobbyId}/select-ability

Request:

```json
{
  "abilityId": "dash"
}
```

### POST /api/lobbies/{lobbyId}/start

Host-only.

Response:

```json
{
  "matchId": "match_100",
  "connectUrl": "wss://server.example.com/matches/match_100"
}
```

## WebSocket Lobby Events

### Client -> Server

```json
{
  "type": "Lobby.Subscribe",
  "lobbyId": "lobby_42"
}
```

```json
{
  "type": "Lobby.SetReady",
  "isReady": true
}
```

```json
{
  "type": "Lobby.SelectAbility",
  "abilityId": "dash"
}
```

### Server -> Client

```json
{
  "type": "Lobby.Updated",
  "lobby": {
    "lobbyId": "lobby_42",
    "state": "Preparing",
    "players": []
  }
}
```

```json
{
  "type": "Match.Starting",
  "matchId": "match_100",
  "connectUrl": "wss://server.example.com/matches/match_100",
  "startsInSeconds": 5
}
```

## Error Format

Use consistent errors:

```json
{
  "errorCode": "LOBBY_FULL",
  "message": "Lobby is full."
}
```

Common error codes:

```text
UNAUTHORIZED
USERNAME_INVALID
USERNAME_TAKEN
SESSION_EXPIRED
LOBBY_NOT_FOUND
LOBBY_FULL
NOT_LOBBY_MEMBER
HOST_ONLY
PLAYER_NOT_READY
INVALID_ABILITY
MATCH_ALREADY_STARTED
```
