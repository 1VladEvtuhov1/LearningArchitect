# Web Runbook

This is the practical runbook for running the portfolio site locally, preparing a Unity WebGL build, and deploying the result to a web server.

## Local Site Preview

From the repository root:

```powershell
python -m http.server 8081 -d Site
```

Open:

```text
http://127.0.0.1:8081
```

Do not use `file://` for WebGL checks. Unity loader, manifest fetch, fullscreen, and preload behavior should be tested over HTTP.

## Local Unity Preview

1. Open **`UnityClient/`** in Unity Hub (not the repository root).
2. Open `Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity`.
3. Press Play.
4. Check module switching, variant switching, stress presets, fullscreen-relevant UI, and **language toggle** (Unity Localization: `ShowcaseContent` + `ShowcaseUI` tables — hub copy is not edited on `ModuleDefinitionSO` / `VariantDefinitionSO`).

This verifies the runtime before creating a browser build.

## Manual WebGL Build

Recommended current flow:

1. In Unity, switch target platform to `WebGL`.
2. Ensure only this scene is in the build:

```text
Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity
```

3. Use a release-style WebGL profile:

- `Development Build`: Off
- `Autoconnect Profiler`: Off
- `Compression Format`: Brotli
- `Decompression Fallback`: Off for a properly configured server
- `Managed Stripping Level`: Medium
- `Strip Engine Code`: On
- `Data Caching`: On

4. Build to a temporary folder, for example:

```text
Builds/WebGL
```

5. Copy the generated `Build` folder into:

```text
Site/webgl/Build
```

6. If Unity generated `StreamingAssets`, copy it into:

```text
Site/webgl/StreamingAssets
```

`TemplateData` and Unity's generated `index.html` are not used by the portfolio shell.

## Generate Site Manifest

From `Site/`:

```powershell
.\prepare-webgl-site.ps1
```

The script writes:

```text
Site/webgl/build-manifest.json
```

The site reads this manifest automatically. Normally, `Site/config.js` does not need manual edits.

By default, the script also expands compressed `.br`, `.gz`, or `.unityweb` files into plain `.data`, `.framework.js`, and `.wasm` files. This makes local preview work with simple static servers such as `python -m http.server`.

## Local Build Check

Run:

```powershell
python -m http.server 8081 -d Site
```

Then check:

1. `http://127.0.0.1:8081/?lang=ru`
2. `http://127.0.0.1:8081/?lang=en`
3. The page preloads without creating Unity immediately.
4. `Launch Demo` creates the Unity instance.
5. `Open Fullscreen` is visible and works after launch.
6. The browser console has no missing `loader/data/framework/wasm` requests.

## Deploy To A Static Server

Deploy the whole `Site/` folder as the web root:

```text
Site/
  index.html
  styles.css
  app.js
  config.js
  assets/
  webgl/
    build-manifest.json
    Build/
    StreamingAssets/
```

Preferred approach: treat `Site/webgl/Build` as a deployment artifact, not source code. It can stay ignored in git and be uploaded by the hosting pipeline.

If a hosting provider must deploy from git directly, you may need to force-add ignored build artifacts:

```powershell
git add -f Site/webgl/build-manifest.json Site/webgl/Build Site/webgl/StreamingAssets
```

Only do that if the hosting model requires it. Otherwise keep WebGL build output out of normal commits.

## Nginx Server Option

For the simple expanded-file mode produced by `prepare-webgl-site.ps1`, a normal static Nginx setup is enough:

```nginx
server {
    listen 80;
    server_name example.com;
    root /var/www/learningarchitect;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location ~* \.(wasm|data|js|json|png|jpg|jpeg|webp|css)$ {
        add_header Cache-Control "public, max-age=31536000, immutable";
        try_files $uri =404;
    }

    location = /index.html {
        add_header Cache-Control "no-cache";
    }

    location = /config.js {
        add_header Cache-Control "no-cache";
    }

    location = /webgl/build-manifest.json {
        add_header Cache-Control "no-cache";
    }
}
```

If you decide to serve Brotli files directly, run:

```powershell
.\prepare-webgl-site.ps1 -KeepCompressedArtifactsOnly
```

Then configure correct headers for compressed Unity files. Example:

```nginx
location ~ \.wasm\.br$ {
    default_type application/wasm;
    add_header Content-Encoding br;
    add_header Cache-Control "public, max-age=31536000, immutable";
}

location ~ \.data\.br$ {
    default_type application/octet-stream;
    add_header Content-Encoding br;
    add_header Cache-Control "public, max-age=31536000, immutable";
}

location ~ \.framework\.js\.br$ {
    default_type application/javascript;
    add_header Content-Encoding br;
    add_header Cache-Control "public, max-age=31536000, immutable";
}
```

Do not use `-KeepCompressedArtifactsOnly` unless the server sends the correct `Content-Encoding` and MIME headers. Without those headers, the Unity loader will fail.

## Future CI Or Server Build

The repository does not currently contain a dedicated Unity batch build script. For real server-side builds, add an editor script such as:

```text
Assets/Content/Editor/WebGLBuildScript.cs
```

It should call `BuildPipeline.BuildPlayer` with:

- `Assets/Content/Showcase/Scenes/ArchitectureShowcase.unity`
- target `BuildTarget.WebGL`
- output path such as `Builds/WebGL`
- non-development build options

Then the CI/server needs:

- Unity Editor matching the project version
- WebGL Build Support module installed
- license activation configured through CI secrets or a machine license
- Node.js available if `prepare-webgl-site.ps1` needs Brotli decompression
- PowerShell available to run `Site/prepare-webgl-site.ps1`

Expected CI sequence:

```text
1. Checkout repository.
2. Restore Unity project dependencies.
3. Run Unity in batchmode to build WebGL into Builds/WebGL.
4. Copy Builds/WebGL/Build to Site/webgl/Build.
5. Copy Builds/WebGL/StreamingAssets to Site/webgl/StreamingAssets if present.
6. Run Site/prepare-webgl-site.ps1.
7. Upload Site/ as the static hosting artifact.
```

Until that editor build script exists, the reliable process is manual Unity build plus static site deployment.

## After Every Future Build

Before sharing a public link:

1. Re-run `Site/prepare-webgl-site.ps1`.
2. Serve `Site/` locally over HTTP.
3. Test `RU` and `EN`.
4. Launch WebGL from the page, not directly from Unity's generated `index.html`.
5. Confirm fullscreen button is visible.
6. Check browser devtools for missing build files.
7. If the build feels slow, inspect build size before changing site code.

## What To Hand-Edit If Needed

Safe manual edits:

- `Site/config.js` for GitHub/source link or custom manifest path.
- `Site/index.html`, `styles.css`, `app.js` for portfolio copy and layout.
- `Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab` for UI placement and alignment.

Avoid manual edits:

- generated Unity WebGL loader files;
- generated `.data`, `.wasm`, `.framework.js` files;
- generated `build-manifest.json`, unless debugging a path issue.
