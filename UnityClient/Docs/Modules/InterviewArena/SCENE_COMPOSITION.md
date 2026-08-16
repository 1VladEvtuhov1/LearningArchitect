# Interview Arena — правила компоновки сцены

Канонический документ: **как строить и именовать иерархию сцены**, куда класть runtime-объекты и как разделять ответственность.

Связанные документы:

- `CODE_ARCHITECTURE.md` — код, системы, wiring
- `SETUP.md` — меню Unity и первый запуск
- `Assets/Content/Showcase/Prefabs/ArchitectureShowcaseHub.prefab` — эталон UI-нейминга

---

## TL;DR

- Сцена делится по **ответственности** и **времени жизни** (`Persistent` / `Scene` / `Runtime`).
- **Root-секции** (`Level`, `Gameplay`, `Actors`, `Runtime`, `UI`) — только контейнеры; логика живёт на дочерних системах. Исключение: composition root вроде `InterviewArenaBootstrap`.
- **Временные Runtime-объекты** имеют parent в `Runtime/*`; корень сцены остаётся чистым.
- **Gameplay** — системы и сервисы, включая управление пулом. **Runtime** — снаряды, VFX, пикапы, temporary objects и pooled instances.
- **Actors** — игровые сущности с поведением: игроки, враги, training targets. Spawn-маркеры — в `Level/SpawnPoints`.
- **UI** — несколько Canvas по частоте обновления; виджеты именуются через `Container -` / `Image -` / `Text -` / `Button -`.
- **Префабы акторов** не содержат scene-owned сервисы и пулы.
- **Scene context** — все корневые ссылки в `InterviewArenaRuntimeContext` через serialized refs. `Find` в runtime gameplay запрещён.
- **Scene Composition Validator** проверяет структуру сцены перед merge / playtest.

---

## Главный принцип

> **Иерархия сцены — инструмент разработки и дебага.**

Hierarchy должна быстро отвечать на три вопроса:

| Вопрос | Пример ответа |
|--------|----------------|
| **Что это по роли?** | `SpawnPoint_Player_01` |
| **Кто владеет / создаёт / удаляет?** | `PlayerSpawnSystem` → `Actors/Players` |
| **Как долго живёт?** | сцена / runtime / между сценами |

Плохой сигнал: по Hierarchy нельзя понять назначение объекта без открытия Inspector.

---

## Время жизни (lifecycle)

| Слой | Где живёт | Примеры |
|------|-----------|---------|
| **Persistent** | `DontDestroyOnLoad` / отдельная bootstrap-сцена | Audio, Save, Network session |
| **Scene** | Текущая `.unity` сцена | Level, Gameplay, UI roots, камеры |
| **Runtime** | Создаётся в Play Mode | снаряды, VFX, пикапы, `(Clone)` |

**Правило:** объекты с **runtime lifetime** не вшиваются в prefab актора, если ими владеет сцена: пулы, общий VFX, scene services.

---

## Базовая структура сцены

Шаблон для gameplay-сцен: гонка, арена, лобби-уровень.

```text
--- BOOTSTRAP (опционально, между сценами) ---
App
Services

--- SCENE: InterviewArena (пример) ---
Level
  Static
    PreviewPlatform
    Walls
    Props_Static
  Dynamic
    Doors
    MovingPlatforms
    Interactables
  SpawnPoints
    SpawnPoint_Player_01
    SpawnPoint_Enemy_01

Gameplay
  InterviewArenaBootstrap      ← composition root, wiring (исключение для root)
  ArenaCombatServices          ← сервисы сцены: pool policy, hit feedback
    PlayerCrossbowBoltPool     ← компонент ProjectilePool; сами болты хранятся в Runtime
  RaceController               ← когда появится гонка
  CheckpointSystem

Actors
  InterviewArenaPlayer         ← scene instance
  Enemy_HollowSoldier_01
  TrainingDummy_01

Runtime
  Projectiles                  ← экземпляры болтов: inactive + active
  VFX
  Pickups
  TemporaryObjects

Cameras
  MainCamera

Lighting
  Directional Light

UI
  Canvas_Static
  Canvas_HUD_Dynamic
  Canvas_Popups
  Canvas_Debug                 ← только dev / playtest
```

