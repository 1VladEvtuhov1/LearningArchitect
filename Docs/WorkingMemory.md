# LearningArchitect Working Memory

This file is a short-lived operational snapshot for the current shape of the project.

Use it for:

- fast re-entry into active architecture work;
- context that is useful now but may become stale later;
- links between current systems, pain points, and recent changes.

Review it regularly and trim anything that has become a stable decision or a resolved task.

## Current Snapshot

### Showcase Shape

- the project is currently one shared architectural showcase, not a set of separate demo scenes;
- `Assets/Showcase/Scenes/ArchitectureShowcase.unity` is the main entry scene;
- `ArchitectureShowcaseHub` is the UI and orchestration shell;
- modules are loaded through `ModuleDefinitionSO` and `VariantDefinitionSO`.

### UI State

- the old single `DescriptionText` approach is no longer the preferred direction;
- the active direction is a section-based right-side description panel;
- `DescriptionPanel`, `NewUIManager`, and `ShowcaseLayoutTool` were already moved toward the composite viewport model;
- the next quality step is stronger per-card behavior and cleaner tests around tab composition.

### Content Authoring Rule

- visible hub copy comes from `ShowcaseContent` localization tables first;
- ScriptableObject fields remain fallback content and asset-local documentation;
- when UI text looks wrong, check localization tables before changing module assets.

### Testing State

- baseline EditMode coverage exists for `ModuleRuntimeHost`, `ShowcaseCoordinator`, `ShowcaseRuntimeController`, and AI simulation logic;
- `LearningArchitect.Core.Edit` currently passes in Unity Test Framework;
- the next missing layer is `LearningArchitect.UI.Edit`, especially for `DescriptionPanel`.

### Memory Workflow Rule

- repo-native memory is the active continuity system for now;
- do not introduce external persistent-memory tooling until the repo docs stop changing rapidly;
- if a note still matters next week, move it into `Decisions.md` or keep it alive in `OpenThreads.md`.
