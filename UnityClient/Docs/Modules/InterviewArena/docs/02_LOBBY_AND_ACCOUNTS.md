# 02. Lobby and Accounts

## Goal

Implement a lightweight account and lobby system that is easy to test, simple to explain, and useful for multiplayer interview topics.

## Account Model

The prototype uses a unique username as the user identity.

Example:

```json
{
  "userId": "u_123",
  "username": "AsdaRunner",
  "createdAt": "2026-05-24T12:00:00Z",
  "lastSeenAt": "2026-05-24T12:10:00Z"
}
```

## Login Flow

```text
1. Player opens the WebGL build.
2. Login screen asks for username.
3. Client sends username to the server.
4. Server validates format.
5. Server checks uniqueness or existing user.
6. Server returns user profile and session token.
7. Client stores session token.
8. Client opens lobby browser.
```

## Username Rules

Suggested rules:

- length: 3-16 characters
- allowed: letters, numbers, underscore
- case-insensitive uniqueness
- displayed with original casing
- no empty names
- no whitespace-only names

Example validation:

```text
Valid:
- AsdaRunner
- VectorMage_01
- Player777

Invalid:
- a
- player with spaces
- !!!!
```

## Session Token

The server should return a temporary token.

Example:

```json
{
  "userId": "u_123",
  "username": "AsdaRunner",
  "sessionToken": "temporary_session_token"
}
```

The client stores it locally.

Prototype storage options:

- Unity `PlayerPrefs`
- encrypted local storage later
- browser local storage through JS plugin later

## Security Note

Username-only login is suitable for this prototype.

Known limitations:

- user identity is weak
- another person could try to claim a name
- account recovery is not solved
- token security is basic

Future improvements:

- username + PIN
- password login
- email magic link
- OAuth
- token refresh
- secure password hashing
- rate limiting

## Lobby Model

```json
{
  "lobbyId": "lobby_42",
  "name": "Forest Run",
  "hostUserId": "u_123",
  "maxPlayers": 4,
  "state": "Preparing",
  "players": [
    {
      "userId": "u_123",
      "username": "AsdaRunner",
      "isHost": true,
      "isReady": false,
      "selectedAbilityId": "dash",
      "colorIndex": 0
    }
  ]
}
```

## Lobby States

```csharp
public enum LobbyState
{
    WaitingForPlayers,
    Preparing,
    AllReady,
    StartingMatch,
    InGame,
    Closed
}
```

## Lobby Player States

```csharp
public enum LobbyPlayerState
{
    Connected,
    Selecting,
    Ready,
    Loading,
    InGame,
    Disconnected
}
```

## Lobby Actions

Players can:

- create lobby
- join lobby
- leave lobby
- select ability
- select color
- toggle ready
- send chat/emote later

Host can:

- start match
- change map later
- change mode later
- close lobby later

## Server Validation Rules

The server validates:

- username exists
- session token is valid
- lobby exists
- lobby is joinable
- lobby is not full
- player is a member of the lobby
- selected ability exists
- host-only actions are called by the host
- match can start only when ready rules pass

## REST API Draft

```http
POST /api/auth/login-by-name
GET  /api/profile/me

GET  /api/lobbies
POST /api/lobbies
POST /api/lobbies/{lobbyId}/join
POST /api/lobbies/{lobbyId}/leave
POST /api/lobbies/{lobbyId}/ready
POST /api/lobbies/{lobbyId}/select-ability
POST /api/lobbies/{lobbyId}/select-color
POST /api/lobbies/{lobbyId}/start
```

## WebSocket Events

Client to server:

```text
Lobby.Subscribe
Lobby.SetReady
Lobby.SelectAbility
Lobby.SelectColor
Lobby.StartMatch
```

Server to clients:

```text
Lobby.Created
Lobby.Updated
Lobby.PlayerJoined
Lobby.PlayerLeft
Lobby.PlayerReadyChanged
Lobby.PlayerAbilityChanged
Lobby.StateChanged
Match.Starting
Match.Created
Error
```

## Lobby UI

Suggested layout:

```text
Lobby: Forest Run #42

Players:
[HOST] AsdaRunner       Selecting Dash       Not Ready
       VectorMage       Selecting Reflect    Ready
       PhysicsGoblin    Selecting Pulse      Ready

Selected Ability:
[Dash] [Gravity Pulse] [Reflect]

Character Color:
[0] [1] [2] [3]

[Ready]
[Start Run] host only
[Leave Lobby]
```

## Transition to Match

```text
1. Host presses Start Run.
2. Server checks lobby state.
3. Server checks ready rules.
4. Server creates match room.
5. Server sends Match.Starting event.
6. Clients show countdown/loading UI.
7. Unity loads GameScene.
8. Clients connect to match endpoint.
9. Server spawns players using lobby data.
```

## Edge Cases

Handle later, but document early:

- host leaves
- player disconnects
- duplicate login
- token expires
- lobby is full
- match starts while someone is loading
- ability selection becomes invalid
- player refreshes browser tab
- server restarts
