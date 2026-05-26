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
- `DescriptionPanelTests` now cover section ordering, hidden optional sections, optional-body handling, and bullet formatting against the shipped contract;
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
- `ShowcaseLayoutTool` is now an explicit safe-mode stub only;
- legacy layout normalization code has been removed instead of being kept behind disabled paths;
- this area still matters because prefab layout, compatibility code, and validator expectations are all part of the same teaching-critical contract.

Next Useful Steps:

1. keep the prefab, not the rebuild tool, as the long-term source of truth unless there is a very strong reason to reverse that;
2. verify whether other UI cards need validator-style structural contracts without introducing another auto-rebuild path;
3. only reintroduce layout tooling if a fully explicit, non-destructive contract is designed first.

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
- `ShowcaseLayoutToolTests` now only guard that the deprecated editor entrypoints stay in safe mode and do not pretend to normalize layout;
- `ShowcaseLocalizationTableTests` now exercise real `ShowcaseContent` lookups and explicit missing-marker behavior against configured localization assets;
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

### T-006: Tighten Recruiter Demo Content Contract

Status:

- in progress

Goal:

- keep the guided recruiter-review flow aligned with the project's data-driven story and localization contract.

Known State:

- the current guided demo shell is localized for its chrome text;
- `RecruiterDemoScenarioSO` now owns ordered steps, note localization keys, durations, stress modes, and camera cues;
- `RecruiterDemoController` now references `ModuleDefinitionSO` and `VariantDefinitionSO` directly through the scenario asset instead of routing by `asset.name`;
- recruiter-demo step notes now resolve through `ShowcaseContent` keys instead of bilingual strings inside the scenario asset.

Next Useful Steps:

1. add focused validator/test coverage only if the scenario gains another authoring contract beyond the current required note keys and direct references;
2. keep the scenario small and explicit rather than rebuilding a second full content-management layer around it;

Primary Files:

- `Assets/Showcase/UI/RecruiterDemoController.cs`
- `Assets/Showcase/Runtime/RecruiterDemoScenarioSO.cs`
- `Assets/Showcase/Data/RecruiterDemoScenario.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_en.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_ru.asset`

### T-007: Interview Arena Module (Planned)

Status:

- planning

Goal:

- separate `InterviewArena` scene + hub launch dock (not an 8th `ModuleDefinitionSO`); phased gameplay: local movement → lobby mock → server-backed run.

Known State:

- full design package lives in `Docs/Modules/InterviewArena/` (imported from `Assets/Content/interview-arena-project-docs`);
- integration assessment and phased rollout documented in `Docs/Modules/InterviewArena/INTEGRATION_ASSESSMENT.md`;
- Cursor rules: `.cursor/rules/interview-arena.mdc`;
- architecture core in code (`GameStateMachine`, `InterviewArenaRuntimeContext`, `PhysicsQueryService`, buffered input, slope locomotion);
- player mechanics polished without requiring a configured scene (prefab fields may stay empty).

Next Useful Steps:

1. run **Learning Architect → Interview Arena → Setup Interview Arena Scenes** in Unity (refresh scene + build settings + hub dock);
2. Play Mode: WASD move, Space jump, Shift dash, reach green finish portal;
4. add `ShowcaseContent` keys for game screens when UI grows;
5. choose backend stack (`docs/11_BACKEND_STACK_OPTIONS.md`) before MVP2 lobby.

Primary Files:

- `Docs/Modules/InterviewArena/README.md`
- `Docs/Modules/InterviewArena/INTEGRATION_ASSESSMENT.md`
- `Docs/Modules/InterviewArena/docs/04_BACKLOG.md`
