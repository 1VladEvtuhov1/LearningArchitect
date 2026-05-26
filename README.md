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

The showcase currently contains 7 modules and 16 variants.

Active module groups:

- architecture-pattern modules: `Update Loop Strategies`, `Object Pooling`, `VFX Delivery`
- simulation modules: `Effects System`, `AI System`, `Inventory Systems`, `Layered Character Animation`

The full module taxonomy and per-variant breakdown live in `Docs/Architecture.md`.

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
- live runtime metrics with conservative browser-safe presets instead of treating WebGL as a benchmark harness;
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

Module and variant **wiring** (`ModuleDefinitionSO` / `VariantDefinitionSO`) lives under `Assets/Modules/*/Data` and holds `localizationKey`, taxonomy, prefab references, and stress counts. **All visible prose** is authored in Unity Localization:

- `ShowcaseContent` — per-module and per-variant strings (EN/RU tables);
- `ShowcaseUI` — shared hub chrome and fixed labels (including module **category** line keys derived from `ShowcaseModuleCategory`).

Localization identity is explicit:

- `ModuleDefinitionSO.localizationKey` and `VariantDefinitionSO.localizationKey` define the stable lookup id used by the runtime;
- asset names are authoring concerns and should not be treated as localization ids.

If UI text is wrong or missing markers appear, edit the string tables and keys first. The menu `Learning Architect/Localization/Setup Showcase Localization` can seed or repair table scaffolding but does not replace hand-authored `ShowcaseContent` rows.

The full content rules live in `Docs/Architecture.md`.

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

- Documentation index and reading order: `Docs/README.md`
- Project architecture: `Docs/Architecture.md`
- Web runbook and deployment: `Docs/WebDeployment.md`
- Testing strategy: `Docs/TestingStrategy.md`
- Humanoid animation setup: `Docs/HumanoidAnimationSetup.md`
- Active decisions: `Docs/Decisions.md`
- Open threads: `Docs/OpenThreads.md`
- Working memory: `Docs/WorkingMemory.md`

## Tooling Notes

- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab` is the runtime UI source of truth. `Assembly-CSharp.csproj` is Unity-generated and should not be read as an architecture document.
- `MSB3277` warnings involving `System.Net.Http` or `System.IO.Compression` currently come from Unity-generated project references and external editor assemblies, not from showcase runtime logic.
- occasional disposed `NetworkStream` logs from `MCPForUnity` should be treated as external tooling noise unless they map to a reproducible project defect.
