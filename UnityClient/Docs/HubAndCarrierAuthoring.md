# Hub And Carrier Authoring

This file documents the authoring contract around the main showcase hub prefab and the reusable module carrier prefabs.

Use it when changing:

- `Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`
- `Assets/Content/Showcase/UI/DescriptionPanel.cs`
- `Assets/Content/Showcase/Art/ModuleCarriers`
- variant prefabs that assign `visualPrefab`

## Main Rule

`ArchitectureShowcaseHub.prefab` is the visual source of truth for the showcase shell.

That means:

- do not treat `ShowcaseLayoutTool` as the system that owns the UI composition;
- do not rebuild the hub layout just to "normalize" names or structure;
- prefer small, explicit prefab edits plus validation over automatic layout regeneration.

The public rebuild entrypoints in `ShowcaseLayoutTool` are currently left in safe mode for exactly this reason.
The tool intentionally does not keep a hidden private normalizer or compatibility mutator behind those entrypoints.
If the hub needs structural changes, make them explicitly in the prefab and update validation/tests to match.

## Hub Prefab Contract

The root prefab is:

- `Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab`

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

`DescriptionPanel` uses one shipped viewport layout.

### Legacy baseline layout

Used by the restored hand-authored hub baseline:

- `Viewport`
  - `DescriptionText`

This path is the current runtime and validation contract.

## Important Constraint

The runtime and validator now follow the shipped legacy single-text path.

So:

- do not remove `DescriptionText` from the hub prefab unless you intentionally migrate the visual baseline;
- do not reintroduce alternate `DescriptionPanel` viewport layouts without changing the shipped hub prefab and validator contract at the same time;
- do not expand `ShowcaseLayoutTool` back into a full scene/prefab rebuild path unless the visual baseline ownership model is intentionally changed.

## Tabs Contract

`DescriptionPanel` expects:

- `Container - DescriptionCharacters`
- `ActiveTabUnderline`

The current prefab uses three text labels under the tab bar. Keep that naming/layout unless the visual baseline is intentionally migrated.

## Carrier Prefab Contract

Reusable showcase carriers live under:

- `Assets/Content/Showcase/Art/ModuleCarriers`

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

## Content Identity Contract

Module and variant assets are also part of the authored runtime contract.

That means:

- real `ModuleDefinitionSO` and `VariantDefinitionSO` assets must assign an explicit `localizationKey`;
- `asset.name` is not a valid runtime localization id;
- if a module or variant is renamed in the Project view, the localization identity should stay stable unless you intentionally migrate the localization keys and tables together.

Visible hub copy resolves **only** through localization tables (`ShowcaseContent` for module/variant prose, `ShowcaseUI` for shared chrome including module category labels). ScriptableObjects supply keys and wiring, not a second locale-specific text layer.

## Safe Change Workflow

When changing hub UI or carrier wiring:

1. edit the prefab intentionally;
2. run `Tools/LearningArchitect/Validate Showcase Configuration`;
3. run the relevant EditMode tests;
4. run the thin PlayMode smoke suite if runtime wiring changed;
5. if module or variant **copy** changed, verify the relevant `ShowcaseContent` rows (and any new `ShowcaseUI` keys); if only prefabs, keys, or stress counts changed, verify `ModuleDefinitionSO` / `VariantDefinitionSO` and run the validator;
6. only then update docs if the contract itself changed.
