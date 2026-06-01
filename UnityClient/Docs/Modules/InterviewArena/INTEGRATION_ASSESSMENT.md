# Interview Arena × LearningArchitect — оценка интеграции

Дата: 2026-05-20 (обновлено 2026-05-20)  
Статус: **MVP1–3 реализован локально** (сцена, бой, FSM, respawn, WebGL quick wins). Backend/multiplayer — backlog.

## Краткий вывод (обновлено)

Interview Arena — **не восьмой архитектурный модуль** в списке `ModuleDefinitionSO`, а **второй режим** того же hub: сверху остаётся демонстрация архитектурных модулей, снизу — отдельная зона «Игра / Interview Arena».

Так не нужно подгонять lobby, сеть и `GameStateMachine` под stress/metrics/`IModule`.

**Рекомендация (реализовано в репо):** **отдельная сцена** `InterviewArena` + кнопка на hub (`InterviewArenaLaunchDock`), без регистрации в `ShowcaseCompositionRoot.Modules`.

---

## Оценки (1–10)

| Критерий | Балл | Комментарий |
|----------|------|-------------|
| Ценность для портфолио / собеседования | **9** | WebGL + client/server + physics + pooling + FSM — редко укладывается в один репозиторий |
| Соответствие философии LearningArchitect | **7** | Hub и контракты совпадают; полноценный game loop и backend выходят за текущий «stress + compare variants» |
| Переиспользование существующих модулей | **8** | Object Pooling, AI FSM, Update Loop, VFX — прямые демонстрации тем из docs Interview Arena |
| Реализуемость в одном репо | **6** | Нужен backend (VPS), CI, отдельные тесты сети; без этого MVP2+ застрянет |
| Риск для текущего showcase | **5** | Средний: тяжёлые пакеты (Netcode), размер WebGL, Play Mode guard, время компиляции |
| Ясность документации | **9** | Пакет docs структурирован, MVP и TD зафиксированы |
| Завершённость MVP по плану | **7** | MVP1–3 в коде; MVP3 multiplayer/backend — отдельный трек |

**Итоговая пригодность как модуль LearningArchitect:** **7.5/10** — брать, но **не одним PR** и не как замену текущих 7 модулей.

**Итоговая пригодность как отдельный режим hub (рекомендуется):** **8.5/10** — лучшее соотношение ценности и риска.

---

## Предпочтительная модель: два режима hub

```text
┌─────────────────────────────────────────────┐
│  Architecture mode (по умолчанию)           │
│  • ModuleNavigation + variants              │
│  • DescriptionPanel + stress + metrics      │
│  • ShowcaseCoordinator / 7 modules          │
├─────────────────────────────────────────────┤
│  Dock / нижняя панель                       │
│  [ Architecture demos ]  [ Interview Arena ]│
└─────────────────────────────────────────────┘

При выборе Interview Arena:
  • coordinator.Exit() — выгрузить текущий variant
  • скрыть arch-only UI (stress, module ring, description tabs)
  • включить InterviewArenaBootstrap (свой state machine, HUD)
  • отдельный spawn root (не ModuleRoot под stress-контрактом)

Кнопка «Назад к демо» — обратный переход.
```

### Почему это лучше, чем 8-й модуль

| | 8-й `ModuleDefinitionSO` | Отдельный режим снизу |
|--|--------------------------|------------------------|
| Stress / 3 кнопки | Искусственные пресеты (боты?) | Не нужны |
| `IShowcaseMetricsSource` | Мало смысла для lobby | Свой HUD при желании |
| Description panel | Длинный текст про сеть в том же UI | Свои экраны Login/Lobby |
| `GameStateMachine` | Конфликт с `ShowcaseCoordinator` | Свободен внутри режима |
| Validator | Расширять под игру | Отдельный чеклист / optional |
| Смысл для ревьюера | «Ещё один демо-слайс» | «Два продукта в одном WebGL» |

### Что сохраняем из D-001

- **Одна** entry-сцена и **один** hub prefab (не возвращаем legacy отдельные demo-сцены).
- Два **опыта**, а не два репозитория.

### Минимальный код-скелет

```text
HubExperienceController          // enum: Architecture | InterviewArena
ShowcaseCompositionRoot        // без Interview Arena в modules[]
InterviewArenaEntryPoint       // EnterGameMode / ExitGameMode
InterviewArenaBootstrap        // Boot → Login → Lobby → Match (из docs)
```

Переиспользовать: `ShowcaseTransitionController` (fade), `ShowcaseLocalization` (ключи для кнопок dock), `ModuleSpawnRoot` или sibling `GameSpawnRoot`.

---

## Что уже хорошо ложится на репозиторий

