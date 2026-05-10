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

### T-001: Verify Description Panel On Real Long-Form Content

Status:

- in progress

Goal:

- verify that the shipped `DescriptionText`-based panel remains readable and stable for long `RU` and `EN` content.

Known State:

- the panel now follows the shipped `Viewport -> DescriptionText` contract only;
- tabs are auto-laid out instead of relying on old absolute positions;
- `DescriptionPanelTests` now cover section ordering, hidden optional sections, fallback text, and bullet formatting against the shipped contract;
- optional sections can still hide cleanly inside the rendered body.

Next Useful Steps:

1. verify panel behavior in Unity for long `RU` and `EN` strings on the restored baseline;
2. tune spacing, wrapping, and scroll feel only if a real readability issue appears in the shipped prefab;
3. avoid inventing a second authoring model unless there is a deliberate visual redesign.

Primary Files:

- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

### T-002: Keep Prefab, Description Panel, And Editor Tooling Locked To One Contract

Status:

- in progress

Goal:

- keep prefab structure, `DescriptionPanel`, validator rules, and `ShowcaseLayoutTool` consistent around one shipped hub baseline.

Known State:

- the current hub prefab is again the visual source of truth;
- `NewUIManager` has been removed from the current tree;
- the hub prefab, runtime, and validator now all use the legacy `DescriptionText` viewport path;
- `ShowcaseLayoutTool` public rebuild/apply entrypoints are intentionally in safe mode because automatic normalization already proved too destructive;
- the remaining private `ShowcaseLayoutTool` path is now a preserve-first normalizer for a few known drift cases, not a layout generator;
- this area still matters because prefab layout, compatibility code, and validator expectations are all part of the same teaching-critical contract.

Next Useful Steps:

1. keep the prefab, not the rebuild tool, as the long-term source of truth unless there is a very strong reason to reverse that;
2. verify whether other UI cards need validator-style structural contracts without introducing another auto-rebuild path;
3. keep `ShowcaseLayoutTool` preserve-only unless a full redesign is explicitly justified.

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
- `ShowcaseLayoutToolTests` now cover preserve-only normalization and drift-cleanup scenarios;
- `ShowcaseLayoutToolTests` are now explicitly guarding preserve/normalization behavior, not layout generation;
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
- the contract is now documented explicitly in `Docs/HubAndCarrierAuthoring.md`.

Next Useful Steps:

1. decide whether emitter-heavy VFX variants should also converge on reusable carrier-style authoring where it improves teaching clarity;
2. consider whether carrier-prefab naming/material conventions need their own lightweight doc or editor helper;
3. keep validator coverage focused on missing visual-prefab or actor-profile assignments for teaching-critical variants.

Primary Files:

- `Assets/Showcase/Runtime/ShowcaseVisualInstanceFactory.cs`
- `Assets/Modules/LayeredCharacterAnimation/Runtime/HumanoidAnimationVariant.cs`
- `Assets/Editor/ShowcaseValidator.cs`
