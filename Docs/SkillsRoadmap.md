# Skills & Interview Roadmap

Живой чеклист навыков Unity и full-stack для проекта **ExtractionRPG** (workspace `LearningArchitect`).

**Зачем документ:** понимать, что уже можно показывать на собесе, что добивать по ходу обычной разработки, и не расползаться в сторонние туториалы.

**Как пользоваться:**

1. Перед собесом — пробеги раздел [Elevator pitch](#elevator-pitch) и таблицы со статусом ✅.
2. При планировании спринта — бери задачи из [Приоритетный backlog](#приоритетный-backlog) и сверяй с `Roadmap.md`.
3. После реализации пункта — обнови статус в этом файле и при необходимости `Roadmap.md` / Arena backlog.

**Легенда статусов**

| Символ | Значение |
|--------|----------|
| ✅ | Реализовано, можно демонстрировать |
| 🟡 | Частично / инфраструктура есть, нет полного сценария |
| ❌ | Нет в проекте |
| 📋 | Запланировано в backlog ниже |

---

## Elevator pitch

Три слоя одного репозитория — не «40 мелких туториалов», а связная история:

| Слой | Что это | Где смотреть |
|------|---------|--------------|
| **Engineering Showcase** | 7 модулей со stress-тестами: pooling, update strategies, VFX batching, AI simulation | `UnityClient/Assets/Content/Modules/*`, hub-сцена |
| **Interview Arena** | Gameplay: locomotion, combat, FSM, buffs, scene composition, EditMode-тесты | `UnityClient/Assets/Content/Modules/InterviewArena/` |
| **Online extraction** | REST, auth, lobby, match start/result; backend с тестами | `Backend/`, online flow в Arena |

Фраза для собеса: *«Showcase доказывает инженерию, Arena — геймплей и архитектуру сцены, backend — что клиенту нельзя доверять результат матча.»*

---

## Базовый чеклист (20 пунктов)

Классический список тем для Unity-собеса и портфолио.

| № | Тема | Статус | Где в проекте | Следующий шаг |
|---|------|--------|---------------|---------------|
| 1 | Object Pool (враги, пули, VFX) | 🟡 | `ProjectilePool`; модуль **Object Pooling** (`PooledVariant`); респавн врагов без пула | Пул врагов + VFX в Arena; связать с волнами |
| 2 | SO-события (GameEvent + Listener) | ✅ | `Shared/Events/`, `GameEventEditor`, bridge на `RunCompleted` | Подписать UI через `GameEventListener` |
| 3 | SO-переменные (Int/Float Variable) | ❌ | Данные в `*Config` SO | Низкий приоритет; только если multi-scene hub |
| 4 | JSON save/load в файл | ✅ | `GameSettings` → `persistentDataPath/game_settings.json` | Расширить поля (audio, graphics) |
| 5 | Аддитивная загрузка сцен | 🟡 | Runtime: `LoadSceneMode.Single` (`ShowcaseSceneLoader`); additive в Editor | Additive Arena + unload hub |
| 6 | Loading screen + progress bar | ❌ | Fade в `ShowcaseTransitionController` | `LoadSceneAsync`, `allowSceneActivation`, UI progress |
| 7 | Health Bar World Space | ❌ | Screen HUD (`PlayerIframeHud`); tint на меше (`InvulnerabilityWorldIndicator`) | Prefab WS Canvas над врагом |
| 8 | FSM AI (Idle→Patrol→Chase→Attack) | 🟡 | `EnemyBrain` + `EnemyFsmLogic`: Patrol/Chase/Attack/Ranged; Idle нет | Опционально Idle; документировать testable FSM |
| 9 | AI LOD (реже Update вдали) | ❌ | Все враги каждый кадр | Таймер по дистанции до камеры |
| 10 | Виртуализация скролла (UI pool) | ❌ | Обычный `ScrollRect` в hub | Lobby browser при росте списка |
| 11 | GPU Instancing | ❌ | `MaterialPropertyBlock` в VFX-модуле | Отдельный showcase-вариант или instanced crowd |
| 12 | Profiler до/после | 🟡 | Metrics overlay, `SystemStatusSampler`, stress-модули | 2–3 скрина + цифры в `UnityClient/Docs/` |
| 13 | Zenject / Extenject | ❌ | Composition root: `ShowcaseCompositionRoot`, `InterviewArenaRuntimeContext` | **Не обязателен** — уметь объяснить trade-off |
| 14 | Addressables (load по ключу) | 🟡 | Пакет + группы (локализация); runtime `Load*` в коде нет | Загрузка Arena scene/prefab по ключу |
| 15 | UniTask + CancellationToken | 🟡 | `Task` + `Task.Yield` в HTTP-клиенте; token в API | UniTask + `GetCancellationTokenOnDestroy` |
| 16 | Корутины (волны с задержкой) | 🟡 | Hit feedback, transitions, respawn timer | `WaveSpawner` с `WaitForSeconds` / `WaitUntil` |
| 17 | NavMesh | 🟡 | Пакет `com.unity.ai.navigation`; Arena — planar motor | Bake + `NavMeshAgent` на врагах |
| 18 | Локализация (2+ языка) | ✅ | Unity Localization, EN/RU, `ShowcaseLocalization` | Arena UI keys в `ShowcaseContent` |
| 19 | Custom Inspector (подписчики событий) | ✅ | `Editor/Events/GameEventEditor.cs` | Расширить на другие SO при необходимости |
| 20 | Asset Menu для ScriptableObject | ✅ | `CreateAssetMenu` на configs Arena, modules, animation | Поддерживать для новых SO |

**Сводка:** ✅ 6 · 🟡 10 · ❌ 4

---

## Расширенный чеклист (по категориям)

Темы, которые часто спрашивают, но не входили в исходные 20 пунктов.

### Архитектура и качество кода

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| Assembly Definitions, границы модулей | ✅ | `LearningArchitect.Modules.InterviewArena.asmdef`, showcase modules |
| Pure logic + unit-тесты без Play Mode | ✅ | `EnemyFsmLogic`, `PlayerLocomotionMath`, `BackendJsonParserTests` |
| Composition Root (без Service Locator) | ✅ | `InterviewArenaRuntimeContext`, `ShowcaseCompositionRoot` |
| Интерфейсы для внешних систем | ✅ | `IBackendApiClient`, backend repositories |
| Scene composition contract + validators | ✅ | `SCENE_COMPOSITION.md`, `InterviewArenaSceneCompositionValidator` |
| Command / Strategy для abilities | ❌ | Combat — прямой wiring |
| Event Bus vs SO-events (осознанный выбор) | 🟡 | Разные стили; нет единого документа trade-offs |

### Производительность и память

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| Zero/low GC в hot path | 🟡 | Stress-модули; Arena не везде без аллокаций |
| Jobs + Burst | ❌ | — |
| Physics NonAlloc, layer masks | 🟡 | `PhysicsQueryService`, `InterviewArenaPhysicsLayers` |
| FixedUpdate vs Update | 🟡 | Rigidbody player, planar enemies |
| SRP Batcher / draw calls | 🟡 | URP; нет оформленного кейса |
| Mesh LOD / occlusion culling | ❌ | — |
| Утечки при unload сцены (подписки) | 🟡 | Online flow отписывается; нет чеклиста |
| Profiler: CPU/GPU/Memory workflow | 🟡 | In-game metrics; нет методички |

### Input, камера, game feel

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| New Input System | ✅ | `PlayerInputReader` |
| Camera-relative movement + follow | ✅ | `PlayerMotor`, `InterviewArenaCameraFollow` |
| Coyote time, jump buffer, dash, i-frames | ✅ | Locomotion + combat |
| Cinemachine | ❌ | Свой follow |

### UI

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| MVP / Presenter | 🟡 | `HubPresenter`, `MetricsPresenter` |
| UI Toolkit | ❌ | UGUI + TMP |
| Safe area / responsive | ❌ | — |
| Data binding для HUD | 🟡 | Ручной bind buff/iframe HUD |

### Контент, ассеты, билд

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| Prefab Variants | 🟡 | Player/enemy prefabs |
| SO как data layer | ✅ | Weapon/player/enemy/backend configs |
| WebGL / IL2CPP заметки | 🟡 | `UnityClient/WEBGL.md`, build editor script |
| Editor content validation | ✅ | Validators, Setup Complete |
| Git для Unity (meta, LFS) | 🟡 | Implicit в workflow |
| CI (Unity + backend tests) | 🟡 | Backend tests локально; CI в `Roadmap.md` |

### Сеть и full-stack

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| REST + opaque session token | ✅ | `Backend/`, `SessionStorage` |
| Server authority для match result | ✅ | `POST /api/matches/{id}/result` |
| WebSocket / realtime sync | 🟡 | `connectUrl` stub; design в backlog |
| CORS + WebGL limits | 🟡 | Backend CORS config |
| Docker + Postgres readiness | 🟡 | `docker-compose.yml`, health `/health/ready` |
| API contract + automated tests | ✅ | `ApiContract.md`, 29+ backend tests |

### Геймдизайн-системы (Arena)

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| Damage pipeline (teams, i-frames, knockback) | ✅ | `CombatHitResolver`, `Health`, weapons |
| Buff/debuff + HUD/VFX | ✅ | `PlayerBuffController`, pickups |
| Spawn/respawn без лишнего Instantiate | 🟡 | Respawn; не pool |
| Win/lose / extraction loop | 🟡 | `FinishPortal`, match reporter |
| Явный boot order | 🟡 | `InterviewArenaBootstrap`, deferred gameplay wire |

### Editor tooling

| Тема | Статус | Где / комментарий |
|------|--------|-------------------|
| One-click scene setup | ✅ | `InterviewArenaSceneSetup` |
| Composition validators | ✅ | Validate Scene Composition |
| PropertyDrawer / CustomEditor | 🟡 | MenuItem-heavy; мало CustomEditor |
| EditMode + PlayMode tests | ✅ | Arena + Showcase tests |

### Процесс (soft skills на senior)

| Тема | Статус | Действие |
|------|--------|----------|
| Гипотеза → профайл → фикс → повторный замер | 🟡 | Оформить 1–2 кейса в доке |
| ADR / decisions | ✅ | `Docs/Decisions.md` |
| Trade-offs (почему не ECS/Zenject/NGO) | 📋 | Короткая секция в `Decisions.md` |
| Code review checklist для Unity | 📋 | Подписки, serialized refs, execution order |

---

## Приоритетный backlog

Задачи, которые **лучше всего** дополняют текущий проект (не generic tutorial).

### Tier A — максимум для демо и собеса

| ID | Задача | Закрывает пункты | Оценка | Зависимости |
|----|--------|------------------|--------|-------------|
| A1 | Loading screen + `LoadSceneAsync` + progress | 5, 6, 14 | 2–4 дня | — |
| A2 | Additive load/unload Interview Arena | 5 | 1–2 дня | A1 |
| A3 | World Space health bar (враги, опц. игрок) | 7 | 1–2 дня | — |
| A4 | Wave spawner (корутины) + enemy pool | 1, 16 | 3–5 дней | — |
| A5 | NavMesh AI для врагов | 17 | 3–5 дней | Arena scene bake |
| A6 | Profiler playbook + 2 скрина до/после | 12, 35 | 1 день | Stress-модули |

### Tier B — архитектурные паттерны «для ответов»

| ID | Задача | Закрывает пункты | Оценка | Зависимости |
|----|--------|------------------|--------|-------------|
| B1 | GameEvent + Listener + CustomEditor | 2, 19 | 2–3 дня | ✅ сделано |
| B2 | JSON settings в `persistentDataPath` | 4 | 1–2 дня | ✅ сделано |
| B3 | UniTask в online HTTP flow | 15 | 1–2 дня | — |
| B4 | Addressables: load Arena по ключу | 14 | 2–3 дня | A1 |
| B5 | Virtual scroll для lobby list | 10 | 2–3 дня | Online flow |

### Tier C — по времени и интересу

| ID | Задача | Закрывает пункты | Комментарий |
|----|--------|------------------|-------------|
| C1 | AI LOD (реже Update) | 9 | Быстрее NavMesh, если времени мало |
| C2 | Jobs/Burst (один кейс) | Jobs | VFX или spatial query |
| C3 | GPU Instancing showcase | 11 | Параллельно Object Pooling модулю |
| C4 | WebSocket lobby presence | 52 | После PostgreSQL / Фазы 5 |
| C5 | Zenject | 13 | Отдельный учебный проект, не обязателен |
| C6 | UI Toolkit экран | UI | Низкий приоритет |

---

## Как встроить в продуктовый Roadmap

Согласование с `Roadmap.md` — skills-задачи **вешать на уже идущие фазы**, а не отдельным «учебным спринтом».

```text
Сейчас (продукт)                    Skills, которые логично делать параллельно
─────────────────────────────────────────────────────────────────────────────
PostgreSQL persistence              B2 JSON settings; метрики для leaderboard UI
CI backend + Unity smoke            EditMode в CI; A6 profiler doc
Arena localization                  уже ✅ п.18; дописать Arena keys
WebSocket / multiplayer (Фаза 5)    C4; virtual lobby (B5)
WebGL deploy + Site                 A1–A2 loading; A6 WebGL profiler note
```

### Рекомендуемая очередь (part-time, 6–8 недель)

| Неделя | Продукт | Skills (из backlog) |
|--------|---------|---------------------|
| 1 | Shared patterns | **B1 GameEvent, B2 JSON settings** ← текущий спринт |
| 1–2 | CI smoke, WebGL hygiene | A6, A1 |
| 2–3 | Arena polish | A2, A3 |
| 3–4 | Gameplay depth | A4 |
| 4–5 | AI improvement | A5 |
| 6 | Online UX | B3, B4 или B5 |
| 7+ | Postgres / leaderboard | C4 design doc |

---

## Prerequisites — план до «тяжёлых» задач

Задачи A1–A2, B4, A5, B3, C4 **не начинать**, пока не закрыт соответствующий этап.

```text
Этап 0 — фундамент
    Setup Complete + Validate Scene (0 errors)
    Play Mode smoke: hub → Arena (offline + online skip)
    A6: profiler baseline (1 кейс + цифры в доке)
    CI: dotnet test + Unity EditMode (Arena)
         ↓
Этап 1 — A1 Loading screen (LoadSceneAsync, progress, allowSceneActivation)
         ↓
Этап 2 — A2 Additive Arena (+ unload при Back)
         ↓
Этап 3 — B4 Addressables load Arena по ключу
         ↓
Этап 4 — A5 NavMesh (bake + NavMeshAgent, FSM без изменений)
         ↓
Этап 5 — B3 UniTask (HTTP + online flow)
         ↓
Этап 6 — C4 WebSocket (Postgres + design doc + стабильный REST)
```

| Задача | Минимум до старта |
|--------|-------------------|
| A1–A2 Loading / additive | Этап 0 |
| B4 Addressables | Этап 1 (async + loading UI) |
| A5 NavMesh | Этап 0 + зафиксированная геометрия Arena |
| B3 UniTask | Стабильный online flow |
| C4 WebSocket | Postgres + design + REST |
| A4 Waves + pool | Этап 0; NavMesh не обязателен |
| **B1 GameEvent** | — (параллельно) |
| **B2 JSON settings** | — (параллельно) |

**Критерий Этапа 0:** можно менять `ShowcaseSceneLoader` и ловить регрессию тестами, а не вручную.

---

## Карта «где что лежит»

| Навык / тема | Путь |
|--------------|------|
| Object pooling (demo) | `UnityClient/Assets/Content/Modules/ObjectPooling/` |
| Object pooling (game) | `.../InterviewArena/Runtime/Combat/ProjectilePool.cs` |
| Enemy FSM | `.../AI/EnemyBrain.cs`, `EnemyFsmLogic.cs` |
| Composition root (Arena) | `.../Core/InterviewArenaRuntimeContext.cs` |
| Composition root (Hub) | `.../Showcase/Runtime/ShowcaseCompositionRoot.cs` |
| Online HTTP | `.../Runtime/Net/`, `.../Services/`, `InterviewArenaOnlineFlowController.cs` |
| Локализация | `.../Showcase/UI/ShowcaseLocalization.cs`, `Editor/ShowcaseLocalizationSetupTool.cs` |
| Editor setup | `.../Editor/InterviewArenaSceneSetup.cs` |
| GameEvent (SO) | `UnityClient/Assets/Content/Shared/Events/` |
| JSON settings | `UnityClient/Assets/Content/Shared/Settings/` |
| Backend API | `Backend/src/ExtractionRpg.Api/` |
| API contract | `Docs/ApiContract.md` |
| Arena setup runbook | `UnityClient/Docs/Modules/InterviewArena/SETUP.md` |
| WebGL | `UnityClient/WEBGL.md` |

---

## Как рассказывать на собесе

### Уже готово (не стесняйся углубляться)

- Пул болтов в production + stress-demo на 5000 объектов.
- FSM вынесен в `EnemyFsmLogic` с EditMode-тестами.
- Нет «божественного» синглтона в Arena — composition root и serialized wiring.
- Combat pipeline: teams, i-frames, knockback, buffs.
- REST: login → lobby → start → result; сервер владеет исходом матча.
- Локализация EN/RU через Unity Localization.
- Editor: one-click setup + validators до Play Mode.

### После Tier A (добавишь к истории)

- «Сцены гружу additive с loading UI и контролем `allowSceneActivation`.»
- «Враги на NavMesh; волны из пула, без Instantiate в бою.»
- «Вот замер: было X ms CPU / Y GC alloc → стало …»

### Trade-offs (если спросят «почему не X»)

| Технология | Позиция проекта |
|------------|-----------------|
| Zenject | Composition root + SerializeField достаточно для масштаба Arena; DI на backend — ASP.NET |
| ECS/DOTS | Gameplay на MonoBehaviour; stress-модули показывают понимание perf без полной миграции |
| Netcode for GameObjects | Сначала REST + server authority; realtime — отдельная фаза |
| UI Toolkit | Hub и Arena на UGUI/TMP; Toolkit — не блокер для текущих целей |

---

## Что сознательно не тащить в один репозиторий

- Отдельный «tutorial scene» на каждый пункт чеклиста.
- Zenject + ECS + NGO одновременно «для галочки».
- Полный MMORPG lobby с сотнями игроков в UI без реальной потребности.
- Дублирование паттернов (GameEvent **и** глобальный EventBus **и** SO variables для одного и того же).

**Правило:** новый skill-пункт должен **усиливать** Showcase, Arena или Online — иначе в backlog Tier C или «нет».

---

## Обновление документа

При закрытии пункта:

1. Поменять статус в таблице (❌/🟡 → ✅).
2. Добавить путь к коду или `Docs/...` с замерами.
3. При крупной фиче — строка в `Roadmap.md` и при необходимости `Decisions.md`.

---

## Связанные документы

| Документ | Назначение |
|----------|------------|
| `Roadmap.md` | Продуктовые фазы (backend, online, deploy) |
| `ApiContract.md` | Контракт API |
| `ClientServerFlow.md` | Login → match → result |
| `Decisions.md` | Архитектурные решения (D-WS-*) |
| `UnityClient/Docs/OpenThreads.md` | Активные UI/showcase задачи |
| `UnityClient/Docs/Modules/InterviewArena/docs/04_BACKLOG.md` | Arena milestones |
| `UnityClient/Docs/Modules/InterviewArena/SETUP.md` | Runbook Unity |

*Последнее согласование с кодовой базой: май 2026.*
