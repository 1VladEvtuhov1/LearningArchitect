# WebGL Notes

Project-specific notes for shipping `LearningArchitect` as a lightweight WebGL showcase.

For the operational build and deployment checklist, see `Docs/WebDeployment.md`.

## Goals

- Fast first load for HR / recruiter links.
- Stable runtime on a typical laptop browser.
- Clear interactive demo, not maximum stress numbers.
- Same-page review flow where explanatory content stays visible before runtime launch.

Recommended target:

- `20-30 MB` compressed download: excellent
- `30-50 MB` compressed download: good
- `70+ MB`: worth investigating before sharing broadly

## Current Findings

The main scene build itself is small in scope:

- only `Assets/Showcase/Scenes/ArchitectureShowcase.unity` is in build settings

The most suspicious size contributors in the repository are:

- `Assets/TextMesh Pro` total: about `11.15 MB`
- `Assets/TextMesh Pro/Examples & Extras` total: about `5.87 MB`
- `Assets/TextMesh Pro/Resources` total: about `3.55 MB`
- `Assets/Shared/UI/main_UI.png`: about `1.61 MB` at `1672x941`

Things that are large in the repository but are not automatically a WebGL build problem:

- `Assets/Screenshots` total: about `13.68 MB`
  Move these out of `Assets/` for project cleanliness, but they are only a build problem if referenced.

Things that are not currently a meaningful size problem:

- `Assets/Showcase/Localization`: about `75 KB`
- most icons and frame textures under `Assets/Shared/UI`

## Quick Wins

1. Remove `Assets/TextMesh Pro/Examples & Extras` if the showcase does not use any of those demo assets.
   This is the highest-confidence cleanup in the project.

2. Audit `main_UI.png`.
   If it is a decorative panel/background, try:
   - lower max size
   - texture compression
   - disabling read/write
   - replacing it with a smaller sliced texture if possible

3. Move `Assets/Screenshots` outside `Assets/`.
   This reduces editor/import noise and prevents accidental references.

4. Keep WebGL stress presets conservative.
   Browser demo quality matters more than aggressive headline numbers.

## Package Audit

Packages clearly used by code:

- `com.unity.inputsystem`
- `com.unity.localization`
- `com.unity.render-pipelines.universal`
- `com.unity.ugui`
- `com.unity.textmeshpro` via built-in TMP assets

Packages with no obvious code usage and worth verifying before removal:

- `com.unity.visualscripting`
- `com.unity.timeline`
- `com.unity.ai.navigation`
- `com.unity.multiplayer.center`

Do not remove them blindly. Verify the active scene and prefabs do not rely on them first.

## Recommended WebGL Preset

Use this as the default shipping profile for the current showcase:

- `Compression Format`: `Brotli`
- `Decompression Fallback`: `Off` if hosting serves compressed files correctly
- `Development Build`: `Off`
- `Autoconnect Profiler`: `Off`
- `Managed Stripping Level`: `Medium`
- `Strip Engine Code`: `On`
- `Code Optimization`: compare `Size` and `Speed`, keep the smaller build if runtime stays stable
- `Data Caching`: `On`
- `Exception Support`: keep minimal unless debugging a specific WebGL issue

Memory policy:

- keep startup memory conservative
- prefer the current growth-based model over a huge fixed upfront allocation

## Practical Sequence

1. Remove TMP demo content if unused.
2. Compress or downscale `main_UI.png`.
3. Move screenshots out of `Assets/`.
4. Build with `Brotli`.
5. Generate a build report and check the real top contributors.
6. Only then consider deeper changes like Addressables or asset streaming.

## Current Shipping Model

The WebGL version is now shipped through the `Site/` shell, not as a direct Unity loader page.

Important behavior:

- the static page starts light preload work while the visitor reads the project page;
- the Unity instance is created only when the launch action is used;
- the same page contains module previews, architecture notes, and the embedded runtime;
- the demo host includes an explicit fullscreen action for interview use.

This format is intentionally optimized for portfolio review rather than for raw benchmark fidelity.

## Reference Metrics Policy

The browser demo no longer treats live browser FPS as the primary storytelling layer for every comparison.

For the `Update Loop` module in WebGL:

- representative values are injected for the current preset;
- the chart uses pre-authored samples instead of relying only on noisy live frame history;
- the purpose is to make architectural differences legible during interviews;
- the demo is showing a stable teaching surface, not claiming exact cross-machine benchmark truth.

This is a deliberate tradeoff for browser delivery.

Current curated examples:

- `Per-Object / 25`: `148 FPS`, `6.7 ms`
- `Per-Object / 100`: `86 FPS`, `11.6 ms`
- `Per-Object / 250`: `43 FPS`, `23.3 ms`
- `Centralized / 100`: `166 FPS`, `6.1 ms`
- `Centralized / 1 000`: `124 FPS`, `8.0 ms`
- `Centralized / 5 000`: `72 FPS`, `13.9 ms`

## Build Output Integration

The static page expects a manifest at:

- `Site/webgl/build-manifest.json`

That manifest is generated by:

- `Site/prepare-webgl-site.ps1`

The helper now resolves the active build artifact family using the loader stem, which avoids accidentally mixing old unpacked files with the newest WebGL export.

That matters because the `Build/` folder may contain multiple generations of files such as:

- `webgl.*`
- `WebGLPortfolioBuild.*`

The manifest must point to one coherent family only.

## Positioning

This repository should be shipped as a browser-friendly architecture showcase.

For that goal, the winning tradeoff is:

- smaller build
- faster load
- stable framerate
- clearer demo presets

not:

- maximum raw object count
- complex streaming setup
- premature infrastructure work
