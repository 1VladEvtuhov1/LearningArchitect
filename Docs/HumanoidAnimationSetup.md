# LearningArchitect Character Animation Setup

This document describes the current character-animation setup for the showcase module now named `Layered Character Animation`.

The module demonstrates one common game-dev mechanic:

- base locomotion keeps the character moving;
- a combat action can play on the upper body at the same time;
- the `Run + Shoot` variant proves this through an Animator layer and an upper-body `AvatarMask`.

## Source Content

Current source FBX files:

- `Assets/Content/Models/Running.fbx`
- `Assets/Content/Models/Paladin J Nordstrom@Shooting.fbx`

Both are imported as Humanoid rigs. The shooting FBX is used as the visible model because it contains the Paladin mesh. The running FBX provides the locomotion clip and is imported against the shooting avatar so the locomotion retarget stays aligned with the visible rig.

## Generated Showcase Assets

The setup utility creates and updates:

- `Assets/Prefabs/Showcase/Animation_PaladinCombat.controller`
- `Assets/Prefabs/Showcase/Animation_PaladinUpperBody.mask`
- `Assets/Prefabs/Showcase/AnimationActor_PaladinCombat.prefab`
- `Assets/Prefabs/Showcase/Animation_PaladinCombatProfile.asset`
- `Assets/Prefabs/Showcase/AnimationVariant_Run.prefab`
- `Assets/Prefabs/Showcase/AnimationVariant_Shoot.prefab`
- `Assets/Prefabs/Showcase/AnimationVariant_RunShoot.prefab`
- `Assets/Prefabs/Showcase/Animation_RunVariant.asset`
- `Assets/Prefabs/Showcase/Animation_ShootVariant.asset`
- `Assets/Prefabs/Showcase/Animation_RunShootVariant.asset`

The module asset `Assets/Prefabs/Showcase/Animation3DModule.asset` now references these three variants.

## Runtime Variants

- `Run`: base locomotion only.
- `Shoot`: stationary full-body shooting reference.
- `Run + Shoot`: base running plus upper-body shooting layer.

The actor root is intentionally not translated in these variants. The running clip is shown in-place, while the showcase still feeds movement-like `Speed` data into the Animator. This keeps the interview focus on animation composition instead of pathing.

The stress presets are intentionally conservative:

- `Solo` = 1 actor
- `Squad` = 4 actors
- `Crowd` = 8 actors

This module is about readable animation composition, not thousands of skinned meshes.

## Animator Contract

The generated controller uses:

- base layer: `Base Locomotion`
- upper layer: `Upper Body Shooting`
- mask: body, head, arms and fingers enabled; legs disabled

Animator parameters:

- `Speed`
- `MoveY`
- `Turn`
- `IsMoving`
- `IsGrounded`
- `IsShooting`

`HumanoidCrowdActor` writes these parameters from the showcase simulation. `HumanoidAnimationVariant` decides whether the current variant should move, shoot, or do both.

## Regenerating The Setup

If the model or clips are replaced, run the editor utility:

```text
Learning Architect / Setup Paladin Combat Animation
```

The utility lives at:

```text
Assets/Editor/PaladinCombatAnimationSetup.cs
```

It configures FBX import settings, rebuilds the AnimatorController, creates the AvatarMask, assigns URP/Lit materials, creates the actor prefab, and updates the module/variant assets.

## Manual Tuning Points

Safe manual adjustments:

- `Animation_PaladinUpperBody.mask` if shoulders/spine need different blending.
- `Animation_PaladinCombat.controller` transition durations if shooting snaps too hard.
- `AnimationActor_PaladinCombat.prefab` scale/materials if the model presentation needs polish.
- variant prefab `radius`, `moveSpeed`, `visibleCount`, and `visualLimit`.

Avoid manual edits to generated YAML unless debugging a reference issue. Prefer changing the editor setup script and regenerating.
