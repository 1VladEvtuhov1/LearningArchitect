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

- the shipped `ArchitectureShowcaseHub` baseline has been restored to the older hand-authored visual layout;
- `DescriptionPanel` runtime now follows the shipped `DescriptionText` viewport path only;
- validator now enforces the same shipped layout instead of carrying migration-era dual support;
- `ShowcaseLayoutTool` public rebuild/apply entrypoints are intentionally left in safe mode so the tool does not silently rewrite the hub again;
- the remaining `ShowcaseLayoutTool` internals now act only as a small preserve-first normalizer for known drift, not as a UI regeneration system;
- the remaining UI question is readability/polish on the shipped panel, not parallel layout models.

### Module Visual State

- most runtime variants now instantiate shared carrier prefabs through serialized `visualPrefab` references;
- `Assets/Showcase/Art/ModuleCarriers` is the visual source of truth for showcase markers across the active runtime modules;
- the AI module has already been reduced to the same prefab-driven marker path as the other active runtime modules;
- the layered animation module now also requires a configured actor-prefab profile instead of procedural placeholder rigs.

### Authoring Contract State

- `Docs/HubAndCarrierAuthoring.md` is now the explicit contract doc for:
  - the hub prefab baseline;
  - `DescriptionPanel` viewport contract;
  - reusable carrier-prefab wiring.

### Content Authoring Rule

- visible hub copy comes from `ShowcaseContent` localization tables first;
- ScriptableObject fields remain fallback content and asset-local documentation;
- when UI text looks wrong, check localization tables before changing module assets.

### Testing State

- baseline EditMode coverage exists for `ModuleRuntimeHost`, `ShowcaseCoordinator`, `ShowcaseRuntimeController`, `ShowcaseValidator`, `DescriptionPanel`, `HubUI`, module contracts, and AI simulation logic;
- a thin PlayMode smoke layer now covers end-to-end module activation and stress propagation through the real runtime shell;
- localization tests now touch real `ShowcaseContent_en` / `ShowcaseContent_ru` assets and prove both lookup and fallback behavior against configured tables;
- validator coverage now guards the required `DescriptionPanel` prefab hierarchy in addition to module authoring contracts;
- validator-driven checks are now an important part of prefab/data confidence, not just manual play-mode inspection.

### Memory Workflow Rule

- repo-native memory is the active continuity system for now;
- do not introduce external persistent-memory tooling until the repo docs stop changing rapidly;
- if a note still matters next week, move it into `Decisions.md` or keep it alive in `OpenThreads.md`.