**Глубина:** для 90% объектов — до **4 уровней** от корня секции. Большая глубина допустима для скелета, сложного UI и технических prefab-структур.

### Root-секции: только контейнеры

Объекты верхнего уровня (`Level`, `Actors`, `Runtime`, `UI`, `Cameras`, `Lighting`) — **пустые организационные roots**:

- без gameplay-логики на самом root-объекте;
- без MonoBehaviour с игровым поведением;
- исключение: явные composition roots, например `InterviewArenaBootstrap` под `Gameplay`.

**Плохо:** `RaceController` висит на объекте `Level`.  
**Хорошо:** `Gameplay/RaceController`, а дети `Level` содержат геометрию и маркеры.

### Активность root-объектов

Root-контейнеры **не отключают целиком** (`SetActive(false)` на `Runtime` / `UI` / `Level`), если внутри живут ссылки bootstrap, пулы или объекты, которыми владеют системы.

Для включения/выключения режима используйте:

- конкретные системы: `RaceController`, popup Canvas;
- дочерние группы: `Canvas_Popups`, `Enemy_HollowSoldier_01`;
- состояние компонентов: `enabled`, state machine, visibility flags.

---

## Scene Context

Все **корневые ссылки сцены** собираются в одном composition root — `InterviewArenaRuntimeContext` на `InterviewArenaBootstrap`.

| Поле / зона | Назначение |
|-------------|------------|
| `levelRoot` | `Level`: геометрия, static/dynamic окружение, spawn markers |
| `actorsRoot` | `Actors`: игроки, враги, training targets |
| `runtimeRoot` | `Runtime`: projectiles, VFX, pickups, temporary objects |
| `projectilesRoot` | `Runtime/Projectiles`: active + inactive projectile instances |
| `vfxRoot` | `Runtime/VFX`: hit effects, impact effects, temporary visuals |
| `canvasHudDynamic` | `UI/Canvas_HUD_Dynamic`: часто обновляемый HUD |
| `spawnPoint` | authored позиция игрока |
| `player` | `PlayerMotor` instance в сцене |
| `combatServices` | `ArenaCombatServices`: пулы, feedback |
| `cameraFollow` | follow на Main Camera |
| `playerConfig` | SO-тюнинг |

Системы получают ссылки через **bootstrap / serialized refs**: `TryWirePlayerAndCamera`, `WirePlayerCrossbow`.

**Запрещено в runtime gameplay:**

- `FindObjectOfType` / `GameObject.Find` / поиск по строке имени для roots и актора;
- скрытая зависимость от имени объекта в Hierarchy;
- автоматический поиск сервисов в `Update`, `FixedUpdate`, gameplay-методах.

Допустимый fallback: editor tools, одноразовая migration-команда, временный bootstrap для старой сцены. Такой fallback помечается как технический долг.

---

## Level: Static / Dynamic / SpawnPoints

### `Level/Static`

Объекты, которые **по геймдизайну не двигаются** в runtime:

- пол, стены, здания, статичный декор
- коллайдеры уровня

**Важно:** папка `Static` и галочка `Static` в Inspector — разные вещи.  
`Static` / `Batching Static` / `Lightmap Static` включаются **осознанно** под lighting и batching.

### `Level/Dynamic`

Объекты, которые уже есть в сцене, но меняют состояние в игре:

- двери;
- платформы;
- рычаги;
- триггеры с анимацией;
- интерактивные props.

### `Level/SpawnPoints`

Только **маркеры данных** (Transform + тонкий marker-компонент при необходимости):

```text
SpawnPoint_Player_01
SpawnPoint_Enemy_A
```

Спавн-поинты находятся в `Level/SpawnPoints`. Созданный персонаж попадает в `Actors`: игроки, враги и training targets хранятся там даже при runtime spawn.

---

## Gameplay

Системы и координаторы режима. Визуальные mesh, персонажи и сами экземпляры снарядов хранятся в профильных секциях.

| Хорошо | Плохо |
|--------|-------|
| `RaceController` | `GameManager` со всей логикой |
| `ArenaCombatServices` | `Manager` |
| `InterviewArenaBootstrap` | логика боя внутри UI |
| `PlayerCrossbowBoltPool` (компонент пула) | десятки `CrossbowBolt_*` как дети сервиса без `Runtime/Projectiles` |

### Gameplay vs Runtime: снаряды

