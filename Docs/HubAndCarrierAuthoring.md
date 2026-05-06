# Hub And Carrier Authoring

This file documents the authoring contract around the main showcase hub prefab and the reusable module carrier prefabs.

Use it when changing:

- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`
- `Assets/Showcase/UI/DescriptionPanel.cs`
- `Assets/Showcase/Art/ModuleCarriers`
- variant prefabs that assign `visualPrefab`

## Main Rule

`ArchitectureShowcaseHub.prefab` is the visual source of truth for the showcase shell.

That means:

- do not treat `ShowcaseLayoutTool` as the system that owns the UI composition;
- do not rebuild the hub layout just to "normalize" names or structure;
- prefer small, explicit prefab edits plus validation over automatic layout regeneration.

The public rebuild entrypoints in `ShowcaseLayoutTool` are currently left in safe mode for exactly this reason.

## Hub Prefab Contract

The root prefab is:

- `Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

The root object must keep these responsibilities:

- `ShowcaseCompositionRoot`
- `ShowcaseRuntimeController`
- `ShowcaseCommandRouter`
- `ShowcaseStateHub`
- `ShowcaseTransitionController`
- hub-facing UI presenters and views

The root hierarchy must keep these top-level child roles:

- `ModuleRoot`
  - spawn parent for active module variants
- active `Canvas`
  - the visible shell UI
- disabled secondary `Canvas`
  - kept only if the prefab still needs that authored overlay branch

`ModuleRoot` is required wiring, not a cosmetic object. If it is missing or unassigned in `ShowcaseCompositionRoot`, the runtime will not bootstrap.

## Description Panel Contract

`DescriptionPanel` currently supports two viewport layouts.

### 1. Legacy baseline layout

Used by the restored hand-authored hub baseline:

- `Viewport`
  - `DescriptionText`

This path is still valid and still supported at runtime.

### 2. Composite section layout

Supported for more structured card-style rendering:

- `Viewport`
  - `Container - AboutInfo`
  - `Container - ArchitectureInfo`
  - `Container - Trade-OffsInfo`
  - `Container - ProsCons`
    - `Container - Pros`
    - `Container - Cons`

Each composite section must provide:

- `Text - Header`
- `Text - Description`

## Important Constraint

The runtime and validator now accept both layouts, but the current shipped visual baseline is the legacy single-text path.

So:

- do not remove `DescriptionText` from the hub prefab unless you intentionally migrate the visual baseline;
- do not tighten validator/tooling back to composite-only assumptions without changing the shipped hub prefab at the same time.

## Tabs Contract

`DescriptionPanel` expects:

- `TabsBar` or `Container - DescriptionCharacters`
- `ActiveTabUnderline`

Named tabs like `Tab_0`, `Tab_1`, `Tab_2` are supported, but the runtime can also bind fallback text labels under the tab bar when the old baseline layout is used.

## Carrier Prefab Contract

Reusable showcase carriers live under:

- `Assets/Showcase/Art/ModuleCarriers`

The intent is:

1. module logic owns simulation and stress scaling;
2. `ShowcaseVisualInstanceFactory` instantiates the assigned `visualPrefab`;
3. the carrier only represents the object visually for comparison and teaching clarity.

Carrier prefabs should stay:

- lightweight;
- visually inspectable;
- free from hidden gameplay ownership;
- reusable across multiple variants when that helps the teaching story.

## Variant Prefab Contract

For variants using the carrier path:

- assign `visualPrefab` on the runtime component;
- keep `IModule`, `IShowcaseStressTarget`, and ideally `IShowcaseMetricsSource` on the variant root;
- avoid recreating showcase-facing primitives in code when the same thing can be expressed as a carrier prefab.

For humanoid animation:

- assign `animationProfile`;
- the profile must assign `actorPrefab`.

## Safe Change Workflow

When changing hub UI or carrier wiring:

1. edit the prefab intentionally;
2. run `Tools/LearningArchitect/Validate Showcase Configuration`;
3. run the relevant EditMode tests;
4. run the thin PlayMode smoke suite if runtime wiring changed;
5. only then update docs if the contract itself changed.
