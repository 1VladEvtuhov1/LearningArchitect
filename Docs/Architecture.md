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

Two small interfaces define the runtime contract for every loaded variant.

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
   - a module component implementing `IModule`
   - a variant component implementing `IShowcaseStressTarget`
5. The variant component owns the actual runtime behavior and visible subset.

This makes module creation predictable and repeatable.

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
