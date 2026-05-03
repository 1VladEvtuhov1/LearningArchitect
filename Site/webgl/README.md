Place the exported Unity WebGL build here.

Expected structure:

```text
Site/
  webgl/
    Build/
      LearningArchitect.loader.js
      LearningArchitect.framework.js
      LearningArchitect.data
      LearningArchitect.wasm
```

Preferred flow:

1. Export Unity WebGL into `Site/webgl/Build`
2. Run:

```powershell
..\prepare-webgl-site.ps1
```

This writes `build-manifest.json`, and the site will auto-discover the exported file names on load.

The helper also expands compressed WebGL payloads into plain files, so the build can run behind simple local/static servers that do not send Brotli response headers.