1. **Единый hub** — новый `InterviewArenaModule` + 2–3 `VariantDefinitionSO` (например: `Local Arena`, `Lobby Shell`, `Networked Run` stub).
2. **Контракты** — корневой prefab: `IModule` + `IShowcaseStressTarget` (стресс = число ботов / снарядов / игроков-ботов) + `IShowcaseMetricsSource`.
3. **WebGL** — уже есть `WEBGL.md`, Site shell, консервативные stress presets; Interview Arena усиливает ту же историю.
4. **Темы из docs ↔ существующие модули:**

   | Interview Arena topic | Уже есть в showcase |
   |----------------------|---------------------|
   | Object pooling | Object Pooling |
   | AI FSM | AI System (FSM variant) |
   | Centralized vs per-object update | Update Loop Strategies |
   | Batched / emitter VFX | VFX Delivery |
   | Packed vs object slots | Inventory Systems |

5. **Локализация** — только `localizationKey` на SO; тексты в `ShowcaseContent` (как после недавнего рефакторинга variant assets).

---

## Расхождения и риски

### 1. Два «архитектурных центра»

Showcase: `ShowcaseCoordinator` + выбор варианта.  
Interview Arena: собственный `GameStateMachine` (Boot → Login → Lobby → Match).

**Митигация:** variant prefab **владеет** внутренним state machine; hub только `Enter`/`Exit` и stress. При `Exit` — жёсткий teardown (сеть, сцены additive, DontDestroyOnLoad).

### 2. Backend обязателен для заявленного MVP2+

Доки честно фиксируют personal server. В репозитории **нет** `server/` — только Unity.

**Митигация:** Milestone 4 backlog вынести в `server/` на корне или отдельный repo с ссылкой в README; для hub-демо — `MockAuthService` / `MockLobbyService` в Editor.

### 3. WebGL + multiplayer

Unity WebGL + Netcode/транспорты — ограничения памяти, threading, debugging.

**Митигация:** MVP1 без сети в hub; сетевой variant с пометкой «desktop/editor only» до стабильного WebSocket path.

### 4. Размер и время компиляции

Полный модуль (Auth, Lobby, Multiplayer, UI) + asmdef — много кода.

**Митигация:** asmdef по плану `03_UNITY_MODULES.md`, но **после** стабилизации папок (TD-007); не дублировать `InterviewArena.*` вне `Assets/Content/Modules/InterviewArena/`.

### 5. Stress semantics

Текущий showcase: stress = **реальный spawn count**. Для arena: «80 игроков» нереалистично.

**Митигация:** отдельные presets — bots, projectiles, physics props; `visualLimit` и честные метрики в `GetMetricsSnapshot`.

---

## Рекомендуемая интеграция (фазы)

### Фаза A — «модуль в hub» (4–8 недель part-time)

- `Assets/Content/Modules/InterviewArena/` + `LocalArenaVariant` prefab  
- PlayerMotor, ground check, dash, finish portal (MVP1)  
- Регистрация в `ShowcaseCompositionRoot`  
- `ShowcaseContent` EN/RU, validator, EditMode smoke  
- **Без** backend

### Фаза B — «product slice»

- Lobby UI panels в том же prefab или additive scene  
- Mock services + contract tests  
- Документировать API из `09_SERVER_API_DRAFT.md`

### Фаза C — «interview complete»

- Minimal backend repo + WebGL client path  
- Второй variant `NetworkedArenaVariant`  
- Recruiter demo step в `RecruiterDemoScenario`

---

## Варианты представления в showcase

| Вариант | Плюсы | Минусы |
|---------|-------|--------|
| **A. Один модуль, 3 variants** | Единый список модулей | Подгонка stress/metrics, два state machine |
| **B. Один variant «Arena»** | Меньше assets | Те же проблемы |
| **C. Отдельная сцена** | Простой game loop | Два entry point, слабее единая история |
| **D. Dual-mode в hub (рекомендуется)** | Не мерить игру в arch-контракт; одна сцена/WebGL | Нужен `HubExperienceController` + UI dock |

**Выбор:** вариант **D** — архитектура сверху, Interview Arena снизу (или вкладка/dock).

---

## Чеклист перед стартом кода

- [ ] Зафиксировать D-00X в `Docs/Decisions.md` (hub-only entry, mock server policy)  
- [ ] Добавить T-007 в `OpenThreads.md`  
- [ ] Выбрать backend stack (`11_BACKEND_STACK_OPTIONS.md`)  
- [ ] Определить stress presets и `visualLimit` для WebGL  
- [ ] Проверить пакеты: Netcode / Transport vs custom WebSocket  
- [ ] Не хранить дубликаты docs в `Assets/Content/`

---

## Связанные файлы репозитория

- `Docs/Architecture.md` — extension workflow  
- `Docs/WebDeployment.md` — WebGL constraints  
- `Docs/TestingStrategy.md` — EditMode vs PlayMode vs server tests  
- `.cursor/rules/interview-arena.mdc` — правила для агента при работе над модулем  
