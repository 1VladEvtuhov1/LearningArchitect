# ExtractionRPG Documentation

Product-level documentation for the **ExtractionRPG** workspace.

Unity-specific docs (showcase hub, Interview Arena scene composition, WebGL runbook) live under **`UnityClient/docs/`**. Paths in those documents are relative to `UnityClient/` unless stated otherwise.

## Reading Order

1. [Architecture.md](Architecture.md) — workspace topology, mermaid, cross-project links
2. [ClientServerFlow.md](ClientServerFlow.md) — login → match → result
3. [ApiContract.md](ApiContract.md) — backend API draft
4. [Roadmap.md](Roadmap.md) — phases and status
5. [SkillsRoadmap.md](SkillsRoadmap.md) — interview skills checklist, project status, implementation priorities
6. [Decisions.md](Decisions.md) — durable product/workspace choices

## Agent workflow

- [`Engineering_Handbook.md`](Engineering_Handbook.md) — универсальные правила (KISS, fix workflow, нейминг)
- [`AGENT_PROJECT.md`](AGENT_PROJECT.md) — LearningArchitect: пути, Arena wiring, существующий API
- [`../AGENTS.md`](../AGENTS.md) — точка входа для Cursor / AI

## Path Conventions

| Layer | Example path |
|-------|----------------|
| Unity client | `UnityClient/Assets/Content/Modules/InterviewArena/` |
| Showcase entry scene | `UnityClient/Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity` |
| Backend (future) | `Backend/src/` |
| Portfolio shell | `Site/` |

## Related Docs (Unity client)

- Showcase architecture: `UnityClient/docs/Architecture.md`
- Interview Arena: `UnityClient/docs/Modules/InterviewArena/README.md`
- Web deployment: `UnityClient/docs/WebDeployment.md`
- Open threads: `UnityClient/docs/OpenThreads.md`

## Portfolio showcase

Live WebGL demo via `Site/`. Spec: sibling repo `portfolio-hub` → `docs/projects/learningarchitect.md`  
(Obsidian: [[portfolio-hub/docs/projects/learningarchitect]] · [[portfolio-hub/docs/PORTFOLIO]]).

## Obsidian entry

- [[LearningArchitect/docs/Architecture]]
- [[Dashboard]]
