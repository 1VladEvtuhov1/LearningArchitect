# ExtractionRPG Architecture

## Workspace Topology

```text
ExtractionRPG/                    (repository root — product workspace)
├── UnityClient/                  Unity 6000 WebGL project
│   ├── Assets/
│   │   ├── Content/              project-owned assets (showcase, modules, editor tools)
│   │   │   ├── Showcase/       hub shell, localization
│   │   │   └── Modules/
│   │   │       └── InterviewArena/   separate game scene + combat loop
│   │   ├── Settings/             URP / project settings assets
│   │   └── AddressableAssetsData/
│   ├── Packages/
│   ├── ProjectSettings/
│   └── Docs/                     Unity/showcase-specific documentation
├── Backend/                      ASP.NET Core (scaffold only)
│   ├── src/
│   └── tests/
├── Docs/                         product-level docs (this folder)
├── Site/                         static portfolio + embedded WebGL host
└── docker-compose.yml            PostgreSQL + Redis for local dev
```

## Client (Unity)

**Open in Unity Hub:** `UnityClient/` (not the repo root).

Two experiences in one client:

1. **Architecture showcase** — data-driven hub (`ArchitectureShowcase.unity`), module variants, stress presets, metrics.
2. **Interview Arena** — separate scene (`InterviewArena.unity`), locomotion, combat, AI, buffs; opened from hub dock or directly.

Detailed runtime design: `UnityClient/Docs/Architecture.md` and `UnityClient/Docs/Modules/InterviewArena/CODE_ARCHITECTURE.md`.

## Backend (planned)

ASP.NET Core API behind:

- pin / username login and session tokens
- lobby CRUD and ready-check
- match lifecycle and authoritative results
- PostgreSQL persistence; Redis for ephemeral/session cache (TBD)

Scaffold only in `Backend/` until implementation phase. No business logic in the migration PR.

## Delivery

- **Editor / WebGL build:** from `UnityClient/`
- **Public demo:** `Site/` embeds WebGL build (`Site/webgl/`) with bilingual landing copy

## Trust Boundaries

| Component | Trust |
|-----------|--------|
| Unity WebGL client | Input, rendering, prediction; not final match authority |
| Backend | Sessions, lobby state, match results, persistence |
| Site | Static hosting only; no game logic |

## Assembly / Code Boundaries (Unity)

See `UnityClient/Docs/Architecture.md` for `LearningArchitect.Showcase`, `LearningArchitect.Modules.*`, and editor test assemblies.
