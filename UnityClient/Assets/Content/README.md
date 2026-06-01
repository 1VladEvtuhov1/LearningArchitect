# LearningArchitect content root

All **project-owned** gameplay, showcase, modules, editor tooling, and related assets live under **`Assets/Content/`**.

Open the Unity project from `UnityClient/` in Unity Hub. Paths below are relative to that folder.

## Layout

| Folder | Purpose |
|--------|---------|
| `Content/Showcase/` | Hub scene, shared runtime shell, localization tables |
| `Content/Modules/` | Pluggable modules (including `InterviewArena/`) |
| `Content/Shared/` | Cross-module shared assets |
| `Content/Editor/` | Learning Architect menu tools, validators, WebGL build |
| `Content/Prefabs/` | Shared prefabs not tied to one module |
| `Content/Screenshots/` | Working captures (gitignored) |
| `Content/Tests/` | Cross-cutting play/edit mode tests |

## Entry scenes

- `Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity`
- `Assets/Content/Modules/InterviewArena/Scenes/InterviewArena.unity`

## Outside `Content/` (engine / packages)

- `Assets/Settings/` — URP and project settings assets
- `Assets/AddressableAssetsData/`
- `Assets/TextMesh Pro/`, `Assets/TutorialInfo/` — third-party / template

## Documentation

- Unity client docs: `UnityClient/Docs/`
- Product workspace docs: `../../Docs/`

Do not use this folder as a dump for external doc packages — specs belong in `UnityClient/Docs/`.
