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

### D-004: Description Panel Uses Section-Based Composite Content

Status:

- active

Decision:

- the right-side description area is authored and rendered as section-based composite content inside `DescriptionPanel`, and the legacy `DescriptionText` runtime path is no longer part of the active architecture.

Why:

- a single rich-text dump is weak both visually and structurally;
- different tabs need different card composition, not just differently formatted paragraphs;
- section-based UI gives better control over empty-state hiding, localization growth, and future card-specific styling;
- tests can reason about section visibility and ordering much more reliably than about one large formatted string.

Touches:

- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Editor/ShowcaseLayoutTool.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`
- `Assets/Showcase/Tests/EditMode/Editor/UI/DescriptionPanelTests.cs`

### D-005: Repo-Native Memory Before External Memory Tooling

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

### D-006: Runtime Visuals Use Assigned Carrier Prefabs

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