Разделяйте **сервис** и **экземпляры**:

```text
Gameplay/ArenaCombatServices/PlayerCrossbowBoltPool
  → ProjectilePool: ёмкость, prefab, cooldown policy, wire в CrossbowWeaponController

Runtime/Projectiles
  → GameObject-ы болтов: inactive pool + активные в полёте
```

| Слой | Что хранит |
|------|------------|
| **Gameplay** | кто и как стреляет, лимиты пула, конфиг, wiring |
| **Runtime** | физические объекты, которые появляются, отключаются, переиспользуются или уничтожаются в Play Mode |

**Правило:** сами болты хранятся под `Runtime/Projectiles`, даже если рядом в `Gameplay` лежит `ProjectilePool`.

Pooled inactive objects всё равно считаются runtime-owned instances. Поэтому выключенные болты, заранее созданные пулом, тоже лежат в `Runtime/Projectiles`.

**MVP / миграция:** если болты временно дочерние к pool host, помечайте это как долг и переносите instances под `Runtime/Projectiles`.

### Ownership игроков

Игроки и враги хранятся в `Actors`, даже если созданы во время игры. Их игровая роль важнее способа создания.

```text
Actors/InterviewArenaPlayer        ← authored scene instance
Actors/Players/Player_Local        ← runtime-spawned local player
Actors/Players/Player_Remote_01    ← runtime-spawned network player
Actors/Enemies/Enemy_Melee_01      ← runtime-spawned enemy
```

`Runtime` используется для временных gameplay-объектов: снарядов, VFX, pickups, temporary objects и pooled instances.

Для **Interview Arena MVP** игрок — **prefab instance в сцене**, поле `Player` на `InterviewArenaRuntimeContext` заполнено вручную или через editor setup.

---

## Actors

Персонажи и цели, с которыми взаимодействует геймплей:

```text
Actors/
  InterviewArenaPlayer
  Enemy_HollowSoldier_01
  TrainingDummy_01
```

**Нейминг:** `{Role}_{Variant}_{Index}`.

```text
Enemy_Melee_01
Enemy_Ranged_02
TrainingDummy_01
Player_Local
Player_Remote_01
```

**После стабилизации сцены** в Play Mode Hierarchy **не должно оставаться**:

```text
Enemy(Clone) / Bullet(Clone) в корне сцены
GameObject
десятки объектов без parent в Runtime/*
```

Кратковременный `(Clone)` при отладке spawn допустим. Целевое состояние: parent в `Actors/` или `Runtime/*`, осмысленное имя.

---

## Runtime

Всё, что **создаётся, отключается, переиспользуется или уничтожается** во время игры. Обязательны **root-контейнеры**:

```text
Runtime/
  Projectiles/
  VFX/
  Pickups/
  TemporaryObjects/
```

В коде — явные ссылки через composition root:

```csharp
public Transform ProjectilesRoot { get; }
public Transform VfxRoot { get; }
```

### Interview Arena — обязательно

| Объект | Где |
|--------|-----|
| `ProjectilePool` (компонент) | `Gameplay/ArenaCombatServices/PlayerCrossbowBoltPool` |
| `CombatHitFeedback` | `Gameplay/ArenaCombatServices` |
| Экземпляры болтов | `Runtime/Projectiles`: inactive + active |
| VFX удара | `Runtime/VFX` или procedural через `CombatHitFeedback` |

Меню миграции: **Learning Architect → Interview Arena → Migrate Scene Composition (Pools Off Player)**.

---

## UI

### Canvas по частоте обновления

| Canvas | Содержимое | Частота изменений |
|--------|------------|-------------------|
| `Canvas_Static` | фон, рамки, заголовки | редко |
| `Canvas_HUD_Dynamic` | таймер, HP, i-frames, счётчики | часто |
| `Canvas_Popups` | пауза, результат, ошибки | по событию |
| `Canvas_Debug` | FPS, net stats | dev only |

**Почему:** изменение `RectTransform` / текста может вызывать rebuild Canvas. Динамический HUD держится отдельно от тяжёлой статичной вёрстки.

### Нейминг UI-виджетов

В проекте принят префикс **типа виджета**:

