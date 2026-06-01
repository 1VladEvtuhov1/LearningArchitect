# ExtractionRPG Backend

ASP.NET Core backend for the Interview Arena / Extraction RPG loop.

Planned responsibilities:

- pin-code login
- JWT/session handling
- lobby
- ready-check
- match lifecycle
- extraction/death result persistence
- player run history
- leaderboard
- PostgreSQL persistence
- Docker local infrastructure

Implementation starts after repository migration.

## Layout

```text
Backend/
├── src/     # ASP.NET Core API (scaffold)
└── tests/   # backend tests (scaffold)
```

Local dependencies: see root `docker-compose.yml`.
