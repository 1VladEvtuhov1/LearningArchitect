# LearningArchitect Open Threads

This file tracks unfinished work that is still relevant across sessions.

Use it for:

- active implementation threads;
- verification gaps;
- follow-up tasks that depend on earlier decisions.

Do not use it for:

- historical notes that are already resolved;
- broad ideas with no immediate owner or value.

## Current Threads

### T-001: Finish Description Panel Card Model

Status:

- in progress

Goal:

- move `DescriptionPanel` from section remapping to a cleaner per-card model with stronger tab composition and card-specific behavior.

Known State:

- the panel already supports composite section UI;
- tabs are auto-laid out instead of relying on old absolute positions;
- optional sections can now hide;
- the implementation is still closer to dynamic section remapping than to fully specialized card views.

Next Useful Steps:

1. add EditMode coverage for tab composition and hidden empty sections;
2. verify panel behavior in Unity for long RU and EN strings;
3. decide whether `pros/cons` should stay text-backed or become row-based list items.

Primary Files:

- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

### T-002: Align Prefab, Runtime UI Builder, And Editor Tooling

Status:

- in progress

Goal:

- keep prefab structure, `NewUIManager`, and `ShowcaseLayoutTool` consistent so the project does not silently drift back toward legacy `DescriptionText` assumptions.

Known State:

- code paths already prefer the new section-based viewport structure;
- prefab cleanup removed old serialized `DescriptionText` references;
- this area remains fragile because layout can still be changed both manually and through tooling.

Next Useful Steps:

1. verify the current prefab hierarchy after manual editor changes;
2. reduce legacy fallback paths once the new hierarchy is stable;
3. decide whether the prefab or tooling is the long-term source of truth for UI layout.

Primary Files:

- `Assets/Showcase/UI/NewUIManager.cs`
- `Assets/Editor/ShowcaseLayoutTool.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

### T-003: Expand Test Coverage Beyond Current Core Baseline

Status:

- in progress

Goal:

- grow confidence around orchestration and UI composition without bloating PlayMode coverage.

Known State:

- `LearningArchitect.Core.Edit` now covers `ModuleRuntimeHost`, `ShowcaseCoordinator`, and `ShowcaseRuntimeController`;
- `LearningArchitect.AI.Edit` covers current AI simulation logic;
- there is still no focused coverage for `DescriptionPanel` composition or localization fallback.

Next Useful Steps:

1. add `LearningArchitect.UI.Edit` tests for `DescriptionPanel`;
2. add a small PlayMode smoke layer for variant activation and stress propagation;
3. keep all test-only code in `Editor` or test-only assemblies.

Primary Files:

- `Docs/TestingStrategy.md`
- `Assets/Showcase/Tests/EditMode/Core`
- `Assets/Showcase/UI`

### T-004: Replace Procedural Humanoid Placeholder With Real Character Pipeline

Status:

- in progress

Goal:

- move the humanoid `Animation3D` variant from primitive placeholder rigs to a real prefab-driven humanoid animation setup.

Known State:

- the project now has scaffold code for a humanoid animation profile and animator-driven actor runtime;
- the current showcase can still fall back to the procedural placeholder if no real actor profile is assigned;
- the remaining dependency is content authoring: model import, avatar validation, controller setup, and prefab wiring.

Next Useful Steps:

1. import the humanoid model and validate the avatar;
2. create a first-pass actor prefab with `Animator`;
3. create and assign `HumanoidAnimationProfileSO`;
4. tune locomotion parameters before increasing crowd size.

Primary Files:

- `Assets/Modules/LayeredCharacterAnimation/Runtime/HumanoidAnimationProfileSO.cs`
- `Assets/Modules/LayeredCharacterAnimation/Runtime/HumanoidCrowdActor.cs`
- `Assets/Modules/LayeredCharacterAnimation/Runtime/HumanoidAnimationVariant.cs`
- `Docs/HumanoidAnimationSetup.md`
