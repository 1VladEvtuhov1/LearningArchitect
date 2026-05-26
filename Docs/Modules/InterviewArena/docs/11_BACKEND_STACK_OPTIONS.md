# 11. Backend Stack Options

Pick one backend stack and keep it simple.

## Option A: ASP.NET Core

Good for:

- C# end-to-end
- strong typing
- clean DTOs
- WebSocket support
- good interview value for C# developer

Suggested structure:

```text
server/
├── InterviewArena.Server/
│   ├── Auth/
│   ├── Lobbies/
│   ├── Matches/
│   ├── Realtime/
│   └── Program.cs
└── InterviewArena.Server.Tests/
```

Pros:

- same language as Unity
- good architecture practice
- easy DTO sharing later

Cons:

- slightly heavier setup than Node

## Option B: Node.js + TypeScript

Good for:

- fast WebSocket prototype
- simple deployment
- lots of examples

Suggested structure:

```text
server/
├── src/
│   ├── auth/
│   ├── lobbies/
│   ├── matches/
│   ├── realtime/
│   └── index.ts
└── package.json
```

Pros:

- quick iteration
- lightweight
- easy WebSocket server

Cons:

- different language from Unity
- DTO typing has to be maintained separately

## Option C: Go

Good for:

- simple deployable server binary
- performance
- clean concurrency model

Pros:

- efficient
- one binary deployment
- good for WebSocket backends

Cons:

- extra language learning
- less directly connected to Unity/C# interview track

## Recommendation

For this project, choose:

```text
ASP.NET Core
```

Reason:

- same C# language family
- useful for explaining full-stack C# architecture
- good fit for typed API contracts
- easier mental model when moving between Unity client and backend

Keep persistence in memory at first.

Upgrade path:

```text
In-memory store -> SQLite -> PostgreSQL
```
