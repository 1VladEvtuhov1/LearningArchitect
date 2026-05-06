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
- `DescriptionPanelTests` already cover section visibility, tab structure, and legacy-tab fallback;
- optional sections can now hide;
- the implementation no longer keeps the old `DescriptionText` compatibility path and remains closer to dynamic section remapping than to fully specialized card views.

Next Useful Steps:

1. verify panel behavior in Unity for long `RU` and `EN` strings after recent prefab changes;
2. decide whether `pros/cons` should stay text-backed or become row-based list items;
3. decide how far the panel should move from section remapping toward truly distinct card layouts.

Primary Files:

- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

### T-002: Align Prefab, Description Panel, And Editor Tooling

Status:

- in progress

Goal:

- keep prefab structure, `DescriptionPanel`, and `ShowcaseLayoutTool` consistent so the project does not silently drift away from the composite section model.

Known State:

- code paths already prefer the new section-based viewport structure;
- `NewUIManager` has been removed from the current tree;
- the hub prefab no longer contains `DescriptionText`, and validator coverage now checks the required composite `DescriptionPanel` hierarchy;
- `ShowcaseLayoutTool` can restore missing `DescriptionPanel` composite section skeletons instead of only assuming they already exist;
- the broader rebuild flow now survives the current alias-heavy hub layout and has dedicated EditMode regression coverage through `ShowcaseLayoutToolTests`;
- this area still matters because prefab layout, rebuild tooling, and validator expectations are all part of the same teaching-critical contract.

Next Useful Steps:

1. decide whether the prefab or tooling is the long-term source of truth for UI layout;
2. verify whether other UI cards need the same validator-style structural contract;
3. decide how much non-description card structure the rebuild tool should actively normalize versus only preserve.

Primary Files:

- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Editor/ShowcaseLayoutTool.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

### T-003: Expand Test Coverage Beyond Current Core Baseline

Status:

- in progress

Goal:

- grow confidence around orchestration and UI composition without bloating PlayMode coverage.

Known State:

- `LearningArchitect.Core.Edit` now covers `ModuleRuntimeHost`, `ShowcaseCoordinator`, `ShowcaseRuntimeController`, and `ShowcaseValidator`;
- `LearningArchitect.UI.Edit` already covers `DescriptionPanel` and `HubUI`;
- `LearningArchitect.Modules.Edit` covers runtime visual-prefab wiring and visible-vs-simulated metric contracts across modules;
- `LearningArchitect.AI.Edit` covers current AI simulation logic;
- `LearningArchitect.Showcase.PlayMode` now adds a thin runtime smoke layer for activation and stress propagation;
- validator coverage now includes missing `animationProfile` and missing humanoid `actorPrefab` authoring failures;
- validator coverage also includes the required `DescriptionPanel` prefab hierarchy;
- `ShowcaseLayoutToolTests` now cover the rebuild flow against the current prefab naming scheme and drift-cleanup scenarios;
- `ShowcaseLocalizationTableTests` now exercise both real table lookups and fallback behavior against configured localization assets;
- the remaining gap is broader localization/asset-graph coverage, not the absence of baseline runtime smoke.

Next Useful Steps:

1. decide whether `ShowcaseValidator` should become part of a pre-release checklist or editor automation step;
2. add only the next most valuable PlayMode smoke case if it closes a real orchestration blind spot;
3. extend localization checks to more content surfaces than module description and architecture body.

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
- the current showcase no longer falls back to a procedural placeholder and now requires a valid actor profile;
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

### T-005: Finish Carrier-Prefab Cleanup Across Modules

Status:

- in progress

Goal:

- make the module prefab pattern consistently teachable by separating simulation code from showcase visuals everywhere practical.

Known State:

- most active modules now assign `visualPrefab` carriers through their variant prefabs;
- `ShowcaseVisualInstanceFactory` centralizes runtime marker instantiation and collider stripping;
- AI runtime has already been reduced to the prefab-driven path and no longer carries dead `PrimitiveType` configuration through the simulation host;
- layered animation now also uses the real actor-prefab path and no longer keeps procedural fallback visuals.

Next Useful Steps:

1. document the expected `visualPrefab` contract near module authoring guidance;
2. decide whether emitter-heavy VFX variants should also converge on reusable carrier-style authoring where it improves teaching clarity;
3. consider validator coverage for missing visual-prefab or actor-profile assignments on teaching-critical variants.

Primary Files:

- `Assets/Showcase/Runtime/ShowcaseVisualInstanceFactory.cs`
- `Assets/Modules/LayeredCharacterAnimation/Runtime/HumanoidAnimationVariant.cs`
- `Assets/Editor/ShowcaseValidator.cs`
