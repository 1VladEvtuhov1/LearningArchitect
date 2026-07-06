# Interview Arena

**Interview Arena** — отдельная сцена в `LearningArchitect`: dark-fantasy арена для подготовки к собеседованию (физика, бой, FSM-враги, pooling, i-frames). Multiplayer/backend — в backlog.

Каноническая документация: эта папка. Код: `Assets/Content/Modules/InterviewArena/`.

## Текущий статус (MVP1–3, локально)

| Реализовано | Описание |
|-------------|----------|
| Отдельная сцена | `InterviewArena` + dock на hub (D-008) |
| Locomotion | WASD, jump, dash, slopes, arena clamp |
| Combat | Melee + crossbow, knockback, i-frames |
| AI | Hollow Soldier: Patrol → Chase → Attack → Ranged |
| Respawn | Враги возвращаются на spawn pose |
| Tests | EditMode: FSM, combat rules, locomotion math, i-frames |
| WebGL | NonAlloc, layers, Brotli, лёгкие примитивы |

## Быстрый старт в Unity

1. **Learning Architect → Interview Arena → Setup Complete (Prefabs + Scene + Wire Player)**
2. Play на сцене `InterviewArena` или зайти через dock с hub.
3. Управление: WASD, Space, Shift, J/LMB (melee), R/RMB (crossbow).

Подробнее: `SETUP.md`, `CODE_ARCHITECTURE.md`, `SCENE_COMPOSITION.md`.

## Reading order

1. `SCENE_COMPOSITION.md` — hierarchy, naming, runtime roots, UI Canvas rules  
2. `CODE_ARCHITECTURE.md` — runtime layers, pipelines, WebGL checklist  
3. `SETUP.md` — editor menus  
4. `INTEGRATION_ASSESSMENT.md` — fit в репозиторий  
5. `docs/04_BACKLOG.md` — lobby, backend, multiplayer  
6. `docs/05_GAME_FEEL_PLAN.md` — полировка отклика персонажа  

```text
Assets/Content/Modules/InterviewArena/
├── Data/           # PlayerConfig, weapons, EnemyConfig
├── Prefabs/        # Player, enemy, bolt
├── Runtime/        # Core, Player, Physics, Combat, AI, UI
├── Scenes/         # InterviewArena.unity
└── Tests/EditMode/
```

## Cursor rules

`.cursor/rules/interview-arena.mdc`

## Assessment

See `INTEGRATION_ASSESSMENT.md`.
