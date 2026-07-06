# LearningArchitect Architecture

## Overview

`LearningArchitect` is organized as a single showcase runtime with pluggable modules.

The important shift in the current architecture is this:

- the scene is not the source of truth for individual demos;
- the hub loads modules from data;
- modules share the same lifecycle, stress controls, UI, and comparison flow.

That makes the project closer to a product shell with interchangeable runtime slices than to a collection of one-off prototype scenes.

There is now also a second shell around that runtime for public delivery:

- the Unity showcase runtime remains the system being demonstrated;
- the `Site/` page is the portfolio-facing host that explains the runtime before launch and embeds the WebGL build on the same page.

## Runtime Topology

The main scene is:

- `Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity`

The main scene object is:

- `ArchitectureShowcaseHub`

The hub is driven by:

- `ShowcaseCompositionRoot`
- `ShowcaseRuntimeController`
- `ShowcaseCommandRouter`
- `ShowcaseStateHub`
- `ShowcaseTransitionController`

These components are not domain modules themselves. They are the shell that hosts modules.

The current visual shell source of truth is still the hand-authored hub prefab:

- `Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

That prefab is the thing to preserve first. Tooling should validate or lightly assist it, not silently regenerate its layout.

For browser delivery, the runtime is embedded into the static site host instead of being opened as a standalone Unity page.

## Main Flow

The runtime flow is:

1. `ShowcaseCompositionRoot` resolves dependencies and builds a `ShowcaseCoordinator`.
2. `ShowcaseCoordinator` receives the configured `ModuleDefinitionSO[]`.
3. `ShowcaseRuntimeController` asks the coordinator to activate the selected variant.
4. `VariantSpawner` instantiates the selected variant prefab under the module root.
5. `ModuleRuntimeHost` validates the prefab contracts:
   - `IModule`
   - `IShowcaseStressTarget`
6. `IModule.Enter()` activates the module shell.
7. `IShowcaseStressTarget.SetStressLevel()` applies the selected preset.
8. `ShowcaseStateHub` publishes:
   - current module
   - current variant
   - current stress level
   - active item count
9. UI presenters subscribe to `ShowcaseStateHub` and update the hub.

This architecture keeps domain code out of the scene control flow.

## Contracts

Three small interfaces define the runtime contract for every loaded variant.

### `IModule`

Purpose:

- lifecycle entry and exit for the spawned module root.

Responsibilities:

- react to activation;
- react to deactivation.

In practice the current modules use it as a lightweight shell boundary around the active prefab.

### `IShowcaseStressTarget`

Purpose:

- expose stress control to the shared showcase runtime.

Responsibilities:

- accept stress preset changes through `SetStressLevel(int count)`;
- report the visible or active count through `ActiveCount`.

This keeps stress UI generic and reusable across unrelated systems.

### `IShowcaseMetricsSource`

Purpose:

- expose a normalized metrics snapshot to the shared shell.

Responsibilities:

- return `ShowcaseMetricsSnapshot` with simulation count, visible count, and optional module CPU time;
- let the shell compare very different modules without baking metrics rules into presenters.

In practice this is now part of the real contract even if a module can fall back to partial metrics.

## Data Layer

The showcase is data-driven through two ScriptableObject types that **wire** modules and variants; **all user-visible copy** is stored in Unity Localization string tables.

### `ModuleDefinitionSO`

Contains:

- `localizationKey` — stable id for `ShowcaseContent` / `ShowcaseUI` rows (not the asset file name);
- `category` — taxonomy used for navigation and for **generic** module-type labels in `ShowcaseUI` (`module_category_simulation` / `module_category_architecture`);
- references to `VariantDefinitionSO` instances.

Role:

- defines which variants belong to a module and how the hub classifies it.

### `VariantDefinitionSO`

Contains:

- `localizationKey` — stable id for `ShowcaseContent` rows;
- prefab reference;
- numeric stress presets (counts).

Role:

- defines how a specific implementation is spawned and stress-tested. Long-form explanations, names, and comparison copy live only in `ShowcaseContent`.

Stress preset **button labels** are not duplicated per locale on the asset: the shared UI formats counts unless you later add dedicated table keys.

This means prefab wiring and stress counts are attached to data instead of hardcoded scene logic, while copy stays in one localization place.

## Content Source Of Truth

Visible text is authored **only** in Unity Localization tables.

### `ShowcaseContent` (module and variant copy)

At runtime, the hub resolves module and variant strings through `ShowcaseLocalization` / `ShowcaseLocalizationContent`.

The lookup path is:

1. build the table entry id from `ModuleDefinitionSO.localizationKey` or `VariantDefinitionSO.localizationKey` (for example `effectsmodule.description`, `effects_chunkvariant.architecture`);
2. read the matching `ShowcaseContent` entry for the active locale;
3. if a required entry is missing, the UI shows an explicit `[MISSING: ShowcaseContent.<key>]` marker — there is **no** silent fallback to fields on the ScriptableObject.

Authoritative tables:

- `Assets/Content/Showcase/Localization/Tables/ShowcaseContent_en.asset`
- `Assets/Content/Showcase/Localization/Tables/ShowcaseContent_ru.asset`

### `ShowcaseUI` (chrome and taxonomy labels)

Short hub chrome and fixed labels (tabs, headers, **module category line** derived from `ShowcaseModuleCategory`) live in `ShowcaseUI` tables:

- `Assets/Content/Showcase/Localization/Tables/ShowcaseUI_en.asset`
- `Assets/Content/Showcase/Localization/Tables/ShowcaseUI_ru.asset`

Module and variant **assets** under `Assets/Content/Modules/*/Data` remain the place for **references** (prefabs, variant lists, keys, category). They are not a second copy of localized prose.

Important constraint:

- real showcase assets must assign an explicit `localizationKey`;
- asset renames are authoring changes only and must not be used as a content-management tool;
- `localizationKey` is the runtime identity for localized module and variant copy;
- if you intentionally rename a localization id, you must update the relevant table entries as part of the same change.

Current validation rule:

- `ShowcaseValidator` treats missing `localizationKey` on real `ModuleDefinitionSO` and `VariantDefinitionSO` assets as an error;
- missing required `ShowcaseContent` entries are validator errors (or warnings for optional fields such as WebGL notes);
- required visible content resolves only through tables or explicit `[MISSING: ...]` markers.

When copy changes, edit the string tables (and run `Tools/LearningArchitect/Validate Showcase Configuration`). Editing a `ModuleDefinitionSO` / `VariantDefinitionSO` asset alone does not change visible text.

**Editor scaffolding:** `Learning Architect/Localization/Setup Showcase Localization` ensures locale and table assets exist and can merge keys for `ShowcaseUI`. It does **not** populate `ShowcaseContent` from module/variant assets — those rows stay hand-authored in the string tables.

## Layering

The project is intentionally split into four layers.

### 1. Showcase Shell

Location:

- `Assets/Content/Showcase/Runtime`

Contains:

- selection state;
- runtime coordination;
- spawning;
- lifecycle hosting;
- stress state;
- module asset definitions.

This layer does not care whether the active module is AI, VFX, inventory, or animation.

### 2. Presentation Shell

Location:

- `Assets/Content/Showcase/UI`

Contains:

- hub widgets;
- presenters;
- localization bridge;
- metrics and status hosts, presenters, runtime samplers, formatters, and views;
- navigation;
- guided demo flow.

This layer listens to shared runtime state and never owns domain logic directly.

### 3. Module Layer

Location:

- `Assets/Content/Modules`

Contains:

- module shell components implementing `IModule`;
- variant components implementing `IShowcaseStressTarget`;
- domain-specific runtime behavior for each showcase module.

This is the main extension point of the project.

### 4. Data And Asset Wiring

Location:

- `Assets/Content/Showcase/Prefabs` and `Assets/Content/Modules/*/Data`

Contains:

- module prefabs;
- variant prefabs;
- module definition assets;
- variant definition assets;
- shared showcase visuals.

This layer is what lets the same runtime shell load multiple systems without scene duplication.

### 5. Portfolio Delivery Shell

Location:

- `Site`

Contains:

- bilingual presentation page copy;
- module preview explorer;
- architecture notes for interview reading;
- embedded WebGL host and launch flow;
- build-manifest preparation for static hosting.

This layer does not replace the Unity runtime. It wraps it in a browser-friendly review experience.

## Assembly Boundaries

The project is also split at the assembly level.

- `LearningArchitect.Showcase`
  - shared runtime shell, UI, data definitions, localization, and common contracts.
- `LearningArchitect.Modules.*`
  - feature-owned module implementations such as AI, pooling, inventory, VFX, and update-loop strategies.
- `LearningArchitect.Editor`
  - editor tooling such as validation and layout helpers.
- `*.Tests.Editor`
  - isolated EditMode suites for showcase shell, AI logic, and module contract coverage.

This matters for learning value because the repository teaches not only scene composition, but also how to keep a Unity codebase segmented without overcomplicating the runtime.

## Current Module Taxonomy

The project currently separates modules into two categories.

### Architecture Pattern Modules

These focus on production patterns that cut across multiple domains.

- `Update Loop Strategies`
- `Object Pooling`
- `VFX Delivery`

### Simulation Modules

These focus on domain-style runtime systems.

- `Effects System`
- `AI System`
- `Inventory Systems`
- `Layered Character Animation`

The category is not about importance. It is about the kind of architectural problem the module is teaching.

## Module Design Pattern

Every module follows the same high-level pattern:

1. A `ModuleDefinitionSO` exposes the module to the hub.
2. Each variant has a `VariantDefinitionSO`.
3. Each variant definition points to one prefab.
4. That prefab contains:
   - a module root implementing `IModule`
   - a stress-facing implementation of `IShowcaseStressTarget`
   - a metrics-facing implementation of `IShowcaseMetricsSource`
5. Most variants now author runtime visuals through an assigned `visualPrefab` carrier instead of creating ad-hoc primitives directly inside the module.
6. The variant component owns the actual runtime behavior and visible subset.

This makes module creation predictable and repeatable.

## Runtime Visual Authoring

One of the newer architectural shifts is that module visuals are being pushed into reusable carrier prefabs under:

- `Assets/Content/Showcase/Art/ModuleCarriers`

The pattern is:

1. simulation code decides count, position, and state;
2. `ShowcaseVisualInstanceFactory` instantiates the assigned visual carrier;
3. the module updates only the visible subset and reports metrics separately from simulation scale.

This is important for the educational story because it keeps three concerns visible and separate:

- simulation ownership;
- showcase-facing metrics;
- visual representation used for comparison.

The animation module was the last major holdout here and now also requires a real actor-prefab profile instead of procedural placeholder rigs. The remaining cleanup is mostly about documenting and validating the authoring contract, not about keeping hidden fallback rendering paths alive.

The practical authoring contract for the hub prefab, `DescriptionPanel`, and reusable carriers lives in:

- `docs/HubAndCarrierAuthoring.md`

## Hub UI Authoring

The hub shell currently keeps two truths that matter:

- the shipped visual baseline is the restored hand-authored prefab layout;
- `DescriptionPanel` now follows that shipped single-text viewport path as the actual runtime contract.

In other words, the shipped prefab is no longer competing with a second runtime authoring model. Tooling should preserve the baseline, not reinterpret it.

## WebGL Demo Architecture

The browser version now has one additional architectural constraint:

- the page must still communicate value before Unity finishes loading.

That led to these decisions:

1. The static portfolio page and the Unity WebGL runtime live in the same browser page.
2. Loader assets can be staged early, but the Unity instance is created only on explicit launch.
3. The demo host supports fullscreen so the runtime can become the sole focus when needed.
4. `EN` and `RU` page content are switched independently of the Unity runtime shell.

This is not a generic website around Unity. It is part of the productized presentation of the showcase.

## WebGL Metrics Strategy

Browser delivery still has a presentation constraint:

- WebGL should communicate architectural differences without pretending to be a lab-grade benchmark.

The current strategy is:

- keep live runtime metrics in the shared shell;
- use conservative stress presets that remain readable in a browser;
- rely on the page copy and guided flow to frame what the viewer should compare.

In other words:

- the showcase does not ship a hidden runtime reference-metrics catalog anymore;
- the browser story is carried by the same runtime metrics path plus tighter preset discipline and better surrounding context.

## New Modules Added In This Iteration

### Inventory Systems

Purpose:

- compare object-rich slot ownership against packed slot processing.

Variants:

- `Object Slots`
- `Packed Slots`

Architectural point:

- inventory code often starts as convenient object graphs and later becomes a hot mutation path.

### Layered Character Animation

Purpose:

- show one humanoid character model running, shooting, and combining both through Animator layers.

Variants:

- `Run`
- `Shoot`
- `Run + Shoot`

Architectural point:

- action-game animation often needs composition, not full-body clip replacement. The `Run + Shoot` variant keeps locomotion on the base layer and routes shooting through an upper-body `AvatarMask`.

### VFX Delivery

Purpose:

- compare emitter-local presentation with batched visual feedback delivery.

Variants:

- `Emitter Bursts`
- `Batched Pulses`

Architectural point:

- dense VFX problems are often ownership and rendering problems, not just content problems.

## Why Legacy Was Removed

The repository previously still contained standalone effect demos and a separate VFX scene that were not part of the active showcase runtime.

Those assets were removed because they worked against the current direction:

- they duplicated entry points;
- they were not wired into the shared hub;
- they kept old namespaces, tests, and prefabs alive without participating in the current architecture.

The project now has one architectural story instead of parallel old and new paths.

## Testing And Validation

Current validation approach:

- run `./Verify-Showcase.ps1` for the repo-level build and site-copy checks;
- compile through Unity and `dotnet build`;
- run Unity EditMode tests;
- run `Tools/LearningArchitect/Validate Showcase Configuration` when module data, prefab wiring, or localization tables change;
- verify the active scene and generated module assets;
- verify module switching in play mode through the shared runtime controller.

The current architecture favors integration-level validation because most value is in shared orchestration and wiring, not only in isolated utility methods.

Detailed guidance for test layering, suite categories, and build-safety rules lives in:

- `docs/TestingStrategy.md`

Project continuity notes live in:

- `docs/Decisions.md`
- `docs/OpenThreads.md`
- `docs/WorkingMemory.md`

## Planned Module: Interview Arena

A large planned module (**Interview Arena**) is documented under `docs/Modules/InterviewArena/`.

Intent:

- compact WebGL multiplayer arena (physics movement, lobby, personal server);
- interview-oriented depth (client/server, pooling, FSM AI, vector math);
- integrated through the same hub contracts (`IModule`, stress, localization keys), not as a second entry scene.

Integration assessment and phased rollout:

- `docs/Modules/InterviewArena/INTEGRATION_ASSESSMENT.md`

Runtime code lives under `Assets/Content/Modules/InterviewArena/`. The playable shell is a **separate scene** (`Assets/Content/Modules/InterviewArena/Scenes/InterviewArena.unity`), opened from the hub via `InterviewArenaLaunchDock` and `ShowcaseSceneLoader` — not via the module/variant extension workflow below.

## Extension Workflow

To add a new module correctly:

1. Create a module shell implementing `IModule`.
2. Create one or more variants implementing `IShowcaseStressTarget`.
3. Create variant prefabs.
4. Create `VariantDefinitionSO` assets for those prefabs.
5. Create a `ModuleDefinitionSO` that references the variants.
6. Register the module in `ShowcaseCompositionRoot.Modules`.
7. Assign explicit `localizationKey` values and add matching rows to `ShowcaseContent_en` / `ShowcaseContent_ru` (and any new `ShowcaseUI` keys if you introduce them).
8. Add or adjust `Assets/Content/Showcase/Data/RecruiterDemoScenario.asset` if the new module should appear in the guided walkthrough.

That workflow is the main architectural contract of the repository.
