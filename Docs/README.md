# ExtractionRPG Documentation

Product-level documentation for the **ExtractionRPG** workspace.

Unity-specific docs (showcase hub, Interview Arena scene composition, WebGL runbook) live under **`UnityClient/Docs/`**. Paths in those documents are relative to `UnityClient/` unless stated otherwise.

## Reading Order

1. `Architecture.md` — workspace topology
2. `ClientServerFlow.md` — login → match → result
3. `ApiContract.md` — backend API draft
4. `Roadmap.md` — phases and status
5. `Decisions.md` — durable product/workspace choices

## Path Conventions

| Layer | Example path |
|-------|----------------|
| Unity client | `UnityClient/Assets/Content/Modules/InterviewArena/` |
| Showcase entry scene | `UnityClient/Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity` |
| Backend (future) | `Backend/src/` |
| Portfolio shell | `Site/` |

## Related Docs (Unity client)

- Showcase architecture: `UnityClient/Docs/Architecture.md`
- Interview Arena: `UnityClient/Docs/Modules/InterviewArena/README.md`
- Web deployment: `UnityClient/Docs/WebDeployment.md`
- Open threads: `UnityClient/Docs/OpenThreads.md`