```text
Container - InterviewArenaCanvas
Container - BackButton
Container - PlayerHealthHud
Container - PlayerIframeHud
Button - StartRace
Button - Back
Image - Background
Image - Fill
Text - Title
Text - Subtitle
Text - RaceTimer
Text - PlayerName
```

Формат:

```text
{WidgetType} - {SemanticName}
```

Где `WidgetType` ∈ `Container`, `Button`, `Image`, `Text`, `Slider`, `Toggle`, `Input`, …

**Плохо:**

```text
Title
Label
Background
New Text
UI_Button_StartRace
```

**Хорошо:**

```text
Text - Title
Text - RaceTimer
Button - StartRace
Image - HealthFill
Container - PlayerHud
```

### Нейминг gameplay-объектов

Для игровых объектов используется роль в игре:

```text
Trigger_FinishLine
Checkpoint_01
Door_LobbyExit
SpawnPoint_Player_01
Enemy_Melee_01
```

Шаблоны:

```text
Role_Variant_Index
Type_Role_Index
```

UI и gameplay-объекты используют разные стили нейминга:

```text
UI:       Button - StartRace
Gameplay: Trigger_FinishLine
```

---

## Prefab актора

### Имена prefab assets и scene instances

Имя prefab asset показывает, что это asset в проекте. Имя scene instance показывает роль конкретного объекта в сцене.

```text
Prefab asset:
  PF_Player_InterviewArena
  PF_Projectile_CrossbowBolt
  PF_VFX_HitSpark
  PF_Enemy_HollowSoldier

Scene instance:
  InterviewArenaPlayer
  PlayerCrossbowBoltPool
  Enemy_HollowSoldier_01
  TrainingDummy_01
```

Правило: `PF_` используется для prefab assets в Project window. В Hierarchy используются имена ролей без `PF_`.

### Внутренняя структура prefab актора

Префаб = подсистемы, сгруппированные по ответственности.

```text
InterviewArenaPlayer
  View/
    ViewPivot/
      CrossbowMuzzle
  Physics/
    CapsuleCollider          ← или на root, если один collider
    Rigidbody
  Locomotion/
    PlayerMotor
    GroundDetector
    PlayerInputReader
  Combat/
    Health
    PlayerCombat
    MeleeStrikeController
    CrossbowWeaponController ← ссылка на scene pool
```

**В prefab игрока запрещены:**

- `ProjectilePool` и десятки `CrossbowBolt_*`;
- scene-only сервисы;
- `Canvas` всего HUD, кроме world-space nameplate при необходимости.

---

## Камеры и свет

```text
Cameras/
  MainCamera                 ← + InterviewArenaCameraFollow

Lighting/
  Directional Light
  Reflection Probes          ← при необходимости
```

---

## Мультиплеер

Дополнительные roots добавляются без изменения single-player layout:

```text
Network/
  NetworkRunner
  NetworkSession

Actors/
  Players/
    Player_Local
    Player_Remote_01
  Enemies/
    Enemy_Networked_01

Runtime/
  NetworkedProjectiles
  NetworkedPickups
```

Runtime-spawned игроки остаются в `Actors/Players`, потому что это игровые сущности с поведением.

---

## Scene Composition Validator

Цель validator-а — автоматически проверять правила документа перед merge / playtest.

Меню:

```text
Learning Architect → Interview Arena → Validate Scene Composition
```

Минимальные проверки:

- существуют root-секции `Level`, `Gameplay`, `Actors`, `Runtime`, `UI`, `Cameras`, `Lighting`;
- root-секции не содержат лишние MonoBehaviour, кроме явных composition roots;
- `InterviewArenaBootstrap` содержит заполненный `InterviewArenaRuntimeContext`;
- `Actors/Players` существует, если сцена использует runtime-spawned игроков или мультиплеер;
- `Runtime/Projectiles` и `Runtime/VFX` существуют;
- `ProjectilePool` находится в `Gameplay/ArenaCombatServices/PlayerCrossbowBoltPool`;
- экземпляры болтов находятся под `Runtime/Projectiles`;
- `ProjectilePool` отсутствует внутри `InterviewArenaPlayer` prefab instance;
- `Canvas_Static`, `Canvas_HUD_Dynamic`, `Canvas_Popups`, `Canvas_Debug` существуют;
- новые UI-виджеты используют формат `WidgetType - SemanticName`;
- в корне сцены нет `(Clone)`, `GameObject`, `New Text`, `Cube` без осмысленного имени;
- `Runtime/Players` отсутствует; игроки и враги лежат в `Actors`;
- roots `Runtime` / `UI` / `Level` активны и не используются как runtime toggle.

