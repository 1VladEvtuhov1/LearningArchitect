# LearningArchitect Site

This folder is a standalone static presentation shell for the project.

It is intentionally separate from `UnityClient/Assets/` so it can be hosted as a normal portfolio page while the Unity project keeps its own build and authoring structure.

For the full run, build, and server deployment checklist, see `../UnityClient/Docs/WebDeployment.md`.

## What It Contains

- `index.html`
  - presentation page
  - `EN / RU` language toggle
  - architecture notes
  - module explorer
  - embedded WebGL host section
- `styles.css`
  - site styling
- `app.js`
  - module explorer interactivity
  - background preload flow
  - Unity WebGL launch integration
  - fullscreen button state for the embedded runtime
- `config.js`
  - source URL
  - default WebGL discovery settings
- `content/site-copy.csv`
  - source of truth for all editable site copy
  - contains both `EN` and `RU` columns in one table
- `content/site-copy.runtime.json`
  - generated runtime payload consumed by the page
- `scripts/site-copy-tool.mjs`
  - converts `CSV -> JSON`
  - can also bootstrap the initial CSV from the older embedded `app.js` content
- `assets/images`
  - copied preview screenshots used by the page
- `webgl`
  - expected location for Unity WebGL export
- `prepare-webgl-site.ps1`
  - scans the exported WebGL build
  - writes `webgl/build-manifest.json` for auto-discovery
  - expands compressed `.br/.unityweb` artifacts into plain files for simple static servers
  - keeps one coherent file family together by following the active loader stem

## Local Preview

Use a static server from the repository root:

```powershell
python -m http.server 8080 -d Site
```

Then open:

```text
http://localhost:8080
```

Do not rely on opening `index.html` directly with `file://` if you want to test the Unity loader flow. Serve it over HTTP.

The current local preview used during development is typically:

```powershell
python -m http.server 8081 -d Site
```

## Copy Workflow

The site now uses a strict content pipeline:

1. `content/site-copy.csv`
   - source of truth
2. `content/site-copy.runtime.json`
   - generated runtime payload
3. `app.js`
   - reads the generated JSON only

When you edit site text, change the CSV first and then rebuild the runtime JSON:

```powershell
node .\scripts\site-copy-tool.mjs build-json
```

If you ever need to re-bootstrap the CSV from an older embedded-copy version of `app.js`, run:

```powershell
node .\scripts\site-copy-tool.mjs extract-appjs
```

## How To Plug In The Unity WebGL Build

1. Build the project from Unity for `WebGL`.
2. Export the generated files into:

```text
Site/webgl/Build
```

3. From `Site/`, run:

```powershell
.\prepare-webgl-site.ps1
```

4. Serve `Site/` through static hosting.

The page will auto-discover `webgl/build-manifest.json`. In the common case you no longer need to hand-edit `config.js`.

The helper also expands compressed WebGL payloads into plain `.data/.framework.js/.wasm` files so local preview works even through simple servers like `python -m http.server`, which do not add `Content-Encoding: br` headers automatically.

The helper now also avoids a subtle failure mode where the `Build/` folder contains multiple generations of exports. It chooses the matching data/framework/wasm files by the detected loader stem so the manifest does not mix:

- `webgl.*`
- `WebGLPortfolioBuild.*`

inside the same launch configuration.

## Typical Auto-Generated Manifest

Example:

```json
{
  "enabled": true,
  "webglRoot": "./webgl",
  "buildRoot": "./webgl/Build",
  "loaderFile": "LearningArchitect.loader.js",
  "dataFile": "LearningArchitect.data.unityweb",
  "frameworkFile": "LearningArchitect.framework.js.unityweb",
  "codeFile": "LearningArchitect.wasm.unityweb",
  "streamingAssetsUrl": "./webgl/StreamingAssets",
  "companyName": "LearningArchitect",
  "productName": "LearningArchitect",
  "productVersion": "1.0.0",
  "matchWebGLToCanvasSize": true,
  "devicePixelRatio": 1
}
```

If Unity emits compressed or differently named files, the helper script will pick them up and write the correct manifest.

## Hosting Direction

The intended deployment model is:

1. one presentation page
2. one embedded Unity WebGL client
3. one repository link

That gives interviewers context before launch and avoids dropping them into a blank Unity loader without explanation.

## Current UX Intent

The page is designed for interview review, not as a generic game landing page.

Key expectations:

- the visitor can read before launching the runtime;
- the module explorer previews what the runtime is about to demonstrate;
- the embedded host becomes the proof step after the explanation step;
- the fullscreen action is visually emphasized because it is one of the highest-value review controls;
- some WebGL comparisons may use curated reference metrics so architectural deltas remain readable in browser conditions.
