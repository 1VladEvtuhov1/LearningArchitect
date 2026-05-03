# LearningArchitect Testing Strategy

## Goals

The project is a showcase runtime, not a backend service.

That changes what test coverage is worth paying for:

- protect orchestration and module-switching contracts;
- protect data-driven behavior that can silently regress;
- keep tests cheap enough to run during normal Unity work;
- avoid test-only code leaking into player and WebGL builds.

The testing model should optimize for confidence in shared runtime wiring, not for maximum raw test count.

## Principles

1. Prefer fast EditMode tests for deterministic logic.
2. Use PlayMode tests only for high-value integration paths.
3. Keep Editor-only test code in `Editor` folders or test-only assemblies.
4. Avoid UI screenshot or pixel-perfect assertions as a primary safety net.
5. Treat manual visual verification as complementary, not as the main regression strategy.

## Recommended Test Layers

### 1. Core EditMode Tests

Purpose:

- validate runtime shell contracts and orchestration rules.

Primary targets:

- `ShowcaseCoordinator`
- `ShowcaseRuntimeController`
- `ModuleRuntimeHost`
- `VariantSpawner`
- selection and stress state transitions

Examples:

- switching to the next module respects category boundaries;
- variant selection clamps safely when the module changes;
- stress level changes are forwarded to the active target;
- runtime host survives destroyed or missing module components.

These are the highest-value tests in the repository.

### 2. Module EditMode Tests

Purpose:

- validate deterministic logic inside a module without scene bootstrapping.

Primary targets:

- AI decision models
- packed or data-oriented simulation rules
- inventory state transforms
- pool bookkeeping
- metrics snapshot calculations

Examples:

- a decision model transitions to the expected state;
- a packed inventory operation preserves slot invariants;
- a pooled variant never reports negative active counts.

These should be the bulk of module coverage.

### 3. Presentation EditMode Tests

Purpose:

- validate presenter and formatting behavior that does not require full play mode.

Primary targets:

- `DescriptionPanel`
- presenters bound to `ShowcaseStateHub`
- localization fallback behavior

Examples:

- `DescriptionPanel` hides optional empty sections;
- tab selection produces the expected section set;
- localization falls back to ScriptableObject content when table entries are missing.

These tests are useful, but should stay narrower than core/module tests.

### 4. Thin PlayMode Integration Tests

Purpose:

- verify a small number of end-to-end runtime flows that EditMode tests cannot prove.

Primary targets:

- variant spawn and activation in play mode
- stress preset propagation through the shared runtime shell
- state publication into presenters

Recommended scope:

- only a handful of smoke tests
- no broad duplication of EditMode coverage

Examples:

- loading one module activates exactly one runtime variant;
- stress button flow updates the active variant count;
- changing module/variant updates the shared state hub.

## What Not To Over-Automate

Do not spend much test budget on:

- purely decorative UI layout;
- one-off visual polish checks;
- every prefab field by reflection;
- editor tooling cosmetics;
- module internals that are cheaper to reason about than to maintain in tests.

Those areas are better served by manual inspection and focused smoke checks.

## Category Scheme

The project should use stable NUnit categories so suites can be grouped later in a custom test window.

Recommended initial categories:

- `LearningArchitect.Core.Edit`
- `LearningArchitect.AI.Edit`
- `LearningArchitect.UI.Edit`
- `LearningArchitect.Showcase.Play`

Future module-specific categories:

- `LearningArchitect.Inventory.Edit`
- `LearningArchitect.Pooling.Edit`
- `LearningArchitect.Performance.Edit`
- `LearningArchitect.VFX.Edit`
- `LearningArchitect.Animation.Edit`

Rules:

- category names should be stable;
- one test class should usually belong to one primary suite;
- avoid mixing unrelated modules into one category just to reduce the list size.

## Build And WebGL Impact

This strategy should not materially affect player or WebGL builds if implemented correctly.

Safe rules:

- keep test code in `Editor` folders or test-only assemblies;
- do not place NUnit or Unity Test Framework dependencies into runtime assemblies;
- do not introduce test-only hooks into production code unless the payoff is clear and the hook is harmless in release builds.

If those rules are followed:

- test code stays out of player builds;
- WebGL runtime size and behavior should not materially change;
- the main cost is editor complexity, not player complexity.

## Current Baseline

Current automated coverage is still small:

- `ModuleRuntimeHostTests`
- `AiSimulationTests`

This is acceptable for the current stage, but it means the biggest risk areas are still under-covered:

- coordinator and selection transitions;
- runtime controller behavior;
- localization fallback behavior;
- `DescriptionPanel` content composition;
- stress preset propagation across modules.

## Recommended Next Steps

1. Keep existing tests in EditMode.
2. Add category attributes to current tests.
3. Add core tests for coordinator/runtime-controller selection flow.
4. Add focused `DescriptionPanel` EditMode tests for tab composition and empty-section behavior.
5. Add only 2-4 PlayMode smoke tests for runtime integration.
6. Consider a custom Test Center only after the suite taxonomy is stable.

The project needs a better test model before it needs a heavier test-running framework.
