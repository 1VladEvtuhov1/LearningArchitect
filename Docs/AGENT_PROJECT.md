# Agent Project Appendix — LearningArchitect / ExtractionRPG

Дополнение к [`Engineering_Handbook.md`](Engineering_Handbook.md). Только для этого репозитория.

---

## Workspace

| Слой | Путь |
|------|------|
| Unity client | `UnityClient/` — **открывать в Unity Hub отсюда** |
| Arena runtime | `UnityClient/Assets/Content/Modules/InterviewArena/` |
| Arena editor | `UnityClient/Assets/Content/Editor/` (Setup, Validator) |
| Showcase hub | `UnityClient/Assets/Content/Showcase/` |
| Shared patterns | `UnityClient/Assets/Content/Shared/` |
| Backend | `Backend/src/` |
| Product docs | `Docs/` (корень репо) |
| Unity module docs | `UnityClient/Docs/Modules/InterviewArena/` |
| Portfolio / WebGL shell | `Site/` |

**Не править** `Assets/` в корне репо — актуальные ассеты под `UnityClient/Assets/Content/`.

---

## Планы и статус

| Документ | Назначение |
|----------|------------|
| `Docs/Roadmap.md` | Продуктовые фазы (backend, WebGL, CI) |
| `Docs/SkillsRoadmap.md` | Skills checklist, Tier A/B/C, очередь задач |
| `UnityClient/Docs/Modules/InterviewArena/SETUP.md` | Setup Complete, Validate, Play smoke |
| `UnityClient/Docs/OpenThreads.md` | Открытые ветки showcase |

---

## Interview Arena — editor wiring

После изменений prefab / scene / locomotion:

1. **Learning Architect → Interview Arena → Setup Complete**  
2. **Validate Scene Composition** — **0 errors**  
3. Play Mode smoke: движение, aim, crossbow, FSM врагов, buff HUD  

### Правила wiring

- `SerializeField` заполняются **editor setup** или `InterviewArenaRuntimeContext.TryWirePlayerAndCamera()`, не `Find*` в `Awake`  
- Missing reference → `InterviewArenaAuthoringLog.MissingReference`, не silent fallback  
- Камера и `arenaFloor` часто на **scene instance**, не в prefab asset  
- Projectile pool — на `InterviewArenaCombatServices` (scene), не в prefab игрока  

### Online vs offline

- `InterviewArenaRuntimeContext.ShouldDeferGameplayWire()` — при online flow wiring откладывается  
- Реальный wiring: `InterviewArenaOnlineFlowController.WireGameplayIfNeeded()` после skip/login  
- Баг «игрок не двигается только в online» → проверить deferred wire, не `PlayerMotor` вслепую  

---

## Battlerite locomotion (Arena)

| Компонент | Ответственность |
|-----------|-----------------|
| `ArenaHoverMotor` | Hover physics, ground probe, arena clamp |
| `ArenaCursorAim` | Aim по курсору, поворот `ViewPivot` |
| `PlayerMotor` | Dash в hover-режиме; классический motor без hover |
| `ArenaHumanoidVisual` | Paladin visual, animator params |
| `CrossbowWeaponController` | Выстрел по `muzzle.forward` (child of `ViewPivot`) |

### Execution order

- **Один владелец aim yaw:** `ArenaCursorAim` → `ViewPivot`  
- Визуал не должен читать aim **до** обновления pivot в том же кадре  
- При «отстаёт на кадр» — сначала execution order / порядок `LateUpdate`, не второй поворот тела  
- `[DefaultExecutionOrder]` — только когда порядок кадра часть контракта  

---

## Существующий API — не переписывать

| Задача | Использовать |
|--------|----------------|
| Scene / prefab wiring | `InterviewArenaSceneSetup`, `WirePlayerLocomotionReferences` |
| Runtime wire + spawn | `InterviewArenaRuntimeContext.TryWirePlayerAndCamera()` |
| Composition validation | `InterviewArenaSceneCompositionValidator` |
| Authoring errors | `InterviewArenaAuthoringLog` |
| Wall slide (player + enemy) | `PlanarLocomotionCollision` |
| Combat hit pipeline | `CombatHitResolver`, `CombatStrikeUtility` |
| Crossbow pool + config | `InterviewArenaCombatServices.WirePlayerCrossbow` |
| Buffs | `PlayerBuffController`, `BuffKind`, `BuffPickup` |
| Enemy AI | `EnemyBrain` FSM + `EnemyMotor` / attacks |
| Game events | `UnityClient/Assets/Content/Shared/Events/` (`GameEvent`) |
| JSON settings | `UnityClient/Assets/Content/Shared/Settings/` |
| Pure logic tests | `UnityClient/Assets/Content/Modules/InterviewArena/Tests/EditMode/Editor/` |
| Online HTTP | `InterviewArenaOnlineFlowController`, `AuthService`, `BackendApiConfig` |
| Result type | `Result` / `ApiResult<T>` — явный failure для wiring и API |

### Паттерны конфигурации

- `ScriptableObject` config + `ApplyConfig(...)` на runtime-компонентах  
- Bootstrap (`InterviewArenaBootstrap`) — thin entry, делегирует в `RuntimeContext`  

---

## Meta и ассеты

- Не удалять `.meta` вручную без пересоздания asset  
- Битый GUID в `.meta` ломает компиляцию всего проекта  
- Новые скрипты — дождаться Unity-generated `.meta`, не копировать GUID  

---

## Backend

- Auth / lobby / match — REST в `Backend/src/`  
- Match results пока in-memory; Postgres — по `Docs/Roadmap.md`  
- Integration tests: `EXTRACTIONRPG_INTEGRATION=1` (см. backend README)  

---

## Типичные ошибки агента в этом репо

- Пути вне `UnityClient/Assets/Content/`  
- Runtime `Find` / null-guard вместо Setup Complete + Validate  
- Второй поворот тела вместо fix execution order aim  
- Новый `IArenaMoveInput` / LocomotionService вместо расширения `PlayerMotor` / `ArenaHoverMotor`  
- Loading / Addressables / NavMesh «заодно» с багфиксом locomotion  
- Коммит / push без явной просьбы  
- Unity Package Manager / Unity ID — инфраструктура редактора, не баг проекта  
- Удаление debug-instrumentation до подтверждения fix (в debug-сессиях Cursor)  

---

## Рекомендуемая очередь (кратко)

```text
Этап 0: Setup Complete + Validate + Play smoke + profiler baseline
    → A1 Loading screen
    → A2 Additive Arena
    → Backend Postgres + CI
    → A5 NavMesh, B3 UniTask, B4 Addressables
```

Подробно: `Docs/SkillsRoadmap.md`.

---

## Git

- Коммиты и PR — **только по явной просьбе** пользователя  
- Не amend / force-push без явного запроса  
