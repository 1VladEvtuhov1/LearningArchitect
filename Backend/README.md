# ExtractionRPG Backend

ASP.NET Core API for the Interview Arena / Extraction RPG loop.

## Layout

```text
Backend/
├── ExtractionRpg.slnx
├── src/ExtractionRpg.Api/
│   ├── Auth/           login, sessions, users (in-memory MVP)
│   ├── Lobbies/        list, create, join, ready, start
│   ├── Matches/        in-memory match records (connectUrl stub)
│   ├── Profile/        authenticated profile
│   └── Common/         API errors, Bearer helper
└── tests/ExtractionRpg.Api.Tests/
```

## Run locally

1. Infrastructure (from repo root):

```powershell
docker compose up -d
```

2. API:

```powershell
cd Backend/src/ExtractionRpg.Api
dotnet run
```

- Root: `http://localhost:5000/`
- Liveness: `GET /health`
- Readiness (Postgres): `GET /health/ready`
- Login: `POST /api/auth/login-by-name` — see `Docs/ApiContract.md`
- Profile: `GET /api/profile/me` with `Authorization: Bearer {sessionToken}`
- OpenAPI (Development): `/openapi/v1.json`

## Tests

```powershell
cd Backend
dotnet test
```

Postgres integration (optional):

```powershell
$env:EXTRACTIONRPG_INTEGRATION = "1"
dotnet test
```

## Implemented

- [x] Health endpoints + CORS for Site / Unity local preview
- [x] `POST /api/auth/login-by-name` (opaque session tokens, in-memory store)
- [x] `GET /api/profile/me` (Bearer session validation)
- [x] `GET /api/lobbies`, `POST /api/lobbies`, join, ready, **start match**
- [x] Structured API errors (`errorCode`, `message`)

## Next (see `Docs/Roadmap.md`)

- PostgreSQL persistence for users/sessions/lobbies
- Match start + result persistence
