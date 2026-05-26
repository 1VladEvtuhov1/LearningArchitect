# LearningArchitect Docs

This folder is intentionally split into a small set of documents with different roles.

Use this file as the map of what to read first and where specific kinds of information belong.

## Reading Order

If you are new to the repository:

1. `../README.md`
2. `Architecture.md`
3. `HubAndCarrierAuthoring.md`
4. `TestingStrategy.md`
5. `Decisions.md`
6. `OpenThreads.md`
7. `WorkingMemory.md`

If you are returning to active work:

1. `WorkingMemory.md`
2. `OpenThreads.md`
3. `Decisions.md`

If you are preparing delivery or a demo:

1. `WebDeployment.md`
2. `TestingStrategy.md`
3. `Architecture.md`

## Document Roles

### Entry Point

- `../README.md`
  - project overview, current shape, repository layout, and link-out to the deeper docs.

### System Architecture

- `Architecture.md`
  - the main technical picture of the showcase runtime, shell layering, contracts, data flow, module pattern, and browser delivery model.

### Stable Decisions

- `Decisions.md`
  - decisions that are expected to remain relevant across sessions and explain why the repository is shaped this way.

### Active Continuity

- `OpenThreads.md`
  - unfinished work that still needs follow-up.
- `WorkingMemory.md`
  - short-lived operational snapshot for fast re-entry.

### Operations

- `TestingStrategy.md`
  - test layering, suite taxonomy, and the current confidence model.
- `WebDeployment.md`
  - local preview, build preparation, and static hosting workflow.

### Feature-Specific Setup

- `HubAndCarrierAuthoring.md`
  - the prefab and authoring contract for `ArchitectureShowcaseHub`, `DescriptionPanel`, and reusable module carriers.
- `HumanoidAnimationSetup.md`
  - the current setup and regeneration workflow for the layered humanoid animation module.

### Planned Modules (design docs)

- `Modules/InterviewArena/README.md`
  - planned WebGL multiplayer arena module; full spec under `Modules/InterviewArena/docs/`.
- `Modules/InterviewArena/INTEGRATION_ASSESSMENT.md`
  - fit, risks, and phased integration with the existing showcase hub.

## Source-Of-Truth Rules

Use these boundaries to avoid document drift:

- keep `README` short and entrypoint-oriented;
- keep detailed runtime design in `Architecture.md`;
- keep only stable cross-cutting choices in `Decisions.md`;
- keep only unresolved work in `OpenThreads.md`;
- keep only short-lived state in `WorkingMemory.md`;
- keep runbooks and procedures out of architecture docs unless they are part of the architectural story.

### Localization content

- All **visible** module and variant prose is authored in **`ShowcaseContent`** (`ShowcaseContent_en` / `ShowcaseContent_ru`). Shared hub chrome (tabs, headers, module **category** labels derived from `ShowcaseModuleCategory`) lives in **`ShowcaseUI`**.
- `ModuleDefinitionSO` / `VariantDefinitionSO` under `Assets/Modules/*/Data` hold **`localizationKey`**, taxonomy, prefab references, and stress counts only — not duplicate EN/RU strings. Details: `Architecture.md`.

## When To Update Which File

- update `README` when the public-facing shape of the repo changes;
- update `Architecture.md` when runtime topology, layering, contracts, or module patterns change;
- update `Decisions.md` when a new cross-cutting rule becomes intentional and durable;
- update `OpenThreads.md` when a thread becomes active, changes meaningfully, or is resolved;
- update `WorkingMemory.md` when the current working shape changes in a way that matters next session;
- update `TestingStrategy.md` when suite taxonomy, confidence boundaries, or validation rules change;
- update `WebDeployment.md` when the build/deploy flow changes;
- update feature docs like `HumanoidAnimationSetup.md` when their asset/setup contract changes.
- update `HubAndCarrierAuthoring.md` when the hub prefab contract, description-panel layout contract, or carrier-prefab authoring pattern changes.
