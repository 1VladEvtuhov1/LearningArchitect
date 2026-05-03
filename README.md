# LearningArchitect

`LearningArchitect` is a Unity architectural showcase for modular real-time systems.

The repository is built around one interactive scene and one shared showcase runtime. Each module is loaded through data assets, rendered inside the same hub, and stress-tested through the same UI flow.

The current delivery model is two-layered:

- a Unity runtime inside `Assets/`
- a standalone portfolio shell in `Site/` that embeds the WebGL build on the same page as the project explanation

## What The Project Is Now

This is no longer a loose collection of isolated demos.

The current project is a data-driven showcase with:

- one active entry scene: `Assets/Showcase/Scenes/ArchitectureShowcase.unity`;
- one shared hub prefab: `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`;
- module selection driven by `ModuleDefinitionSO` and `VariantDefinitionSO`;
- runtime orchestration handled by `ShowcaseCompositionRoot`, `ShowcaseCoordinator`, `ShowcaseRuntimeController`, `ModuleRuntimeHost`, and `ShowcaseStateHub`;
- a consistent stress-test and comparison workflow across every module.

Legacy standalone effect demos and the old separate VFX scene were removed in favor of this unified runtime.

## Current Module Set

The showcase currently contains 7 modules and 15 variants.

### Architecture Pattern Modules

1. `Update Loop Strategies`
   - `Per-Object`
   - `Centralized`
2. `Object Pooling`
   - `Instantiation`
   - `Reusable Pool`
3. `VFX Delivery`
   - `Emitter Bursts`
   - `Batched Pulses`

### Simulation Modules

4. `Effects System`
   - `Indie`
   - `Chunk`
5. `AI System`
   - `FSM`
   - `Utility`
   - `Behavior Tree`
6. `Inventory Systems`
   - `Object Slots`
   - `Packed Slots`
7. `Layered Character Animation`
   - `Run`
   - `Shoot`
   - `Run + Shoot`

## Core Ideas

The project is meant to show how a Unity codebase can evolve from straightforward, readable runtime structures into more explicit and scalable processing patterns.

Recurring themes across the modules:

- separate showcase orchestration from domain implementation;
- keep module loading explicit and bounded;
- compare variants inside the same runtime shell;
- expose stress presets through data assets instead of hardcoding the UI flow around one system;
- prefer centralized ownership when scaling pressure becomes real;
- preserve readability even when moving toward packed or batched execution.

For public browser delivery the project now also prefers:

- one readable presentation page before the runtime is launched;
- stable, representative WebGL metrics over noisy browser benchmarking;
- a guided review flow that works for recruiters and interviewers who will not inspect the Unity scene directly.

## Repository Structure

- `Assets/Showcase/Runtime`
  - showcase orchestration, lifecycle, selection state, spawning, and runtime state publication.
- `Assets/Showcase/UI`
  - hub UI, presenters, navigation, localization bridge, metrics, and guided demo flow.
- `Assets/Modules`
  - concrete showcase modules and their variants.
- `Site`
  - static presentation shell, bilingual landing page, embedded WebGL host, and launch/preload flow for portfolio delivery.
- `Assets/Showcase/Prefabs`
  - hub prefab and showcase-owned shell assets.
- `Assets/Showcase/Scenes`
  - the active showcase scene.
- `Docs`
  - project documentation.

## Content Authoring

Module and variant information is authored in two layers.

- `ModuleDefinitionSO` assets store module-level fallback content:
  - category
  - display name
  - thesis
  - description
  - problem statement
  - WebGL note
  - active item label
  - referenced variants
- `VariantDefinitionSO` assets store variant-level fallback content:
  - display name
  - prefab reference
  - stress presets
  - architecture description
  - comparison summary
  - takeaway
  - trade-offs
  - pros
  - cons

These assets now live in feature-owned folders under `Assets/Modules/*/Data`.

Runtime UI does not read those fields directly first. `ShowcaseLocalization` resolves content through the `ShowcaseContent` localization table and uses the ScriptableObject fields as fallback values when a localized entry is missing.

That means:

- if a key exists in `Assets/Showcase/Localization/Tables/ShowcaseContent_en.asset` or `Assets/Showcase/Localization/Tables/ShowcaseContent_ru.asset`, that text wins;
- if a localized entry is missing, the value from `ModuleDefinitionSO` or `VariantDefinitionSO` is used instead.

When project content appears out of sync, check the localization tables before assuming the ScriptableObject asset is unused.

## How To Open The Showcase

1. Open `Assets/Showcase/Scenes/ArchitectureShowcase.unity`.
2. Press Play.
3. Switch modules and variants through the hub UI or keyboard navigation.
4. Use the stress presets to compare runtime behavior under the same presentation shell.

## Web Portfolio Flow

The portfolio version is intentionally not a naked Unity loader.

It currently works like this:

1. Open the static page in `Site/`.
2. Read the module overview, architecture notes, and guided review flow while the page stages the lightweight loader assets.
3. Launch the embedded WebGL runtime from the same page.
4. Optionally switch the page between `EN` and `RU`.
5. Use the highlighted fullscreen action if the reviewer wants the runtime isolated from the rest of the page.

The embedded demo is meant to support interview review, not to act like a production benchmark harness.

## Documentation

- Project architecture: `Docs/Architecture.md`
- Web runbook and deployment: `Docs/WebDeployment.md`
- Testing strategy: `Docs/TestingStrategy.md`
- Humanoid animation setup: `Docs/HumanoidAnimationSetup.md`
- Active decisions: `Docs/Decisions.md`
- Open threads: `Docs/OpenThreads.md`
- Working memory: `Docs/WorkingMemory.md`
