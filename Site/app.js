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
    devicePixelRatio: 1
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
    fullscreenButton: document.getElementById("toggle-fullscreen"),
    demoStage: document.getElementById("demo-stage"),
    demoSupporting: document.getElementById("demo-supporting"),
    demoConsole: document.querySelector(".demo-console"),
    demoProgress: document.querySelector(".demo-progress"),
    demoProgressBar: document.getElementById("demo-progress-bar"),
    posterStatus: document.getElementById("poster-status"),
    demoPoster: document.getElementById("demo-poster"),
    demoFrame: document.getElementById("demo-frame"),
    demoSection: document.getElementById("interactive-demo"),
    jumpToDemo: document.getElementById("jump-to-demo"),
    unityCanvas: document.getElementById("unity-canvas")
  };

  const translations = {
    en: {
      htmlLang: "en",
      title: "LearningArchitect | Interactive Architecture Showcase",
      description: "LearningArchitect is a Unity-based architecture showcase for comparing runtime strategies, stress presets, and implementation trade-offs in one shared interactive shell.",
      nav: {
        modules: "Modules",
        architecture: "Architecture Notes",
        demo: "Interactive Demo"
      },
      hero: {
        eyebrow: "Interactive Unity Architecture Showcase",
        title: "Trade\u2011offs in one browser demo.",
        summary: "Launch WebGL and compare decisions on one page.",
        demoCta: "Launch Demo",
        architectureCta: "Architecture Notes",
        sourceCta: "Source on GitHub",
        chips: [
          "7 modules",
          "15 variants",
          "Shared stress presets",
          "WebGL portfolio shell"
        ],
        demoBadgeLabel: "WebGL",
        demoBadgeValue: "Ready",
        panelLabel: "Why this format works in interviews",
        points: [
          "You can read the context before the Unity loader takes over.",
          "The same shell keeps module and stress comparisons consistent.",
          "Architecture notes and source stay available during the walkthrough."
        ],
        stats: {
          modules: "Modules",
          variants: "Variants",
          runtime: "Shared Runtime",
          languages: "Languages"
        }
      },
      sections: {
        whatEyebrow: "What This Demonstrates",
        whatTitle: "Architecture comparison, not isolated demo scenes.",
        whatFocusLabel: "Focus here",
        whatFocusCopy: "Notice what stays constant across every module: one shell, one preset flow, one comparison surface.",
        insights: [
          {
            title: "Comparable runtime pressure",
            copy: "Each module goes through the same shell, stress presets, and metrics surface, so differences point back to architecture rather than presentation drift."
          },
          {
            title: "Pattern and domain range",
            copy: "The showcase spans AI, pooling, update loops, inventory, animation, VFX delivery, and effect simulation instead of repeating one narrow optimization story."
          },
          {
            title: "Data-driven authoring",
            copy: "Modules and variants are wired through ScriptableObject data assets, which makes the showcase easier to extend without scene-specific logic."
          },
          {
            title: "Portfolio-ready presentation",
            copy: "The browser entry page explains what the reviewer should look for before the WebGL runtime starts."
          }
        ],
        modulesEyebrow: "Module Explorer",
        modulesTitle: "Pick a strong example before the live client.",
        modulesCopy: "Use one module as the entry point, then switch variants and presets in the runtime.",
        modulesFocusLabel: "Focus here",
        modulesFocusCopy: "Use this section to decide which module best tells your architecture story before the live runtime starts.",
        moduleLabels: {
          variants: "Variants",
          focus: "Focus",
          try: "What To Try",
          signal: "Architecture Signal"
        },
        architectureEyebrow: "Architecture Notes",
        architectureTitle: "One shell, swappable variants, shared stress and metrics.",
        architectureCopy: "Enough context for a reviewer to understand the structure before opening the repository.",
        architectureFocusLabel: "Focus here",
        architectureFocusCopy: "This is where the reviewer should understand how the shell stays stable while the modules change underneath it.",
        stack: [
          {
            title: "Showcase Shell",
            copy: "The shell owns selection, stress state, localization, and lifecycle instead of scene-specific setup."
          },
          {
            title: "Presentation Shell",
            copy: "UI presenters subscribe to shared state so the interface stays comparable while modules change underneath it."
          },
          {
            title: "Module Layer",
            copy: "Active variants implement the same stress and lifecycle contract while preserving their own runtime behavior."
          },
          {
            title: "Data Layer",
            copy: "Module and variant definitions store presets, copy, and prefab references so new comparisons stay data-driven."
          }
        ],
        runtimeStoryLabel: "Runtime in five steps",
        runtimeStory: [
          "Composition root builds the coordinator.",
          "Coordinator selects module and variant data.",
          "Spawner instantiates the variant prefab.",
          "Runtime host validates lifecycle and stress contracts.",
          "State hub publishes module, variant, stress, and metrics to the UI."
        ],
        reviewLabel: "Why it is portfolio-ready",
        reviewCopy: "The value is not another Unity scene. The value is one disciplined runtime shell for comparing multiple implementation paths under the same rules.",
        reviewEyebrow: "Suggested Review Flow",
        reviewTitle: "What to try once the build is ready.",
        reviewFocusLabel: "Focus here",
        reviewFocusCopy: "This sequence tells an interviewer where to click first instead of leaving them to guess how the demo should be read.",
        reviewSteps: [
          {
            title: "Pick a module family",
            copy: "Start with `Update Loop`, `Pooling`, or `AI` depending on whether you want a pattern-focused or domain-focused comparison."
          },
          {
            title: "Switch variants",
            copy: "Use the shared variant controls to compare how different implementation styles behave under the same shell."
          },
          {
            title: "Raise the stress preset",
            copy: "Use the same preset steps to expose when ownership, layout, batching, or callback volume starts to matter."
          },
          {
            title: "Connect behavior to notes",
            copy: "Read the architecture and trade-off cards beside the runtime so the system teaches the reason behind the difference, not only the surface result."
          }
        ],
        demoEyebrow: "Interactive Demo",
        demoTitle: "Read the context, then launch.",
        demoCopy: "The page stages WebGL delivery in the background. Unity starts only when you choose to launch.",
        demoInlineCopy: "Runtime, modules, stress presets, and notes stay on one page.",
        demoFocusLabel: "Focus here",
        demoFocusCopy: "This block is the handoff from explanation to proof: read the page, then launch the shared runtime only when the reviewer is ready.",
        demoLabel: "WebGL Host State",
        demoFullscreenEnter: "Open Fullscreen",
        demoFullscreenExit: "Exit Fullscreen",
        demoNotesCta: "Read Architecture Notes",
        demoNote1: "<strong>Review mode:</strong> keep scrolling and reading while the page stages the loader and build URLs.",
        demoNote2: "<strong>Interview mode:</strong> jump back here and launch the runtime once the reviewer has context.",
        posterLabel: "Embedded Unity Runtime",
        posterTitle: "Launch the showcase shell here.",
        footerCopy: "Unity runtime for explaining architecture trade\u2011offs in one browser-friendly shell.",
        footerTop: "Back to top",
        footerArchitecture: "Architecture Notes",
        footerSource: "Source on GitHub",
        guide: {
          title: "Reading Guide",
          topTitle: "Overview",
          topCopy: "What this project is.",
          whatTitle: "Signals",
          whatCopy: "Why the comparison matters.",
          modulesTitle: "Modules",
          modulesCopy: "Pick the strongest example.",
          architectureTitle: "Architecture",
          architectureCopy: "How the shell is layered.",
          reviewTitle: "Review Flow",
          reviewCopy: "How to walk through the demo.",
          demoTitle: "Launch",
          demoCopy: "Where proof starts."
        }
      },
      demoPhases: {
        idle: {
          pill: "Preload idle",
          pillCopy: "Core demo delivery will start staging in the background while you read.",
          stage: "Waiting for configuration",
          supporting: "The host shell is ready. Drop a Unity WebGL export into Site/webgl/Build, run prepare-webgl-site.ps1, and this section can preload and launch the runtime from the same page.",
          poster: "No WebGL build is connected yet. The presentation shell is ready; add the export and generate <code>webgl/build-manifest.json</code>.",
          button: "Launch Demo"
        },
        buildPending: {
          pill: "Build pending",
          pillCopy: "The presentation shell is ready, but the Unity WebGL export is not configured yet.",
          stage: "Build pending",
          supporting: "Add the Unity WebGL export into Site/webgl/Build and run prepare-webgl-site.ps1 to generate the launch manifest.",
          poster: "No WebGL build is configured yet. The host shell is in place and waiting for the exported Unity build.",
          button: "Check for WebGL Build"
        },
        preloading: {
          pill: "Preloading",
          pillCopy: "Staging Unity loader and build artifacts while the page remains readable.",
          stage: "Preloading build URLs",
          supporting: "The page is warming the loader and build files in the background. You can keep reading until launch time.",
          poster: "Core delivery files are being staged in the background. The interactive runtime instance will only be created when you click launch.",
          button: "Launch While Preparing"
        },
        preloadReady: {
          pill: "Preload ready",
          pillCopy: "The Unity loader is staged. Launch will create the runtime instance on demand.",
          stage: "Core loader ready",
          supporting: "The shell has staged the Unity loader. The runtime instance will initialize only when you choose to open the demo.",
          poster: "The host shell is ready. Click launch to create the Unity WebGL instance inside this page.",
          button: "Launch Demo"
        },
        preloadPartial: {
          pill: "Preload partial",
          pillCopy: "The page could not verify the loader yet, but launch can still try a direct initialization.",
          stage: "Loader not verified",
          supporting: "The shell could not verify the WebGL loader path during preload. Launch can still attempt a direct initialization.",
          poster: "The Unity build path may still be wrong. Re-run prepare-webgl-site.ps1 or confirm the exported build sits under Site/webgl/Build.",
          button: "Try Launch Anyway"
        },
        loadingRuntime: {
          pill: "Launching",
          pillCopy: "The Unity runtime is being created only now, after the reviewer already has context.",
          stage: "Creating runtime instance",
          supporting: "The page is creating the Unity WebGL instance only when launch is requested.",
          poster: "The interactive runtime is starting. Once the instance is ready, the poster will get out of the way.",
          button: "Launching Demo..."
        },
        runtimeLive: {
          pill: "Runtime live",
          pillCopy: "The Unity WebGL client is active inside the same presentation page.",
          stage: "Runtime live",
          supporting: "The client is running. Reviewers can now switch modules and variants without leaving this page.",
          poster: "The interactive runtime is live.",
          button: "Demo Running"
        },
        launchFailed: {
          pill: "Launch failed",
          pillCopy: "The presentation shell is fine, but the WebGL build path or exported files still need attention.",
          stage: "Launch failed",
          supporting: "The host shell attempted to create the Unity instance but the build could not start. Check the exported build files and config paths.",
          posterPrefix: "Launch failed: ",
          posterSuffix: "<br>Confirm the Unity build files exist in <code>Site/webgl/Build</code> and re-generate <code>webgl/build-manifest.json</code> with <code>prepare-webgl-site.ps1</code>.",
          button: "Retry Demo Launch"
        },
        runtimeProgress: "Loading runtime {value}%"
      }
    },
    ru: {
      htmlLang: "ru",
      title: "LearningArchitect | Интерактивный архитектурный showcase",
      description: "LearningArchitect — это Unity showcase для сравнения runtime-стратегий, stress preset'ов и архитектурных компромиссов в одной общей оболочке.",
      nav: {
        modules: "Модули",
        architecture: "Архитектура",
        demo: "Демо"
      },
      hero: {
        eyebrow: "Интерактивный Unity Architecture Showcase",
        title: "Trade\u2011offs прямо в браузере.",
        summary: "Запусти WebGL и сравнивай решения на одной странице.",
        demoCta: "Запустить демо",
        architectureCta: "Заметки по архитектуре",
        sourceCta: "Исходники на GitHub",
        chips: [
          "7 модулей",
          "15 вариантов",
          "Общие stress preset'ы",
          "WebGL portfolio shell"
        ],
        demoBadgeLabel: "WebGL",
        demoBadgeValue: "Готово",
        panelLabel: "Почему это удобно для собеседований",
        points: [
          "Контекст можно прочитать до того, как Unity loader займёт экран.",
          "Одна и та же оболочка держит сравнение модулей и stress preset'ов единым.",
          "Архитектурные заметки и код остаются рядом во время показа."
        ],
        stats: {
          modules: "Модулей",
          variants: "Вариантов",
          runtime: "Общий runtime",
          languages: "Языка"
        }
      },
      sections: {
        whatEyebrow: "Что Это Показывает",
        whatTitle: "Сравнение архитектуры, а не набор отдельных сцен.",
        whatFocusLabel: "Фокус Здесь",
        whatFocusCopy: "Здесь важно заметить, что остаётся неизменным у всех модулей: одна оболочка, один preset flow, одна поверхность сравнения.",
        insights: [
          {
            title: "Сопоставимое давление на runtime",
            copy: "Каждый модуль проходит через одну и ту же оболочку, stress preset'ы и поверхность метрик, поэтому различия видны именно в архитектуре."
          },
          {
            title: "Широкий охват паттернов и доменов",
            copy: "Showcase охватывает AI, pooling, update loops, inventory, animation, VFX delivery и effect simulation вместо одной узкой истории про оптимизацию."
          },
          {
            title: "Data-driven авторинг",
            copy: "Модули и варианты связываются через ScriptableObject assets, поэтому проект проще расширять без scene-specific логики."
          },
          {
            title: "Упаковка под портфолио",
            copy: "Стартовая страница объясняет, на что смотреть, ещё до запуска WebGL runtime."
          }
        ],
        modulesEyebrow: "Обзор Модулей",
        modulesTitle: "Выбери сильный пример перед live-клиентом.",
        modulesCopy: "Используй один модуль как точку входа, а затем переключай варианты и preset'ы уже в runtime.",
        modulesFocusLabel: "Фокус Здесь",
        modulesFocusCopy: "Используй этот блок, чтобы выбрать модуль, который лучше всего рассказывает твою архитектурную историю до живого запуска.",
        moduleLabels: {
          variants: "Варианты",
          focus: "Фокус",
          try: "Что попробовать",
          signal: "Архитектурный сигнал"
        },
        architectureEyebrow: "Заметки По Архитектуре",
        architectureTitle: "Одна оболочка, сменные варианты, общий stress и metrics.",
        architectureCopy: "Этого достаточно, чтобы понять структуру до перехода в репозиторий.",
        architectureFocusLabel: "Фокус Здесь",
        architectureFocusCopy: "Здесь зритель должен понять, как shell остаётся стабильным, пока сами модули меняются под ним.",
        stack: [
          {
            title: "Showcase Shell",
            copy: "Оболочка владеет выбором, stress state, локализацией и lifecycle, а не перекладывает это на scene-specific setup."
          },
          {
            title: "Presentation Shell",
            copy: "UI presenters подписываются на общий state, поэтому интерфейс остаётся сопоставимым, пока сами модули меняются под ним."
          },
          {
            title: "Module Layer",
            copy: "Активные variants реализуют один и тот же stress и lifecycle contract, но сохраняют своё runtime-поведение."
          },
          {
            title: "Data Layer",
            copy: "ModuleDefinition и VariantDefinition хранят presets, copy и prefab references, поэтому новые сравнения остаются data-driven."
          }
        ],
        runtimeStoryLabel: "Runtime в пяти шагах",
        runtimeStory: [
          "Composition root собирает coordinator.",
          "Coordinator выбирает данные модуля и варианта.",
          "Spawner инстанцирует prefab варианта.",
          "Runtime host валидирует lifecycle и stress contracts.",
          "State hub публикует модуль, вариант, stress и метрики в UI."
        ],
        reviewLabel: "Почему это удобно для портфолио",
        reviewCopy: "Ценность не в ещё одной Unity-сцене. Ценность в одной дисциплинированной оболочке, которая сравнивает несколько путей реализации по одинаковым правилам.",
        reviewEyebrow: "Как Смотреть Демо",
        reviewTitle: "Что попробовать после запуска build'а.",
        reviewFocusLabel: "Фокус Здесь",
        reviewFocusCopy: "Эта последовательность показывает интервьюеру, куда кликать в первую очередь, вместо того чтобы оставлять его без сценария.",
        reviewSteps: [
          {
            title: "Выбери семейство модулей",
            copy: "Начни с `Update Loop`, `Pooling` или `AI` в зависимости от того, хочешь ли ты pattern-driven или domain-driven сравнение."
          },
          {
            title: "Переключай варианты",
            copy: "Используй общий вариантный control, чтобы сравнить разные implementation styles внутри одной и той же оболочки."
          },
          {
            title: "Подними stress preset",
            copy: "Используй одинаковые preset steps, чтобы увидеть, когда ownership, layout, batching или callback volume начинают иметь значение."
          },
          {
            title: "Связывай поведение с заметками",
            copy: "Читай архитектурные карточки рядом с runtime, чтобы система объясняла не только результат на экране, но и причину различий."
          }
        ],
        demoEyebrow: "Интерактивное Демо",
        demoTitle: "Прочитай контекст, затем запускай.",
        demoCopy: "Страница заранее готовит WebGL-доставку. Unity instance создаётся только по кнопке.",
        demoInlineCopy: "Runtime встроен в страницу: модули, stress preset'ы и заметки рядом.",
        demoFocusLabel: "Фокус Здесь",
        demoFocusCopy: "Этот блок — переход от объяснения к доказательству: сначала чтение, потом запуск общего runtime в нужный момент.",
        demoLabel: "Состояние WebGL Host",
        demoFullscreenEnter: "Открыть на весь экран",
        demoFullscreenExit: "Выйти из полного экрана",
        demoNotesCta: "Читать заметки по архитектуре",
        demoNote1: "<strong>Режим просмотра:</strong> можно листать и читать страницу, пока host подготавливает loader и build URLs.",
        demoNote2: "<strong>Режим собеседования:</strong> вернись сюда и запусти runtime, когда у зрителя уже есть контекст.",
        posterLabel: "Встроенный Unity Runtime",
        posterTitle: "Запусти showcase-оболочку прямо здесь.",
        footerCopy: "Unity runtime для объяснения trade\u2011offs в одной browser-friendly оболочке.",
        footerTop: "Наверх",
        footerArchitecture: "Архитектура",
        footerSource: "Исходники на GitHub",
        guide: {
          title: "Маршрут Чтения",
          topTitle: "Обзор",
          topCopy: "Что это за проект.",
          whatTitle: "Сигналы",
          whatCopy: "Почему это сравнение важно.",
          modulesTitle: "Модули",
          modulesCopy: "Выбери сильнейший пример.",
          architectureTitle: "Архитектура",
          architectureCopy: "Как устроены слои shell.",
          reviewTitle: "Сценарий Показа",
          reviewCopy: "Как провести по демо.",
          demoTitle: "Запуск",
          demoCopy: "Где начинается proof."
        }
      },
      demoPhases: {
        idle: {
          pill: "Preload idle",
          pillCopy: "Подготовка core demo delivery начнётся в фоне, пока ты читаешь страницу.",
          stage: "Ожидание конфигурации",
          supporting: "Host shell уже готов. Положи Unity WebGL export в Site/webgl/Build, запусти prepare-webgl-site.ps1, и эта секция сможет preload'ить и запускать runtime прямо с этой страницы.",
          poster: "WebGL build пока не подключён. Presentation shell уже готов: нужно добавить export и сгенерировать <code>webgl/build-manifest.json</code>.",
          button: "Запустить демо"
        },
        buildPending: {
          pill: "Build pending",
          pillCopy: "Presentation shell уже готов, но Unity WebGL export ещё не подключён.",
          stage: "Build pending",
          supporting: "Добавь WebGL export в Site/webgl/Build и запусти prepare-webgl-site.ps1, чтобы собрать launch manifest.",
          poster: "WebGL build пока не подключён. Host shell уже на месте и ждёт экспортированный Unity build.",
          button: "Проверить WebGL build"
        },
        preloading: {
          pill: "Preloading",
          pillCopy: "Страница подготавливает Unity loader и build artifacts, пока текст остаётся читаемым.",
          stage: "Предзагрузка build URL",
          supporting: "Страница прогревает loader и build files в фоне. Можно продолжать чтение до момента запуска.",
          poster: "Core delivery files уже подготавливаются в фоне. Сам Unity runtime instance будет создан только после клика.",
          button: "Запустить во время подготовки"
        },
        preloadReady: {
          pill: "Preload ready",
          pillCopy: "Unity loader уже подготовлен. Instance будет создан только по запросу.",
          stage: "Core loader готов",
          supporting: "Shell уже прогрел Unity loader. Runtime instance инициализируется только в момент запуска.",
          poster: "Host shell готов. Нажми запуск, чтобы создать Unity WebGL instance прямо на этой странице.",
          button: "Запустить демо"
        },
        preloadPartial: {
          pill: "Preload partial",
          pillCopy: "Страница не смогла подтвердить loader, но прямой запуск всё ещё можно попробовать.",
          stage: "Loader не подтверждён",
          supporting: "Shell не смог проверить путь к WebGL loader во время preload. Но launch всё ещё может попробовать прямую инициализацию.",
          poster: "Путь к Unity build всё ещё может быть неверным. Повтори prepare-webgl-site.ps1 или проверь, что export лежит в Site/webgl/Build.",
          button: "Попробовать запуск"
        },
        loadingRuntime: {
          pill: "Launching",
          pillCopy: "Unity runtime создаётся только сейчас, когда у зрителя уже есть контекст.",
          stage: "Создание runtime instance",
          supporting: "Страница создаёт Unity WebGL instance только в момент запуска.",
          poster: "Интерактивный runtime запускается. Как только instance будет готов, poster уйдёт с экрана.",
          button: "Запуск демо..."
        },
        runtimeLive: {
          pill: "Runtime live",
          pillCopy: "Unity WebGL client уже активен внутри этой же страницы.",
          stage: "Runtime live",
          supporting: "Клиент запущен. Теперь можно переключать модули и варианты, не уходя с этой страницы.",
          poster: "Интерактивный runtime уже активен.",
          button: "Демо запущено"
        },
        launchFailed: {
          pill: "Launch failed",
          pillCopy: "Presentation shell в порядке, но пути до WebGL build или export files ещё нужно проверить.",
          stage: "Запуск не удался",
          supporting: "Host shell попытался создать Unity instance, но build не смог стартовать. Проверь export files и config paths.",
          posterPrefix: "Запуск не удался: ",
          posterSuffix: "<br>Проверь, что Unity build files лежат в <code>Site/webgl/Build</code>, и пересобери <code>webgl/build-manifest.json</code> через <code>prepare-webgl-site.ps1</code>.",
          button: "Повторить запуск"
        },
        runtimeProgress: "Загрузка runtime {value}%"
      }
    }
  };

  const modules = [
    {
      id: "update-loop",
      en: {
        category: "Architecture Pattern Module",
        title: "Update Loop Strategies",
        summary: "Compare per-object callbacks against one centralized owner loop under identical shell controls.",
        variants: "Per-Object, Centralized",
        focus: "Callback ownership, scene noise, and how update responsibility scales.",
        try: "Switch variants, raise the stress preset, and compare how easily behavior maps back to the owning code model.",
        signal: "This module shows when a simple MonoBehaviour-per-object baseline stops being the right long-term path.",
        image: "./assets/images/module-preview-a.png",
        caption: "The same shell can compare a friendly object model against a more scalable centralized update owner."
      },
      ru: {
        category: "Архитектурный Паттерн",
        title: "Стратегии Update Loop",
        summary: "Сравнение Update на каждом объекте и одного централизованного owner loop внутри одной оболочки.",
        variants: "На объекте, Централизованный",
        focus: "Владение callback'ами, шум сцены и то, как масштабируется ответственность за update.",
        try: "Переключай варианты, поднимай stress preset и смотри, насколько поведение легко связать с кодовой моделью владения.",
        signal: "Этот модуль показывает, когда понятная MonoBehaviour-модель на каждый объект перестаёт быть хорошим долгосрочным baseline.",
        image: "./assets/images/module-preview-a.png",
        caption: "Одна и та же оболочка сравнивает дружелюбную объектную модель с более масштабируемым centralized owner loop."
      }
    },
    {
      id: "pooling",
      en: {
        category: "Architecture Pattern Module",
        title: "Object Pooling",
        summary: "Contrast straight instantiation against a reusable pool while the visible hub experience stays the same.",
        variants: "Instantiation, Reusable Pool",
        focus: "Lifecycle churn, allocation pressure, and ownership of reusable runtime objects.",
        try: "Increase stress and compare how each variant frames object lifetime and reset logic.",
        signal: "The difference is not only performance. It is also about who owns re-entry, cleanup, and spawn coordination.",
        image: "./assets/images/module-preview-b.png",
        caption: "The pooled path reframes runtime object lifetime instead of only patching allocations after the fact."
      },
      ru: {
        category: "Архитектурный Паттерн",
        title: "Object Pooling",
        summary: "Сравнение прямой instantiation и reusable pool при одинаковом пользовательском потоке в hub.",
        variants: "Instantiation, Reusable Pool",
        focus: "Lifecycle churn, allocation pressure и владение переиспользуемыми runtime-объектами.",
        try: "Повышай stress и сравнивай, как каждый вариант определяет время жизни объекта и reset logic.",
        signal: "Разница не только в производительности. Она ещё и в том, кто владеет повторным входом, cleanup и spawn coordination.",
        image: "./assets/images/module-preview-b.png",
        caption: "Pooled path переопределяет владение жизненным циклом объектов, а не просто чинит allocations постфактум."
      }
    },
    {
      id: "vfx-delivery",
      en: {
        category: "Architecture Pattern Module",
        title: "VFX Delivery",
        summary: "Compare emitter-local bursts with a batched pulses approach aimed at denser feedback scenarios.",
        variants: "Emitter Bursts, Batched Pulses",
        focus: "Effect ownership, presentation density, and rendering-oriented delivery decisions.",
        try: "Flip between local emitters and batched pulses, then inspect how the same presentation goal shifts the architecture boundary.",
        signal: "Dense VFX problems are often ownership and batching problems, not only content-authoring problems.",
        image: "./assets/images/vfx-batched.png",
        caption: "Batched visual feedback emphasizes architectural delivery choices more than isolated one-off emitters."
      },
      ru: {
        category: "Архитектурный Паттерн",
        title: "VFX Delivery",
        summary: "Сравнение локальных emitter bursts и batched pulses для более плотных сценариев визуальной обратной связи.",
        variants: "Emitter Bursts, Batched Pulses",
        focus: "Владение эффектами, плотность presentation и rendering-oriented delivery decisions.",
        try: "Переключай local emitters и batched pulses и смотри, как одна и та же цель меняет архитектурную границу.",
        signal: "Плотные VFX-задачи часто оказываются задачами ownership и batching, а не только content authoring.",
        image: "./assets/images/vfx-batched.png",
        caption: "Batched visual feedback делает архитектурные различия видимыми сильнее, чем isolated one-off emitters."
      }
    },
    {
      id: "effects-system",
      en: {
        category: "Simulation Module",
        title: "Effects System",
        summary: "Show how straightforward object-style effects compare with chunk-based batched updates.",
        variants: "Indie, Chunk",
        focus: "Readability versus packed update ownership for many small transient effects.",
        try: "Raise the same stress preset in both variants and compare hierarchy friendliness against batched processing scale.",
        signal: "The module shows how a system can evolve from easy-to-read object ownership into explicit packed processing.",
        image: "./assets/images/module-preview-b.png",
        caption: "Chunk-based execution makes the architecture story visible without changing the surrounding shell."
      },
      ru: {
        category: "Симуляционный Модуль",
        title: "Effects System",
        summary: "Показывает разницу между straightforward object-style effects и chunk-based batched updates.",
        variants: "Indie, Chunk",
        focus: "Читаемость против packed update ownership для большого числа мелких transient effects.",
        try: "Подними один и тот же stress preset в обоих вариантах и сравни hierarchy friendliness с масштабом batched processing.",
        signal: "Модуль показывает, как система эволюционирует от простого object ownership к явному packed processing.",
        image: "./assets/images/module-preview-b.png",
        caption: "Chunk-based execution делает архитектурную историю видимой, не меняя остальную оболочку."
      }
    },
    {
      id: "ai-system",
      en: {
        category: "Simulation Module",
        title: "AI System",
        summary: "Compare FSM, Utility, and Behavior Tree decision models inside one common runtime presentation shell.",
        variants: "FSM, Utility, Behavior Tree",
        focus: "Decision-model complexity, explainability, and how control logic scales with agent count.",
        try: "Switch decision models, keep the same stress preset, and read the trade-offs panel alongside the visible runtime changes.",
        signal: "This is less about one best AI pattern and more about how different decision models shape ownership and reasoning cost.",
        image: "./assets/images/module-preview-a.png",
        caption: "The AI module turns abstract decision-model discussions into something visible and comparable."
      },
      ru: {
        category: "Симуляционный Модуль",
        title: "AI System",
        summary: "Сравнение FSM, Utility и Behavior Tree decision models внутри одной общей presentation shell.",
        variants: "FSM, Utility, Behavior Tree",
        focus: "Сложность decision model, explainability и то, как control logic масштабируется по числу агентов.",
        try: "Переключай decision models, держи один и тот же stress preset и читай trade-offs panel рядом с поведением в runtime.",
        signal: "Здесь вопрос не в одном лучшем AI pattern, а в том, как разные decision models меняют ownership и reasoning cost.",
        image: "./assets/images/module-preview-a.png",
        caption: "AI-модуль делает абстрактный разговор о decision models видимым и сопоставимым."
      }
    },
    {
      id: "inventory",
      en: {
        category: "Simulation Module",
        title: "Inventory Systems",
        summary: "Contrast object-rich slot ownership with packed slot processing for a mutation-heavy system.",
        variants: "Object Slots, Packed Slots",
        focus: "Data layout, mutability, and how much convenience the runtime can afford before pressure builds.",
        try: "Inspect the Inventory variants and compare how representation choices change the mental model of ownership.",
        signal: "Inventory starts as convenient object graphs very often. The packed path shows what happens when that convenience becomes hot runtime traffic.",
        image: "./assets/images/inventory-metrics.png",
        caption: "Inventory is useful because layout decisions affect both code clarity and scaling characteristics."
      },
      ru: {
        category: "Симуляционный Модуль",
        title: "Inventory Systems",
        summary: "Сравнение object-rich slot ownership и packed slot processing для mutation-heavy системы.",
        variants: "Object Slots, Packed Slots",
        focus: "Data layout, mutability и то, сколько удобства runtime может позволить себе до появления давления.",
        try: "Посмотри варианты Inventory и сравни, как representation choices меняют mental model of ownership.",
        signal: "Inventory очень часто стартует как удобные object graphs. Packed path показывает, что происходит, когда это удобство становится hot runtime traffic.",
        image: "./assets/images/inventory-metrics.png",
        caption: "Inventory полезен как пример: решения по layout влияют и на читаемость кода, и на scaling characteristics."
      }
    },
    {
      id: "animation",
      en: {
        category: "Simulation Module",
        title: "Layered Character Animation",
        summary: "Show one humanoid model running, shooting, and combining both actions through Animator layers.",
        variants: "Run, Shoot, Run + Shoot",
        focus: "Animator layers, upper-body AvatarMask, and why action games separate locomotion from combat actions.",
        try: "Open Run + Shoot to discuss how the lower body keeps locomotion while the upper body plays the shooting clip.",
        signal: "Animation systems are not only clip playback. The production problem is composing actions without losing movement responsiveness.",
        image: "./assets/images/module-preview-a.png",
        caption: "The animation module broadens the showcase into a common game-dev mechanic: layered locomotion plus combat."
      },
      ru: {
        category: "Симуляционный Модуль",
        title: "Слоистая анимация персонажа",
        summary: "Одна humanoid-модель бежит, стреляет или совмещает оба действия через Animator layers.",
        variants: "Бег, Стрельба, Бег + стрельба",
        focus: "Animator layers, upper-body AvatarMask и разделение locomotion от combat actions.",
        try: "Открой Бег + стрельба и покажи, как ноги продолжают locomotion, пока верх тела проигрывает shooting clip.",
        signal: "Animation systems — это не только playback клипов. В production часто нужно компоновать действия, не теряя отзывчивость движения.",
        image: "./assets/images/module-preview-a.png",
        caption: "Animation-модуль показывает частую game-dev механику: layered locomotion plus combat."
      }
    }
  ];

  const demoState = {
    phase: "idle",
    preloaded: false,
    loaderReady: false,
    scriptInjected: false,
    launchStarted: false,
    observerAttached: false,
    unityInstance: null,
    fullscreenActive: false,
    progress: 6,
    runtimePercent: 0,
    errorMessage: ""
  };

  const requestedLanguage = new URLSearchParams(window.location.search).get("lang");
  let currentLanguage = requestedLanguage === "ru" || requestedLanguage === "en"
    ? requestedLanguage
    : (window.localStorage.getItem("learningArchitectSiteLanguage") === "ru" ? "ru" : "en");
  let selectedModuleId = modules[0].id;

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

  function getTranslation() {
    return translations[currentLanguage];
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

    setText("section-demo-eyebrow", copy.sections.demoEyebrow);
    setText("section-demo-title", copy.sections.demoTitle);
    setText("section-demo-copy", copy.sections.demoCopy);
    setText("demo-inline-copy", copy.sections.demoInlineCopy);
    setText("demo-console-label", copy.sections.demoLabel);
    setText("toggle-fullscreen", demoState.fullscreenActive ? copy.sections.demoFullscreenExit : copy.sections.demoFullscreenEnter);
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
  }

  function renderModuleSelector() {
    if (!dom.moduleSelector)
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
    if (!dom.moduleCategory || !dom.moduleTitle || !dom.moduleSummary || !dom.moduleVariants || !dom.moduleFocus || !dom.moduleTry || !dom.moduleSignal || !dom.moduleImage || !dom.moduleCaption)
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

  function isFullscreenSupported() {
    return Boolean(dom.demoFrame && (dom.demoFrame.requestFullscreen || dom.demoFrame.webkitRequestFullscreen));
  }

  function isDemoFullscreen() {
    const fullscreenElement = document.fullscreenElement || document.webkitFullscreenElement;
    return fullscreenElement === dom.demoFrame || fullscreenElement === dom.unityCanvas;
  }

  function renderFullscreenButton() {
    if (!dom.fullscreenButton)
      return;

    const copy = getTranslation();
    demoState.fullscreenActive = isDemoFullscreen();
    dom.demoFrame.classList.toggle("is-fullscreen", demoState.fullscreenActive);
    dom.fullscreenButton.hidden = !isFullscreenSupported();
    dom.fullscreenButton.textContent = demoState.fullscreenActive
      ? copy.sections.demoFullscreenExit
      : copy.sections.demoFullscreenEnter;
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
    renderFullscreenButton();
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
      devicePixelRatio: demo.devicePixelRatio || 1
    };
  }

  function requestDemoFullscreen() {
    if (demoState.unityInstance && typeof demoState.unityInstance.SetFullscreen === "function") {
      demoState.unityInstance.SetFullscreen(1);
      return Promise.resolve();
    }

    if (!dom.demoFrame)
      return Promise.resolve();

    if (dom.demoFrame.requestFullscreen)
      return dom.demoFrame.requestFullscreen();

    if (dom.demoFrame.webkitRequestFullscreen) {
      dom.demoFrame.webkitRequestFullscreen();
      return Promise.resolve();
    }

    return Promise.resolve();
  }

  function exitDemoFullscreen() {
    if (document.exitFullscreen)
      return document.exitFullscreen();

    if (document.webkitExitFullscreen) {
      document.webkitExitFullscreen();
      return Promise.resolve();
    }

    return Promise.resolve();
  }

  function toggleDemoFullscreen() {
    const action = isDemoFullscreen() ? exitDemoFullscreen() : requestDemoFullscreen();
    Promise.resolve(action).catch(function () {
      renderFullscreenButton();
    });
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
          setDemoPhase("runtimeLive", { progress: 100, runtimePercent: 100 });
        })
        .catch(function (error) {
          demoState.launchStarted = false;
          setDemoPhase("launchFailed", { progress: 32, errorMessage: error.message || "Unknown error." });
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

  function bindEvents() {
    if (dom.launchButton)
      dom.launchButton.addEventListener("click", launchDemo);
    if (dom.fullscreenButton)
      dom.fullscreenButton.addEventListener("click", toggleDemoFullscreen);
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
    document.addEventListener("fullscreenchange", renderFullscreenButton);
    document.addEventListener("webkitfullscreenchange", renderFullscreenButton);
  }

  loadDemoManifest().finally(function () {
    initializeLinks();
    renderStaticCopy();
    renderModuleSelector();
    renderSelectedModule();
    setDemoPhase("idle", { progress: 6 });
    bindEvents();
    attachDemoObserver();
    observeSections();
    setupRevealAnimations();
    scheduleIdlePreload();
  });
}());
