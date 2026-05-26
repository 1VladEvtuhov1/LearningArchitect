# LearningArchitect Decisions

This file stores project decisions that are still expected to matter across sessions.

Use it for:

- architectural choices that shape multiple files or systems;
- source-of-truth rules;
- decisions that explain why the project is organized the way it is.

Do not use it for:

- temporary todos;
- implementation logs;
- visual polish notes that may change daily.

## Active Decisions

### D-001: One Shared Showcase Runtime

Status:

- active

Decision:

- the project is organized around one active showcase scene and one shared runtime shell instead of separate standalone demo scenes.

Why:

- module comparison only works well when every variant runs inside the same shell;
- shared stress controls, state publication, and UI are more valuable than parallel demo entry points;
- isolated legacy scenes were keeping old architecture alive without contributing to the current product story.

Touches:

- `Assets/Showcase/Scenes/ArchitectureShowcase.unity`
- `Assets/Showcase/Runtime`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

### D-002: Data-Driven Module Authoring

Status:

- active

Decision:

- module and variant wiring is authored through `ModuleDefinitionSO` and `VariantDefinitionSO`.

Why:

- the hub needs a stable way to load unrelated systems through the same orchestration flow;
- stress presets and descriptive content should stay attached to module data instead of scene-only wiring;
- adding a new module should be a repeatable asset workflow rather than custom scene logic.

Touches:

- `Assets/Showcase/Runtime/ModuleDefinitionSO.cs`
- `Assets/Showcase/Runtime/VariantDefinitionSO.cs`
- `Assets/Modules/*/Data`

### D-003: Localization Tables Are The Only Copy For Visible Text

Status:

- active

Decision:

- runtime UI resolves module and variant prose **only** through `ShowcaseContent` (and shared chrome through `ShowcaseUI`); missing required keys surface explicit `[MISSING: ...]` markers in the UI and validator findings in the editor.
- localization lookup identity comes from explicit `localizationKey` fields on `ModuleDefinitionSO` and `VariantDefinitionSO`, not from asset names.
- `ModuleDefinitionSO` / `VariantDefinitionSO` do not duplicate EN/RU strings; they carry keys, taxonomy, prefab wiring, and stress counts.

Why:

- visible UI copy must be language-aware;
- a single authoring surface avoids drift between duplicate SerializedObject text fields and shipped localization tables;
- asset renames should not silently break visible content;
- this avoids hardcoding language selection rules inside every presenter or panel.

Touches:

- `Assets/Showcase/UI/ShowcaseLocalization.cs`
- `Assets/Showcase/UI/ShowcaseLocalizationContent.cs`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_en.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_ru.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseUI_en.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseUI_ru.asset`
- `Assets/Showcase/Runtime/ModuleDefinitionSO.cs`
- `Assets/Showcase/Runtime/VariantDefinitionSO.cs`

### D-008: Browser Delivery Uses Live Metrics, Not A Hidden Reference Catalog

Status:

- active

Decision:

- browser delivery keeps the shared live metrics path and uses conservative presets instead of a separate runtime reference-metrics catalog.

Why:

- one metrics path is easier to reason about and document than parallel live-vs-reference runtime branches;
- hidden browser-only metrics layers create doc drift and confuse learning value;
- browser clarity should come from framing and preset discipline, not from a second silent data source.

Touches:

- `Assets/Showcase/UI/MetricsOverlayHost.cs`
- `Assets/Showcase/UI/MetricsOverlayRuntime.cs`
- `Assets/Showcase/UI/MetricsOverlayPresenter.cs`
- `Docs/Architecture.md`
- `WEBGL.md`

### D-004: Hub Prefab Visual Baseline Takes Priority Over UI Normalization

Status:

- active

Decision:

- `ArchitectureShowcaseHub.prefab` is the visual source of truth for the showcase shell, and tooling should not regenerate or normalize that layout at the cost of changing the shipped baseline.

Why:

- the project already had a stronger hand-authored UI composition than the generated rebuild path;
- preserving a good baseline is more important than enforcing internal naming purity;
- the hub is teaching-critical, so accidental editor-tool layout rewrites are worse than carrying temporary compatibility code.

Touches:

- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`
- `Assets/Editor/ShowcaseLayoutTool.cs`
- `Docs/HubAndCarrierAuthoring.md`

### D-005: Description Panel Follows The Shipped Legacy Viewport Contract

Status:

- active

Decision:

- `DescriptionPanel` runtime, validation, and tests follow the shipped `Viewport -> DescriptionText` baseline.

Why:

- the restored shipped hub prefab already provides the clearest public-facing baseline;
- one explicit contract is easier to learn from than a migration-era dual-path UI;
- validator and tooling should preserve the shipped shell instead of teaching two competing authoring models.

Touches:

- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Editor/ShowcaseValidator.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`
- `Docs/HubAndCarrierAuthoring.md`

### D-006: Repo-Native Memory Before External Memory Tooling

Status:

- active

Decision:

- project continuity is documented in-repo through `Docs/Decisions.md`, `Docs/OpenThreads.md`, and `Docs/WorkingMemory.md` before introducing external persistent-memory tooling.

Why:

- the repository needs a transparent, inspectable memory layer first;
- source of truth must stay in repo artifacts, not in agent-managed storage;
- this keeps the workflow simple while the architecture is still changing quickly.

Touches:

- `Docs/Decisions.md`
- `Docs/OpenThreads.md`
- `Docs/WorkingMemory.md`

### D-008: Interview Arena Uses A Separate Scene

Status:

- active

Decision:

- Interview Arena is not an eighth `ModuleDefinitionSO` entry in the architecture showcase hub.
- gameplay lives in `Assets/Modules/InterviewArena/Scenes/InterviewArena.unity` and is opened from the hub through `ShowcaseSceneLoader` / `InterviewArenaLaunchDock`.
- the architecture showcase scene remains the default build entry point.

Why:

- the game loop (movement, lobby, networking) does not fit the showcase stress/metrics/module-variant contract;
- a separate scene keeps the seven teaching modules focused while still shipping one WebGL build with two experiences;
- the local player must be a **scene/prefab-authored** object, not runtime-spawned geometry (production and interview signal).

Touches:

- `Assets/Showcase/Runtime/ShowcaseSceneLoader.cs`
- `Assets/Showcase/UI/InterviewArenaLaunchDock.cs`
- `Assets/Modules/InterviewArena/`

### D-007: Runtime Visuals Use Assigned Carrier Prefabs

Status:

- active

Decision:

- runtime variants should prefer assigned `visualPrefab` carriers and `ShowcaseVisualInstanceFactory` over inline primitive construction for their showcase-facing visuals.

Why:

- simulation logic should stay independent from how the marker looks in the scene;
- prefab-driven visuals make module variants easier to inspect, swap, and validate;
- module contract tests can verify visual wiring directly instead of relying on hidden runtime defaults.

Touches:

- `Assets/Showcase/Runtime/ShowcaseVisualInstanceFactory.cs`
- `Assets/Showcase/Art/ModuleCarriers`
- `Assets/Modules/*/Prefabs`
- `Assets/Modules/Tests/EditMode/Editor/ModuleContractTests.cs`