Результат validator-а: список ошибок и предупреждений с путём объекта в Hierarchy.

```text
ERROR: Runtime/Projectiles is missing
ERROR: ProjectilePool found under Actors/InterviewArenaPlayer/Combat
WARN: Text object uses default name: UI/Canvas_HUD_Dynamic/New Text
```

---

## Антипаттерны

| ❌ | ✅ |
|----|-----|
| `Cube`, `GameObject`, `Image`, `New Text` | `Wall_ArenaNorth`, `Image - Frame`, `Text - RaceTimer` |
| Пул снарядов в prefab игрока | `ArenaCombatServices` + wire в bootstrap |
| 200 объектов в корне сцены | `Runtime/Projectiles`, `Runtime/VFX` |
| `FindObjectOfType` для roots | Serialized refs на `InterviewArenaRuntimeContext` |
| Один Canvas на всё UI | Static / Dynamic / Popups |
| `UI_Button_StartRace` | `Button - StartRace` |
| `Manager` на всё | `RaceController`, `ArenaCombatServices` |
| Иерархия глубже 6–8 уровней без причины | ≤ 4 уровня в типичном случае |

---

## Чеклист перед merge / playtest

- [ ] Сцена читается в Hierarchy без Play Mode
- [ ] Root-секции без лишних MonoBehaviour, кроме composition root
- [ ] Временные runtime-объекты имеют parent в `Runtime/*`
- [ ] Игроки, враги и training targets лежат в `Actors`, включая runtime-spawned instances
- [ ] `ProjectilePool` в `Gameplay`, экземпляры болтов — в `Runtime/Projectiles`
- [ ] Inactive pooled objects лежат в runtime-root своей категории
- [ ] Игрок — instance в `Actors`, ссылки на `InterviewArenaRuntimeContext` заполнены
- [ ] Пул отсутствует внутри prefab Player; wire идёт через `ArenaCombatServices`
- [ ] UI: `Container -` / `Button -` / `Image -` / `Text -` для новых виджетов
- [ ] Динамический HUD не лежит в `Canvas_Static` с тяжёлой вёрсткой
- [ ] Нет «висящих» `(Clone)` / безымянных объектов в корне сцены после 5 минут геймплея
- [ ] Roots `Runtime` / `UI` / `Level` не выключаются целиком при паузе/HUD toggle
- [ ] **Validate Scene Composition** проходит без ошибок
- [ ] **Setup Complete** или **Migrate Scene Composition** прогнаны после смены структуры

---

## Unity Editor — как применить в проекте

| Действие | Меню |
|----------|------|
| Создать / обновить сцену арены | **Learning Architect → Interview Arena → Create Or Update Arena Scene** |
| Полный прогон | **Setup Complete** |
| Перенести пул с игрока на сцену | **Migrate Scene Composition (Pools Off Player)** |
| Обновить prefab игрока без пула | **Create Player Prefab Asset** |
| Поставить игрока в сцену | **Wire Scene Player From Prefab** |
| Проверить структуру сцены | **Validate Scene Composition** |

---

## Финальное разделение sections

```text
Actors    → кто играет: player, enemies, NPC, training targets
Runtime   → что временно появляется: projectiles, VFX, pickups, temporary objects
Gameplay  → кто управляет правилами: controllers, services, bootstrap, pools
Level     → где всё происходит: static/dynamic geometry, spawn markers
UI        → как это показывается: Canvas_Static, HUD, popups, debug
```

## Короткая формулировка для собеседования

> Я делю сцену по **ответственности и времени жизни**: статичное окружение, динамические объекты, gameplay-системы, акторы, runtime-spawned временные объекты, UI, камеры и свет. Игроки и враги лежат в `Actors`, а снаряды, VFX и pickups — в `Runtime`. UI разделяю на Canvas по частоте обновления; виджеты именую по типу и роли, например `Text - RaceTimer` и `Button - StartRace`. Префабы актора держу чистыми: пулы и сервисы сцены живут в scene services и связываются через bootstrap. Так Hierarchy остаётся читаемой в Play Mode и проще дебажится.