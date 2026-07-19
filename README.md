# ExtractionRPG

Unity WebGL + ASP.NET Core backend portfolio project.

The repository is a **product workspace**: Unity client, backend scaffold, shared documentation, and a static portfolio shell for WebGL delivery.

## Repository Structure

- `UnityClient/` — Unity WebGL client, LearningArchitect showcase, Interview Arena scene
- `Backend/` — ASP.NET Core backend API scaffold (implementation starts after migration)
- `docs/` — product-level documentation (architecture, API contract, roadmap)
- `Site/` — portfolio shell for WebGL delivery
- `docker-compose.yml` — local infrastructure (PostgreSQL, Redis)

Unity-specific runbooks, showcase docs, and Interview Arena specs live under **`UnityClient/docs/`**.

## Main Product Flow

Pin login → Lobby → Ready-check → Match → Extraction/Death → Result persistence.

## How to open Unity client

1. Open **`UnityClient/`** in Unity Hub (not the repository root).
2. Open `Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity`.
3. For Interview Arena setup: **Learning Architect → Interview Arena → Setup Complete (Prefabs + Scene + Wire Player)**.

## Documentation

Index: [docs/README.md](docs/README.md)

| Topic | Location |
|-------|----------|
| Product architecture | `docs/Architecture.md` |
| Client ↔ server flow | `docs/ClientServerFlow.md` |
| API contract (draft) | `docs/ApiContract.md` |
| Roadmap | `docs/Roadmap.md` |
| Cross-cutting decisions | `docs/Decisions.md` |
| Unity showcase & Arena | `UnityClient/docs/README.md` |
| WebGL build & deploy | `UnityClient/docs/WebDeployment.md` |

## Local infrastructure

```powershell
docker compose up -d
```

PostgreSQL: `localhost:5432` (db/user/password: `extractionrpg`). Redis: `localhost:6379`.

## Portfolio site

```powershell
python -m http.server 8081 -d Site
```

See `Site/README.md`.
