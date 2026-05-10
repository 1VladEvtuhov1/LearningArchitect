window.LearningArchitectSiteConfig = {
  githubUrl: "https://github.com/1VladEvtuhov1/LearningArchitect",
  demo: {
    autoDiscover: true,
    manifestUrl: "./webgl/build-manifest.json",
    webglRoot: "./webgl",
    enabled: false,
    buildRoot: "./webgl/Build",
    loaderFile: "LearningArchitect.loader.js",
    dataFile: "LearningArchitect.data",
    frameworkFile: "LearningArchitect.framework.js",
    codeFile: "LearningArchitect.wasm",
    streamingAssetsUrl: "./webgl/StreamingAssets",
    companyName: "LearningArchitect",
    productName: "LearningArchitect",
    productVersion: "1.0.0",
    matchWebGLToCanvasSize: true,
    devicePixelRatio: 1.25,
    defaultQualityMode: "balanced",
    maxDevicePixelRatio: 1.5,
    qualityModes: {
      performance: {
        width: 1280,
        height: 720,
        maxDpr: 1.0
      },
      balanced: {
        width: 1600,
        height: 900,
        maxDpr: 1.25
      },
      quality: {
        width: 1920,
        height: 1080,
        maxDpr: 1.5
      }
    }
  }
};
