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

### D-003: Localization Tables Override ScriptableObject Copy

Status:

- active

Decision:

- runtime UI resolves module and variant copy through `ShowcaseContent` localization tables first, and only falls back to ScriptableObject fields when no localized entry is available.

Why:

- visible UI copy must be language-aware;
- module assets should remain self-describing even when localization entries are missing;
- this avoids hardcoding language selection rules inside every presenter or panel.

Touches:

- `Assets/Showcase/UI/ShowcaseLocalization.cs`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_en.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_ru.asset`

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
