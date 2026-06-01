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
- `Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity` is the main entry scene;
- `ArchitectureShowcaseHub` is the UI and orchestration shell;
- modules are loaded through `ModuleDefinitionSO` and `VariantDefinitionSO`.

### UI State

- the shipped `ArchitectureShowcaseHub` baseline has been restored to the older hand-authored visual layout;
- `DescriptionPanel` runtime now follows the shipped `DescriptionText` viewport path only;
- validator now enforces the same shipped layout instead of carrying migration-era dual support;
- `ShowcaseLayoutTool` is intentionally reduced to a safe-mode stub so tooling cannot silently rewrite the hub again;
- metrics and status shell UI are no longer one monolithic overlay script; the current path is split into host/runtime/presenter/view/formatter-style pieces;
- the remaining UI question is readability/polish on the shipped panel, not parallel layout models.

### Module Visual State

- most runtime variants now instantiate shared carrier prefabs through serialized `visualPrefab` references;
- `Assets/Content/Showcase/Art/ModuleCarriers` is the visual source of truth for showcase markers across the active runtime modules;
- the AI module has already been reduced to the same prefab-driven marker path as the other active runtime modules;
- the layered animation module now also requires a configured actor-prefab profile instead of procedural placeholder rigs.

### Authoring Contract State

- `Docs/HubAndCarrierAuthoring.md` is now the explicit contract doc for:
  - the hub prefab baseline;
  - `DescriptionPanel` viewport contract;
  - reusable carrier-prefab wiring.

### Content Authoring Rule

- visible hub and description copy comes **only** from Unity Localization tables (`ShowcaseContent` for module/variant prose, `ShowcaseUI` for chrome including module category labels);
- localization identity comes from `ModuleDefinitionSO.localizationKey` and `VariantDefinitionSO.localizationKey`;
- module/variant ScriptableObjects hold wiring and keys only, not duplicate localized strings;
- asset renames are not localization renames;
- when UI text looks wrong, fix table entries and keys before changing prefab wiring.

### Web Metrics State

- the old hidden reference-metrics runtime layer has been removed;
- browser delivery now relies on the same live metrics path as the rest of the showcase, with conservative presets and stronger surrounding page context.

### Testing State

- baseline EditMode coverage exists for `ModuleRuntimeHost`, `ShowcaseCoordinator`, `ShowcaseRuntimeController`, `ShowcaseValidator`, `DescriptionPanel`, `HubUI`, module contracts, and AI simulation logic;
- a thin PlayMode smoke layer now covers end-to-end module activation and stress propagation through the real runtime shell;
- localization tests now touch real `ShowcaseContent_en` / `ShowcaseContent_ru` assets (and selected `ShowcaseUI` keys where chrome matters) and prove table-backed lookup plus explicit missing-marker behavior against configured tables;
- validator coverage now guards the required `DescriptionPanel` prefab hierarchy in addition to module authoring contracts;
- validator-driven checks are now an important part of prefab/data confidence, not just manual play-mode inspection.

### Tooling Noise State

- `Assembly-CSharp.csproj` is a Unity-generated artifact, not an architecture source of truth;
- `MSB3277` warnings around `System.Net.Http` and `System.IO.Compression` currently come from the generated Unity/editor/MCP assembly graph, not from showcase runtime code;
- disposed `NetworkStream` console logs from `MCPForUnity` are external bridge noise unless they correlate with a reproducible project-side failure;
- generated project files and external bridge logs should not be treated as showcase tech debt unless a concrete repo-owned defect is proven.

### Memory Workflow Rule

- repo-native memory is the active continuity system for now;
- do not introduce external persistent-memory tooling until the repo docs stop changing rapidly;
- if a note still matters next week, move it into `Decisions.md` or keep it alive in `OpenThreads.md`.
