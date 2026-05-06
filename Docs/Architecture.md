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

- `Assets/Showcase/Scenes/ArchitectureShowcase.unity`

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

- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

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

The showcase is data-driven through two ScriptableObject types.

### `ModuleDefinitionSO`

Contains:

- module category;
- display name;
- thesis;
- description;
- problem statement;
- WebGL note;
- active item label;
- list of variants.

Role:

- defines what a module is in the showcase.

### `VariantDefinitionSO`

Contains:

- variant name;
- prefab reference;
- stress presets;
- architecture description;
- comparison summary;
- takeaway;
- trade-offs;
- pros;
- cons.

Role:

- defines how a specific implementation is loaded and explained.

This means UI text, stress presets, and prefab wiring are attached to data instead of hardcoded scene logic.

## Content Source Of Truth

The project has two authoring layers for descriptive content.

### 1. ScriptableObject Fallback Data

`ModuleDefinitionSO` and `VariantDefinitionSO` contain the authorable fallback fields used by the hub:

- module name
- module thesis
- module description
- module problem statement
- module WebGL note
- variant architecture description
- variant comparison summary
- variant takeaway
- variant trade-offs
- variant pros
- variant cons

These assets are stored in feature-owned data folders under `Assets/Modules/*/Data`.

### 2. Localization Table Overrides

At runtime, the hub resolves text through `ShowcaseLocalization`.

The content lookup path is:

1. try `ShowcaseContent` localization entries;
2. if no valid localized entry exists, fall back to the values stored in the ScriptableObject.

In practice this means the localization tables are the first source of truth for what appears in the UI:

- `Assets/Showcase/Localization/Tables/ShowcaseContent_en.asset`
- `Assets/Showcase/Localization/Tables/ShowcaseContent_ru.asset`

The ScriptableObject fields are still important because they:

- provide fallback content when localization keys are missing;
- make module assets self-describing;
- keep content close to prefab wiring.

When editing module copy, always verify whether the relevant key already exists in `ShowcaseContent`. Changing only the ScriptableObject may not change the visible UI if the localized override is present.

## Layering

The project is intentionally split into four layers.

### 1. Showcase Shell

Location:

- `Assets/Showcase/Runtime`

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

- `Assets/Showcase/UI`

Contains:

- hub widgets;
- presenters;
- localization bridge;
- metrics overlay;
- navigation;
- guided demo flow.

This layer listens to shared runtime state and never owns domain logic directly.

### 3. Module Layer

Location:

- `Assets/Modules`

Contains:

- module shell components implementing `IModule`;
- variant components implementing `IShowcaseStressTarget`;
- domain-specific runtime behavior for each showcase module.

This is the main extension point of the project.

### 4. Data And Asset Wiring

Location:

- `Assets/Showcase/Prefabs` and `Assets/Modules/*/Data`

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

- `Assets/Showcase/Art/ModuleCarriers`

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

- `Docs/HubAndCarrierAuthoring.md`

## Hub UI Authoring

The hub shell currently keeps two truths that matter:

- the shipped visual baseline is the restored hand-authored prefab layout;
- runtime support now accepts both the old single-text `DescriptionPanel` viewport path and the newer composite section path.

In other words, the runtime is more flexible than the current shipped prefab. That flexibility exists to prevent breakage during migration, not to justify automatic layout rebuilding.

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

Browser delivery introduced a presentation tradeoff around performance communication.

The current metrics strategy is split:

- in editor and native development flow, live runtime metrics still matter;
- in the WebGL delivery flow, selected showcase variants can use representative reference values and pre-authored chart samples.

This is currently used for the `Update Loop Strategies` module so that:

- `Per-Object` and `Centralized` differ clearly at a glance;
- browser noise does not erase the intended architectural lesson;
- the graph communicates a stable pattern instead of random loader- and machine-dependent spikes.

The reference metrics are resolved in UI presentation code, not by rewriting the module simulation itself.

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

- compile through Unity and `dotnet build`;
- run Unity EditMode tests;
- run `Tools/LearningArchitect/Validate Showcase Configuration` when module data, prefab wiring, or localization tables change;
- verify the active scene and generated module assets;
- verify module switching in play mode through the shared runtime controller.

The current architecture favors integration-level validation because most value is in shared orchestration and wiring, not only in isolated utility methods.

Detailed guidance for test layering, suite categories, and build-safety rules lives in:

- `Docs/TestingStrategy.md`

Project continuity notes live in:

- `Docs/Decisions.md`
- `Docs/OpenThreads.md`
- `Docs/WorkingMemory.md`

## Extension Workflow

To add a new module correctly:

1. Create a module shell implementing `IModule`.
2. Create one or more variants implementing `IShowcaseStressTarget`.
3. Create variant prefabs.
4. Create `VariantDefinitionSO` assets for those prefabs.
5. Create a `ModuleDefinitionSO` that references the variants.
6. Register the module in `ShowcaseCompositionRoot.Modules`.
7. Add or adjust guided demo steps if the new module should appear in curated navigation.

That workflow is the main architectural contract of the repository.
