(function () {
  const siteConfig = window.LearningArchitectSiteConfig || {};
  const descriptionMeta = document.querySelector('meta[name="description"]');
  const demoDefaults = {
    autoDiscover: true,
    manifestUrl: "./webgl/build-manifest.json",
    webglRoot: "./webgl",
    enabled: false,
    buildRoot: "./webgl/Build",
    streamingAssetsUrl: "./webgl/StreamingAssets",
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
  };

  siteConfig.demo = Object.assign({}, demoDefaults, siteConfig.demo || {});

  const dom = {
    sourceLink: document.getElementById("source-link"),
    footerSourceLink: document.getElementById("footer-source-link"),
    navModules: document.getElementById("nav-modules"),
    navArchitecture: document.getElementById("nav-architecture"),
    navDemo: document.getElementById("nav-demo"),
    langEn: document.getElementById("lang-en"),
    langRu: document.getElementById("lang-ru"),
    moduleSelector: document.getElementById("module-selector"),
    moduleCategory: document.getElementById("module-category"),
    moduleTitle: document.getElementById("module-title"),
    moduleSummary: document.getElementById("module-summary"),
    moduleVariants: document.getElementById("module-variants"),
    moduleFocus: document.getElementById("module-focus"),
    moduleTry: document.getElementById("module-try"),
    moduleSignal: document.getElementById("module-signal"),
    moduleImage: document.getElementById("module-image"),
    moduleCaption: document.getElementById("module-caption"),
    statusStrip: document.querySelector(".status-strip"),
    preloadPill: document.getElementById("preload-pill"),
    preloadCopy: document.getElementById("preload-copy"),
    launchButton: document.getElementById("launch-demo"),
    presentationButton: document.getElementById("toggle-presentation"),
    qualityLabel: document.getElementById("demo-quality-label"),
    qualityCopy: document.getElementById("demo-quality-copy"),
    qualityNote: document.getElementById("demo-quality-note"),
    qualityPerformance: document.getElementById("quality-performance"),
    qualityBalanced: document.getElementById("quality-balanced"),
    qualityQuality: document.getElementById("quality-quality"),
    demoStage: document.getElementById("demo-stage"),
    demoSupporting: document.getElementById("demo-supporting"),
    demoConsole: document.querySelector(".demo-console"),
    demoContent: document.querySelector(".content"),
    demoProgress: document.querySelector(".demo-progress"),
    demoProgressBar: document.getElementById("demo-progress-bar"),
    posterStatus: document.getElementById("poster-status"),
    demoPoster: document.getElementById("demo-poster"),
    demoFrame: document.getElementById("demo-frame"),
    demoSection: document.getElementById("interactive-demo"),
    jumpToDemo: document.getElementById("jump-to-demo"),
    unityCanvas: document.getElementById("unity-canvas")
  };

  const runtimeCopyUrl = "./content/site-copy.runtime.json";
  let translations = null;
  let modules = [];

  const demoState = {
    phase: "idle",
    preloaded: false,
    loaderReady: false,
    scriptInjected: false,
    launchStarted: false,
    observerAttached: false,
    unityInstance: null,
    presentationActive: false,
    progress: 6,
    runtimePercent: 0,
    errorMessage: ""
  };

  const requestedLanguage = new URLSearchParams(window.location.search).get("lang");
  const qualityModeStorageKey = "learningArchitectSiteQualityMode";
  let currentLanguage = requestedLanguage === "ru" || requestedLanguage === "en"
    ? requestedLanguage
    : (window.localStorage.getItem("learningArchitectSiteLanguage") === "ru" ? "ru" : "en");
  let selectedModuleId = "";
  let currentQualityMode = normalizeQualityMode(window.localStorage.getItem(qualityModeStorageKey) || siteConfig.demo.defaultQualityMode);

  function setText(id, value) {
    const node = document.getElementById(id);
    if (node)
      node.textContent = value;
  }

  function setHtml(id, value) {
    const node = document.getElementById(id);
    if (node)
      node.innerHTML = value;
  }

  function getQualityModes() {
    return siteConfig.demo && siteConfig.demo.qualityModes
      ? siteConfig.demo.qualityModes
      : demoDefaults.qualityModes;
  }

  function getQualityProfile(mode) {
    const modes = getQualityModes();
    return modes[normalizeQualityMode(mode)] || modes.balanced || demoDefaults.qualityModes.balanced;
  }

  function normalizeQualityMode(mode) {
    const modes = getQualityModes();
    return Object.prototype.hasOwnProperty.call(modes, mode) ? mode : "balanced";
  }

  function isRuntimeLiveOrLaunching() {
    return demoState.phase === "loadingRuntime" || demoState.phase === "runtimeLive";
  }

  function getEffectiveDevicePixelRatio() {
    const demo = siteConfig.demo || {};
    const profile = getQualityProfile(currentQualityMode);
    const requested = Math.max(1, Number(profile.maxDpr || demo.devicePixelRatio || 1) || 1);
    const maxDevicePixelRatio = Math.max(1, Number(demo.maxDevicePixelRatio || 1.5) || 1.5);
    const nativeDevicePixelRatio = Math.max(1, Number(window.devicePixelRatio || 1) || 1);
    return Math.min(requested, maxDevicePixelRatio, nativeDevicePixelRatio);
  }

  class RenderController {
    constructor(canvas, container) {
      this.canvas = canvas;
      this.container = container;
      this.currentDpr = 1;
      this.currentWidth = 0;
      this.currentHeight = 0;
    }

    getContainerMetrics() {
      if (!this.container) {
        return {
          width: 1280,
          height: 720
        };
      }

      const styles = window.getComputedStyle(this.container);
      const paddingX = parseFloat(styles.paddingLeft || "0") + parseFloat(styles.paddingRight || "0");
      const paddingY = parseFloat(styles.paddingTop || "0") + parseFloat(styles.paddingBottom || "0");

      return {
        width: Math.max(320, this.container.clientWidth - paddingX),
        height: Math.max(180, this.container.clientHeight - paddingY)
      };
    }

    update() {
      if (!this.canvas)
        return;

      const profile = getQualityProfile(currentQualityMode);
      const visualBox = this.getContainerMetrics();
      const dpr = getEffectiveDevicePixelRatio();
      const scale = Math.min(
        visualBox.width / profile.width,
        visualBox.height / profile.height
      );
      const safeScale = Number.isFinite(scale) && scale > 0 ? Math.min(scale, 1) : 1;
      const displayWidth = Math.floor(profile.width * safeScale);
      const displayHeight = Math.floor(profile.height * safeScale);

      this.currentDpr = dpr;
      this.currentWidth = displayWidth;
      this.currentHeight = displayHeight;

      this.canvas.style.width = displayWidth + "px";
      this.canvas.style.height = displayHeight + "px";
      this.canvas.dataset.qualityMode = currentQualityMode;
      this.canvas.dataset.renderWidth = String(displayWidth);
      this.canvas.dataset.renderHeight = String(displayHeight);
      this.canvas.dataset.renderDpr = String(dpr);
    }
  }

  const renderController = new RenderController(dom.unityCanvas, dom.demoFrame);

  function renderQualityMode() {
    const copy = getTranslation();
    const buttons = [
      { mode: "performance", node: dom.qualityPerformance, text: copy.sections.demoQualityPerformance },
      { mode: "balanced", node: dom.qualityBalanced, text: copy.sections.demoQualityBalanced },
      { mode: "quality", node: dom.qualityQuality, text: copy.sections.demoQualityQuality }
    ];

    if (dom.qualityLabel)
      dom.qualityLabel.textContent = copy.sections.demoQualityLabel;
    if (dom.qualityCopy)
      dom.qualityCopy.textContent = copy.sections.demoQualityCopy;
    if (dom.qualityNote)
      dom.qualityNote.textContent = isRuntimeLiveOrLaunching()
        ? copy.sections.demoQualityNoteLive
        : copy.sections.demoQualityNote;
    if (dom.presentationButton) {
      dom.presentationButton.textContent = demoState.presentationActive
        ? copy.sections.demoPresentationExit
        : copy.sections.demoPresentationEnter;
      dom.presentationButton.setAttribute("aria-pressed", demoState.presentationActive ? "true" : "false");
    }

    buttons.forEach(function (entry) {
      if (!entry.node)
        return;

      entry.node.textContent = entry.text;
      entry.node.classList.toggle("is-active", currentQualityMode === entry.mode);
      entry.node.setAttribute("aria-pressed", currentQualityMode === entry.mode ? "true" : "false");
    });
  }

  function setQualityMode(mode) {
    currentQualityMode = normalizeQualityMode(mode);
    window.localStorage.setItem(qualityModeStorageKey, currentQualityMode);
    renderController.update();
    renderQualityMode();
  }

  function getTranslation() {
    if (!translations)
      throw new Error("Site copy has not been loaded yet.");

    const localizedCopy = translations[currentLanguage] || translations.en;
    if (!localizedCopy || !localizedCopy.site)
      throw new Error(`Missing site copy for language '${currentLanguage}'.`);

    return localizedCopy.site;
  }

  function loadSiteCopy() {
    return fetch(runtimeCopyUrl, { cache: "no-store" })
      .then(function (response) {
        if (!response.ok)
          throw new Error(`Unable to load site copy from ${runtimeCopyUrl}.`);

        return response.json();
      })
      .then(function (content) {
        if (!content || !content.translations || !content.translations.en || !content.translations.ru)
          throw new Error("Site copy JSON is missing translations.en or translations.ru.");

        if (!Array.isArray(content.modules) || content.modules.length === 0)
          throw new Error("Site copy JSON does not define any modules.");

        translations = content.translations;
        modules = content.modules;
        selectedModuleId = modules[0].id;
      });
  }

  function initializeLinks() {
    const githubUrl = siteConfig.githubUrl || "#";
    if (dom.sourceLink)
      dom.sourceLink.href = githubUrl;
    const topbarSourceLink = document.getElementById("topbar-source-link");
    if (topbarSourceLink)
      topbarSourceLink.href = githubUrl;
    if (dom.footerSourceLink)
      dom.footerSourceLink.href = githubUrl;
  }

  function renderStaticCopy() {
    const copy = getTranslation();
    document.documentElement.lang = copy.htmlLang;
    document.title = copy.title;
    if (descriptionMeta)
      descriptionMeta.setAttribute("content", copy.description);

    setText("nav-modules", copy.nav.modules);
    setText("nav-architecture", copy.nav.architecture);
    setText("nav-demo", copy.nav.demo);

    setText("hero-eyebrow", copy.hero.eyebrow);
    setText("hero-title", copy.hero.title);
    setText("hero-summary", copy.hero.summary);
    setText("jump-to-demo", copy.hero.demoCta);
    setText("cta-architecture", copy.hero.architectureCta);
    setText("source-link", copy.hero.sourceCta);
    setText("topbar-source-link", copy.hero.sourceCta);
    setText("hero-chip-1", copy.hero.chips[0]);
    setText("hero-chip-2", copy.hero.chips[1]);
    setText("hero-chip-3", copy.hero.chips[2]);
    setText("hero-chip-4", copy.hero.chips[3]);
    setText("hero-demo-badge-label", copy.hero.demoBadgeLabel);
    setText("hero-demo-badge-value", copy.hero.demoBadgeValue);
    setText("hero-panel-label", copy.hero.panelLabel);
    setText("hero-point-1", copy.hero.points[0]);
    setText("hero-point-2", copy.hero.points[1]);
    setText("hero-point-3", copy.hero.points[2]);
    setText("stat-modules", copy.hero.stats.modules);
    setText("stat-variants", copy.hero.stats.variants);
    setText("stat-runtime", copy.hero.stats.runtime);
    setText("stat-languages", copy.hero.stats.languages);

    setText("section-modules-eyebrow", copy.sections.modulesEyebrow);
    setText("section-modules-title", copy.sections.modulesTitle);
    setText("section-modules-copy", copy.sections.modulesCopy);
    setText("module-label-variants", copy.sections.moduleLabels.variants);
    setText("module-label-focus", copy.sections.moduleLabels.focus);
    setText("module-label-try", copy.sections.moduleLabels.try);
    setText("module-label-signal", copy.sections.moduleLabels.signal);

    setText("section-architecture-eyebrow", copy.sections.architectureEyebrow);
    setText("section-architecture-title", copy.sections.architectureTitle);
    setText("section-architecture-copy", copy.sections.architectureCopy);
    copy.sections.stack.forEach(function (item, index) {
      setText("stack-" + (index + 1) + "-title", item.title);
      setText("stack-" + (index + 1) + "-copy", item.copy);
    });
    setText("brief-runtime-label", copy.sections.runtimeStoryLabel);
    copy.sections.runtimeStory.forEach(function (item, index) {
      setText("brief-runtime-" + (index + 1), item);
    });
    setText("brief-review-label", copy.sections.reviewLabel);
    setText("brief-review-copy", copy.sections.reviewCopy);
    setText("section-review-eyebrow", copy.sections.reviewEyebrow);
    setText("section-review-title", copy.sections.reviewTitle);
    setText("section-review-copy", copy.sections.reviewFocusCopy);
    copy.sections.reviewSteps.forEach(function (item, index) {
      setText("review-step-" + (index + 1) + "-title", item.title);
      setText("review-step-" + (index + 1) + "-copy", item.copy);
    });
    setText("integrity-label", copy.sections.integrityLabel);
    setText("integrity-copy", copy.sections.integrityCopy);

    setText("section-demo-eyebrow", copy.sections.demoEyebrow);
    setText("section-demo-title", copy.sections.demoTitle);
    setText("section-demo-copy", copy.sections.demoCopy);
    setText("demo-inline-copy", copy.sections.demoInlineCopy);
    setText("demo-console-label", copy.sections.demoLabel);
    setText("cta-read-notes", copy.sections.demoNotesCta);
    setHtml("demo-note-1", copy.sections.demoNote1);
    setHtml("demo-note-2", copy.sections.demoNote2);
    setText("poster-label", copy.sections.posterLabel);
    setText("poster-title", copy.sections.posterTitle);

    setText("footer-copy", copy.sections.footerCopy);
    setText("footer-top", copy.sections.footerTop);
    setText("footer-architecture", copy.sections.footerArchitecture);
    setText("footer-source-link", copy.sections.footerSource);

    if (dom.moduleSelector)
      dom.moduleSelector.setAttribute("aria-label", copy.nav.modules);
    if (dom.langEn) {
      dom.langEn.classList.toggle("is-active", currentLanguage === "en");
      dom.langEn.setAttribute("aria-pressed", currentLanguage === "en" ? "true" : "false");
    }
    if (dom.langRu) {
      dom.langRu.classList.toggle("is-active", currentLanguage === "ru");
      dom.langRu.setAttribute("aria-pressed", currentLanguage === "ru" ? "true" : "false");
    }

    renderQualityMode();
  }

  function renderModuleSelector() {
    if (!dom.moduleSelector || modules.length === 0)
      return;

    const localizedNav = getTranslation().nav;
    dom.moduleSelector.innerHTML = "";

    modules.forEach(function (module, index) {
      const button = document.createElement("button");
      const localized = module[currentLanguage];
      button.type = "button";
      button.className = "module-tab";
      button.setAttribute("role", "tab");
      button.setAttribute("aria-selected", module.id === selectedModuleId ? "true" : "false");
      button.dataset.moduleId = module.id;
      button.innerHTML = "<strong>" + localized.title + "</strong><small>" + localized.variants + "</small>";
      button.addEventListener("click", function () {
        selectedModuleId = module.id;
        renderModuleSelector();
        renderSelectedModule();
      });
      if (index === 0)
        button.setAttribute("tabindex", "0");
      dom.moduleSelector.appendChild(button);
    });

    dom.moduleSelector.setAttribute("aria-label", localizedNav.modules);
  }

  function renderSelectedModule() {
    if (!dom.moduleCategory || !dom.moduleTitle || !dom.moduleSummary || !dom.moduleVariants || !dom.moduleFocus || !dom.moduleTry || !dom.moduleSignal || !dom.moduleImage || !dom.moduleCaption || modules.length === 0)
      return;

    const module = modules.find(function (candidate) {
      return candidate.id === selectedModuleId;
    }) || modules[0];
    const localized = module[currentLanguage];

    dom.moduleCategory.textContent = localized.category;
    dom.moduleTitle.textContent = localized.title;
    dom.moduleSummary.textContent = localized.summary;
    dom.moduleVariants.textContent = localized.variants;
    dom.moduleFocus.textContent = localized.focus;
    dom.moduleTry.textContent = localized.try;
    dom.moduleSignal.textContent = localized.signal;
    dom.moduleImage.src = localized.image;
    dom.moduleImage.alt = localized.title + " preview";
    dom.moduleCaption.textContent = localized.caption;
  }

  function applyDemoConfig(overrides) {
    if (!overrides)
      return;

    siteConfig.demo = Object.assign({}, siteConfig.demo || {}, overrides);
  }

  function loadDemoManifest() {
    const demo = siteConfig.demo || {};

    if (demo.autoDiscover === false || !demo.manifestUrl)
      return Promise.resolve();

    return fetch(demo.manifestUrl, { cache: "no-store" })
      .then(function (response) {
        if (!response.ok)
          return null;

        return response.json();
      })
      .then(function (manifest) {
        if (!manifest)
          return;

        applyDemoConfig(manifest);
      })
      .catch(function () {
      });
  }

  function ensureDemoConfigured() {
    if (isDemoConfigured())
      return Promise.resolve(true);

    return loadDemoManifest().then(function () {
      return isDemoConfigured();
    });
  }

  function isDemoConfigured() {
    const demo = siteConfig.demo || {};
    return demo.enabled &&
      demo.buildRoot &&
      demo.loaderFile &&
      demo.dataFile &&
      demo.frameworkFile &&
      demo.codeFile;
  }

  function deriveWebGlRoot() {
    const demo = siteConfig.demo || {};
    const explicitRoot = (demo.webglRoot || "").replace(/\/+$/, "");

    if (explicitRoot)
      return explicitRoot;

    const buildRoot = (demo.buildRoot || "").replace(/\/+$/, "");
    const slashIndex = buildRoot.lastIndexOf("/");
    return slashIndex > 0 ? buildRoot.slice(0, slashIndex) : ".";
  }

  function resolveBuildUrl(fileName) {
    const demo = siteConfig.demo || {};
    const buildRoot = (demo.buildRoot || "").replace(/\/+$/, "");
    return buildRoot + "/" + fileName;
  }

  function resolveStreamingAssetsUrl() {
    const demo = siteConfig.demo || {};
    const candidate = demo.streamingAssetsUrl || "StreamingAssets";

    if (/^(?:[a-z]+:)?\/\//i.test(candidate) || candidate.startsWith("/") || candidate.startsWith("."))
      return candidate;

    return deriveWebGlRoot() + "/" + candidate.replace(/^\/+/, "");
  }

  function syncPresentationState() {
    document.body.classList.toggle("is-presentation-active", demoState.presentationActive);
    if (dom.demoContent)
      dom.demoContent.classList.toggle("is-presentation", demoState.presentationActive);
    if (dom.demoFrame)
      dom.demoFrame.classList.toggle("is-presentation", demoState.presentationActive);
    renderController.update();
    renderQualityMode();
  }

  function togglePresentationView() {
    demoState.presentationActive = !demoState.presentationActive;
    syncPresentationState();
    window.setTimeout(function () {
      renderController.update();
    }, 20);
  }

  function handleGlobalKeyDown(event) {
    if (event.key === "Escape" && demoState.presentationActive) {
      demoState.presentationActive = false;
      syncPresentationState();
    }
  }

  function stagePrefetchLink(url, rel, asValue) {
    const link = document.createElement("link");
    link.rel = rel;
    link.href = url;
    if (asValue)
      link.as = asValue;
    document.head.appendChild(link);
  }

  function renderDemoState() {
    if (!dom.preloadPill || !dom.preloadCopy || !dom.statusStrip || !dom.demoStage || !dom.demoSupporting || !dom.demoProgressBar || !dom.demoProgress || !dom.demoConsole || !dom.posterStatus || !dom.launchButton)
      return;

    const copy = getTranslation().demoPhases;
    const phaseCopy = copy[demoState.phase];
    if (!phaseCopy)
      return;

    dom.preloadPill.textContent = phaseCopy.pill;
    dom.preloadCopy.textContent = phaseCopy.pillCopy;
    dom.statusStrip.classList.toggle("is-loading", demoState.phase === "preloading" || demoState.phase === "loadingRuntime");
    dom.demoStage.textContent = demoState.phase === "loadingRuntime"
      ? copy.runtimeProgress.replace("{value}", String(demoState.runtimePercent))
      : phaseCopy.stage;
    dom.demoSupporting.textContent = phaseCopy.supporting;
    dom.demoProgressBar.style.width = Math.max(4, Math.min(100, demoState.progress)) + "%";
    dom.demoProgress.classList.toggle("is-active", demoState.phase === "preloading" || demoState.phase === "loadingRuntime");
    dom.demoConsole.classList.toggle("is-pending", demoState.phase === "buildPending");
    dom.demoConsole.classList.toggle("is-ready", demoState.phase === "preloadReady");
    dom.demoConsole.classList.toggle("is-live", demoState.phase === "runtimeLive");

    if (demoState.phase === "launchFailed") {
      dom.posterStatus.innerHTML = phaseCopy.posterPrefix + escapeHtml(demoState.errorMessage || "Unknown error.") + phaseCopy.posterSuffix;
    } else {
      dom.posterStatus.innerHTML = phaseCopy.poster;
    }

    dom.launchButton.textContent = phaseCopy.button;

    const shouldDisableButton = demoState.phase === "loadingRuntime" ||
      demoState.phase === "runtimeLive";
    dom.launchButton.disabled = shouldDisableButton;
    renderQualityMode();
  }

  function setDemoPhase(phase, options) {
    demoState.phase = phase;
    demoState.progress = options && typeof options.progress === "number" ? options.progress : demoState.progress;
    demoState.runtimePercent = options && typeof options.runtimePercent === "number" ? options.runtimePercent : demoState.runtimePercent;
    demoState.errorMessage = options && options.errorMessage ? options.errorMessage : demoState.errorMessage;
    renderDemoState();
  }

  function prepareDemoAssets(reason) {
    if (reason === "cta")
      dom.demoSection.scrollIntoView({ behavior: "smooth", block: "start" });

    return ensureDemoConfigured().then(function (configured) {
      if (!configured) {
        demoState.preloaded = false;
        setDemoPhase("buildPending", { progress: 6 });
        return false;
      }

      if (demoState.preloaded)
        return true;

      demoState.preloaded = true;
      setDemoPhase("preloading", { progress: 18 });

      const loaderUrl = resolveBuildUrl(siteConfig.demo.loaderFile);
      const dataUrl = resolveBuildUrl(siteConfig.demo.dataFile);
      const frameworkUrl = resolveBuildUrl(siteConfig.demo.frameworkFile);
      const codeUrl = resolveBuildUrl(siteConfig.demo.codeFile);

      stagePrefetchLink(loaderUrl, "preload", "script");
      stagePrefetchLink(dataUrl, "prefetch");
      stagePrefetchLink(frameworkUrl, "prefetch");
      stagePrefetchLink(codeUrl, "prefetch");

      return fetch(loaderUrl, { cache: "force-cache" })
        .then(function (response) {
          if (!response.ok)
            throw new Error("Loader fetch failed.");

          demoState.loaderReady = true;
          setDemoPhase("preloadReady", { progress: 42 });
          return true;
        })
        .catch(function () {
          setDemoPhase("preloadPartial", { progress: 24 });
          return true;
        });
    });
  }

  function injectLoaderScript() {
    if (demoState.scriptInjected)
      return Promise.resolve();

    demoState.scriptInjected = true;

    return new Promise(function (resolve, reject) {
      const script = document.createElement("script");
      script.src = resolveBuildUrl(siteConfig.demo.loaderFile);
      script.onload = resolve;
      script.onerror = function () {
        reject(new Error("Unable to load Unity WebGL loader script."));
      };
      document.body.appendChild(script);
    });
  }

  function buildUnityConfig() {
    const demo = siteConfig.demo;
    return {
      dataUrl: resolveBuildUrl(demo.dataFile),
      frameworkUrl: resolveBuildUrl(demo.frameworkFile),
      codeUrl: resolveBuildUrl(demo.codeFile),
      streamingAssetsUrl: resolveStreamingAssetsUrl(),
      companyName: demo.companyName || "LearningArchitect",
      productName: demo.productName || "LearningArchitect",
      productVersion: demo.productVersion || "1.0.0",
      matchWebGLToCanvasSize: demo.matchWebGLToCanvasSize !== false,
      devicePixelRatio: getEffectiveDevicePixelRatio()
    };
  }

  function launchDemo() {
    if (demoState.launchStarted)
      return;

    prepareDemoAssets("cta").then(function (configured) {
      if (!configured || !isDemoConfigured())
        return;

      if (demoState.launchStarted)
        return;

      demoState.launchStarted = true;
      demoState.errorMessage = "";
      setDemoPhase("loadingRuntime", { progress: 54, runtimePercent: 0 });
      renderController.update();

      injectLoaderScript()
        .then(function () {
          if (typeof window.createUnityInstance !== "function")
            throw new Error("createUnityInstance is not available on window.");

          return window.createUnityInstance(dom.unityCanvas, buildUnityConfig(), function (progress) {
            demoState.runtimePercent = Math.round(progress * 100);
            demoState.progress = 54 + (progress * 46);
            renderDemoState();
          });
        })
        .then(function (instance) {
          demoState.unityInstance = instance;
          dom.demoPoster.classList.add("is-hidden");
          dom.demoFrame.classList.add("is-live");
          renderController.update();
          setDemoPhase("runtimeLive", { progress: 100, runtimePercent: 100 });
        })
        .catch(function (error) {
          demoState.launchStarted = false;
          const errorMessage = describeLaunchError(error);
          console.error("[LearningArchitect.Site] Unity launch failed.", error);
          setDemoPhase("launchFailed", { progress: 32, errorMessage: errorMessage });
        });
    });
  }

  function attachDemoObserver() {
    if (demoState.observerAttached || !dom.demoSection)
      return;

    demoState.observerAttached = true;

    if (!("IntersectionObserver" in window)) {
      window.setTimeout(function () {
        prepareDemoAssets("fallback");
      }, 1200);
      return;
    }

    const observer = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          prepareDemoAssets("viewport");
          observer.disconnect();
        }
      });
    }, { rootMargin: "160px 0px" });

    observer.observe(dom.demoSection);
  }

  function observeSections() {
    const sections = ["top", "interactive-demo", "module-explorer", "architecture-notes"]
      .map(function (id) { return document.getElementById(id); })
      .filter(Boolean);
    let activeSectionId = "";

    if (!("IntersectionObserver" in window) || sections.length === 0)
      return;

    function setActiveSection(id) {
      if (!id || id === activeSectionId)
        return;

      activeSectionId = id;
      document.body.setAttribute("data-active-section", id);

      sections.forEach(function (section) {
        section.classList.toggle("is-active-region", section.id === id);
      });

    }

    const observer = new IntersectionObserver(function (entries) {
      let bestEntry = null;

      entries.forEach(function (entry) {
        if (!entry.isIntersecting)
          return;

        if (bestEntry === null || entry.intersectionRatio > bestEntry.intersectionRatio)
          bestEntry = entry;
      });

      if (!bestEntry || !bestEntry.target || !bestEntry.target.id)
        return;

      setActiveSection(bestEntry.target.id);
    }, {
      rootMargin: "-12% 0px -55% 0px",
      threshold: [0.12, 0.22, 0.4, 0.6]
    });

    setActiveSection(sections[0].id);

    sections.forEach(function (section) {
      observer.observe(section);
    });
  }

  function setupRevealAnimations() {
    const revealTargets = Array.prototype.slice.call(document.querySelectorAll(
      ".section-heading, .hero-panel-card, .module-tab, .module-detail, .stack-card, .brief-card, .demo-console, .demo-host, .footer"
    )).filter(function (element) {
      return !element.classList.contains("reveal-on-scroll");
    });

    if (revealTargets.length === 0)
      return;

    revealTargets.forEach(function (element, index) {
      element.classList.add("reveal-on-scroll");
      element.style.setProperty("--reveal-delay", String(Math.min(index % 6, 5) * 55) + "ms");
    });

    if (!("IntersectionObserver" in window)) {
      revealTargets.forEach(function (element) {
        element.classList.add("is-visible");
      });
      return;
    }

    const revealObserver = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (!entry.isIntersecting)
          return;

        entry.target.classList.add("is-visible");
        revealObserver.unobserve(entry.target);
      });
    }, {
      rootMargin: "0px 0px -12% 0px",
      threshold: 0.16
    });

    revealTargets.forEach(function (element) {
      revealObserver.observe(element);
    });
  }

  function scheduleIdlePreload() {
    const start = function () {
      prepareDemoAssets("idle");
    };

    if ("requestIdleCallback" in window)
      window.requestIdleCallback(start, { timeout: 1500 });
    else
      window.setTimeout(start, 900);
  }

  function stringifyLaunchErrorPayload(error) {
    if (error === null || typeof error === "undefined")
      return "";

    if (typeof error === "string")
      return error;

    if (error instanceof Error)
      return error.message || String(error);

    if (typeof error.message === "string" && error.message.trim())
      return error.message.trim();

    if (typeof error.reason === "string" && error.reason.trim())
      return error.reason.trim();

    if (error.reason && typeof error.reason.message === "string" && error.reason.message.trim())
      return error.reason.message.trim();

    if (error.error && typeof error.error.message === "string" && error.error.message.trim())
      return error.error.message.trim();

    try {
      const serialized = JSON.stringify(error);
      return serialized && serialized !== "{}" ? serialized : "";
    } catch (serializationError) {
      return "";
    }
  }

  function describeLaunchError(error) {
    const message = stringifyLaunchErrorPayload(error);
    return message || "The Unity loader did not provide an error message.";
  }

  function escapeHtml(value) {
    return String(value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function setLanguage(language) {
    currentLanguage = language === "ru" ? "ru" : "en";
    window.localStorage.setItem("learningArchitectSiteLanguage", currentLanguage);
    renderStaticCopy();
    renderModuleSelector();
    renderSelectedModule();
    renderDemoState();
    setupRevealAnimations();
  }

  window.LearningArchitectSiteSetLanguage = setLanguage;

  function bindEvents() {
    if (dom.launchButton)
      dom.launchButton.addEventListener("click", launchDemo);
    if (dom.presentationButton)
      dom.presentationButton.addEventListener("click", togglePresentationView);
    if (dom.jumpToDemo) {
      dom.jumpToDemo.addEventListener("click", function () {
        prepareDemoAssets("cta");
      });
    }
    if (dom.langEn) {
      dom.langEn.addEventListener("click", function () {
        setLanguage("en");
      });
    }
    if (dom.langRu) {
      dom.langRu.addEventListener("click", function () {
        setLanguage("ru");
      });
    }
    if (dom.qualityPerformance) {
      dom.qualityPerformance.addEventListener("click", function () {
        setQualityMode("performance");
      });
    }
    if (dom.qualityBalanced) {
      dom.qualityBalanced.addEventListener("click", function () {
        setQualityMode("balanced");
      });
    }
    if (dom.qualityQuality) {
      dom.qualityQuality.addEventListener("click", function () {
        setQualityMode("quality");
      });
    }
    window.addEventListener("resize", renderController.update.bind(renderController));
    document.addEventListener("keydown", handleGlobalKeyDown);
  }

  Promise.all([loadSiteCopy(), loadDemoManifest()]).then(function () {
    initializeLinks();
    syncPresentationState();
    renderController.update();
    renderStaticCopy();
    renderModuleSelector();
    renderSelectedModule();
    setDemoPhase("idle", { progress: 6 });
    bindEvents();
    attachDemoObserver();
    observeSections();
    setupRevealAnimations();
    scheduleIdlePreload();
  }).catch(function (error) {
    console.error("[LearningArchitect.Site] Failed to initialize site copy.", error);
  });
}());
