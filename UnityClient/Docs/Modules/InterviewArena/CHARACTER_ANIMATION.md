# Character Animation — Interview Arena

Источник правды по аниматору и связанным механикам **игрока и врагов** на арене. Showcase-модуль (`HumanoidCrowdActor` / варианты Run-Shoot) описан отдельно в [`../../HumanoidAnimationSetup.md`](../../HumanoidAnimationSetup.md).

Код драйвера: `ArenaHumanoidVisual`.  
Контракт клипов и имён параметров: `HumanoidAnimationProfileSO`.  
Контроллер: `Animation_PaladinCombat.controller` (один на игрока и врагов).

## Animation Policy — Interview Arena

Interview Arena использует **in-place / arcade combat**.

- Gameplay-код владеет боем: movement locks, attack windows, hit frames, i-frames, dash, hitstun и action priority.
- Animator не решает `CanMove`, `CanAttack`, `CanDash`, hit frames или invulnerability. Animator только показывает уже принятое gameplay-состояние.
- Root Motion на arena-акторах выключен. Position и yaw принадлежат коду.
- `ViewPivot`, `VisualAnchor` и `LogicalBodyForward` нельзя смешивать:
  - `ViewPivot.forward` — cursor / muzzle / aim pivot;
  - `LogicalBodyForward` — locomotion, melee commit;
  - `VisualAnchor` — presentation mesh, компенсирующий parent `ViewPivot`.
- Arena-презентация — **только меч**: Longsword idle / walk / turn / melee / dash / hit. Стрельба в Animator не гоняется.
- Melee использует committed body facing, а не raw mesh forward.
- IK на арене выключен. Он не источник gameplay-направления.
- Controller общий для showcase / arena / Vampire / Paladin, поэтому параметры явно разделены на arena live, showcase live и debug/unused. Параметры из controller **не удалять**.

## Animator Parameter Ownership

Один `Animation_PaladinCombat.controller` обслуживает Arena и showcase. Класс ниже — кто **имеет право писать** параметр, не список полей YAML.

| Класс | Параметры | Кто пишет | Что делать |
|------|-----------|-----------|------------|
| Arena live | `MoveX`, `MoveY`, `TurnInPlace`, `TurnDirection`, `Jump`, `JumpBackward`, `Dash`, `ComboRight1`, `ComboRight2`, `ComboRight3`, `Melee`, `Melee2`, `IsBlocking`, `BlockStart`, `Hit`, `HitFront`, `HitBack`, `HitLeft`, `HitRight` | `ArenaHumanoidVisual` / gameplay systems | Писать в Arena |
| Showcase live | `Speed`, `MoveX`, `MoveY`, `Turn`, `IsMoving`, `IsGrounded`, `IsShooting` | `HumanoidCrowdActor` | Оставить для showcase |
| Unused in Arena | `IsBowStance`, `Fire`, `Speed`, `Turn`, `TurnAngle`, `AimYaw`, `IsMoving`, `IsGrounded`, `IsShooting` | Arena не пишет (`IsBowStance` один раз ставится в `false` при bind) | Не строить на них новую логику Arena |

`MoveX` / `MoveY` входят и в Arena, и в showcase — оба драйвера пишут их в свой runtime.  
`Speed`, `Turn`, `IsMoving`, `IsGrounded` живые для showcase и одновременно unused для Arena.

`ArenaHumanoidVisual` пишет только Arena live. Параметры в controller не удалены: `Fire` / `IsBowStance` остаются для совместимости shared controller.

---

## Кто есть кто

| Роль | Ассет / компонент |
|------|-------------------|
| Игрок (меш) | Vampire Knight — `AnimationActor_VampireCombat.prefab` |
| Игрок (профиль) | `Animation_VampireCombatProfile.asset` на `InterviewArenaPlayer` |
| Враги (меш) | Paladin — `AnimationActor_PaladinCombat.prefab` |
| Враги (профиль) | `Animation_PaladinCombatProfile.asset` |
| Общий Animator | `Animation_PaladinCombat.controller` |
| Avatar игрока | `Vampire.fbx` (Humanoid) |
| Avatar врагов | Paladin из `ShootingAndModel.fbx` |

Профили почти идентичны по параметрам. Разница — `actorPrefab` и `avatar`. Контроллер, клипы и имена параметров общие.

---

## Иерархия и владельцы поворота

```text
InterviewArenaPlayer          ← Rigidbody / capsule (физика, не крутится к курсору)
└── ViewPivot                 ← ArenaCursorAim крутит к курсору (aim yaw)
    └── VisualAnchor          ← ArenaHumanoidVisual задаёт world yaw = LogicalBodyForward
        └── AnimationActor_*  ← Animator + HumanoidCrowdActor
            ├── меш / кости
            └── WeaponSocket_R / Longsword   ← всегда виден
            └── WeaponSocket_L / Bow         ← inactive
```

Три разных «вперёд», их нельзя смешивать:

| Вектор | Кто владеет | Для чего |
|--------|-------------|----------|
| `transform.forward` корня | физика / камера | не корпус |
| `ViewPivot.forward` | `ArenaCursorAim` | курсор, muzzle |
| `LogicalBodyForward` | `ArenaHumanoidVisual.currentBodyRotation` | локомоция, melee commit |

`VisualAnchor` — child `ViewPivot`. World-yaw якоря компенсирует поворот pivot:

`localRotation = Inverse(ViewPivot) * LogicalBodyRotation`.

`Apply Root Motion` = **off**. Yaw корпуса принадлежит коду, не клипу.

---

## Порядок кадра (контракт)

| Order | Компонент | Что делает |
|------:|-----------|------------|
| −25 | `MeleeStrikeController` | старт удара, `SnapPlanarFacing` |
| 50 | `ArenaCursorAim` | `AimDirection`, поворот `ViewPivot` |
| 75 | `BowAimDirectionProvider` | clamp курсора к `LogicalBodyForward` (±75°) — gameplay fire, не Animator |
| 110 | `ArenaHumanoidVisual` | facing, Animator params (melee) |

Визуал читает aim **после** обновления pivot в том же `LateUpdate`.

---

## Animator Controller

Файл: `Assets/Content/Modules/LayeredCharacterAnimation/Animations/Animation_PaladinCombat.controller`.

### Слои

| Слой | Вес | IK Pass | Mask | Назначение |
|------|-----|---------|------|------------|
| `Base Locomotion` | 1 | да | — | локомоция, jump/dash/melee/hit/turn |
| `Upper Body Shooting` | **0**, Arena не поднимает | нет | `Animation_PaladinUpperBody.mask` | пустой `No Upper Override`; слой не удалять |

### Base Locomotion — состояния

Default: **`Melee Locomotion`**.

| Состояние | Motion | Вход | Выход |
|-----------|--------|------|-------|
| `Melee Locomotion` | 2D Freeform `MoveX`/`MoveY` (Longsword) | default / `!IsBowStance` | turn / Any State |
| `Bow Locomotion` | те же Longsword-клипы, что melee | `IsBowStance` (Arena не пишет `true`) | stance / turn / Any State |
| `Turn In Place` | 1D blend `TurnDirection` (−1 / +1) | `TurnInPlace` из loco | `!TurnInPlace` → melee (или bow, если stance true) |
| `Jump Forward` | jump clip | Any State: `Jump` && `!JumpBackward` | exit time → loco |
| `Jump Backward` | jump-back clip | Any State: `Jump` && `JumpBackward` | exit time → loco |
| `Dash` | `LS_SprintBash` | Any State: `Dash` | exit time → loco |
| `Combo Right 1` | `LS_ComboRight_1` | Any State: `ComboRight1` / `Melee` | exit time → loco |
| `Combo Right 2` | `LS_ComboRight_2` | Any State: `ComboRight2` / `Melee2` | exit time → loco |
| `Combo Right 3` | `LS_ComboRight_3` | Any State: `ComboRight3` | exit time → loco |
| `Block Loop` | `LongsLP_BlockLoop` | `IsBlocking` / `BlockStart` | `!IsBlocking` → melee loco |
| `Hit Front` | `Longs_HitFront_p` | Any State: `HitFront` | exit time → loco |
| `Hit Back` | `Longs_Hit_Back` | Any State: `HitBack` | exit time → loco |
| `Hit Left` | `LS_HitLegsLeft` | Any State: `HitLeft` | exit time → loco |
| `Hit Right` | `LS_HitLegsRight` | Any State: `HitRight` / `Hit` | exit time → loco |

### Blend trees локомоции

Оба 2D Freeform Directional, оси `MoveX` (strafe) и `MoveY` (fwd/back). Центр `(0,0)` — idle. **Одинаковые Longsword-клипы.**

| Ячейка | Melee / Bow Locomotion |
|--------|------------------------|
| (0, 0) | `LongsLP_Idle1` |
| (0, 1) | `LongsLP_WalkFwd` |
| (0, −1) | `LongsLP_WalkBwd` |
| (−1, 0) | `LongsLP_WalkLeft` |
| (1, 0) | `LongsLP_WalkRight` |

Mixamo Longbow-клипы в Arena-контроллере **не стоят**. Пакет на диске можно не трогать.

### Upper Body Shooting

Слой оставлен (shared controller / mask). Default weight 0. Единственное состояние: `No Upper Override` (пусто). `Fire` / `Bow Upper Idle` из графа сняты. Параметры `Fire` / `IsBowStance` в controller остаются.

---

## Параметры Animator — детализация

Имена живут в `HumanoidAnimationProfileSO` (не хардкодить строки в геймплее). Владение — в таблице **Animator Parameter Ownership** выше. Здесь — как считается значение.

### Arena live (пишет `ArenaHumanoidVisual`)

| Параметр | Тип | Откуда | Кто читает в контроллере |
|----------|-----|--------|--------------------------|
| `MoveX` | Float | скорость вправо относительно `LogicalBodyForward` | 2D blend loco |
| `MoveY` | Float | скорость вперёд относительно корпуса | 2D blend loco |
| `TurnInPlace` | Bool | `BodyFacingRules.ShouldPlayTurnClip` | вход/выход `Turn In Place` |
| `TurnDirection` | Float | знак yaw (−1 влево / +1 вправо), damp | 1D blend turn clips |
| `IsBlocking` | Bool | `MeleeBlockController.IsBlocking` | вход `Block Loop` |

### Триггеры Arena live (edge, не hold)

| Параметр | Источник геймплея | Когда |
|----------|-------------------|-------|
| `Jump` | `PlayerMotor.IsJumpAnimActive` rising edge | плюс `JumpBackward` bool |
| `JumpBackward` | `PlayerMotor.JumpAnimBackward` | ставится **до** `Jump` |
| `Dash` | `PlayerMotor.IsDashAnimActive` rising edge | |
| `ComboRight1` | `Light1.AnimTriggerOverride` | первый удар комбо |
| `ComboRight2` | `Light2.AnimTriggerOverride` | второй удар |
| `ComboRight3` | `Light3.AnimTriggerOverride` | третий удар |
| `Melee` | fallback профиля, если override пуст; alias на Combo Right 1 | Heavy / legacy |
| `Melee2` | alias на Combo Right 2 | не писать из нового контента |
| `BlockStart` | `MeleeBlockController.BlockStartVersion` | rising edge входа в блок |
| `HitFront` / `HitBack` / `HitLeft` / `HitRight` | `Health.Damaged` + `HitReactDirectionRules` | сторона относительно `LogicalBodyForward` |
| `Hit` | alias на Hit Right | legacy / showcase |

`MeleeAnimStartVersion` нужен, чтобы повтор в том же кадре (end→restart) снова пульнул trigger.

### Unused in Arena (не строить логику; из controller не удалять)

`ArenaHumanoidVisual` эти поля не пишет (кроме одноразового `IsBowStance = false` при bind). Showcase (`HumanoidCrowdActor`) продолжает писать свой набор независимо.

| Параметр | Тип | Заметка |
|----------|-----|---------|
| `IsBowStance` | Bool | Arena ставит `false` при bind. Default loco — melee. |
| `Fire` | Trigger | Arena не пульсирует. Gameplay crossbow может жить отдельно. |
| `Speed` | Float | Arena loco на `MoveX`/`MoveY`. Showcase 1D/2D speed. |
| `Turn` | Float | угол скорость↔корпус / 90. Arena blend tree не читает. |
| `IsMoving` | Bool | Arena переходы не читают. Showcase читает. |
| `IsGrounded` | Bool | Arena jump — trigger, не этот bool. Showcase пишет `true`. |
| `TurnAngle` | Float | сырой yaw; состояния не читают. |
| `AimYaw` | Float | бывший IK; состояния не читают. |
| `IsShooting` | Bool | Arena не пишет. Showcase live. |

---

## Механики, завязанные на анимацию

### Стойка

`PlayerCombatStance` (MeleeReady ↔ BowAim) может оставаться **gameplay-toggle**. На Animator это не влияет: визуал всегда melee, меч виден, лук скрыт (`WeaponSocket_L` inactive).

### Facing корпуса

`BodyFacingRules.ResolveDesiredForward`:

| Состояние | Куда смотрит корпус |
|-----------|---------------------|
| Idle | сырой курсор (`ArenaCursorAim.AimDirection`) |
| Движение | planar velocity |

Порог idle: `\|v_xz\| < movingThreshold * speedNormalization`.

Код крутит `currentBodyRotation` со скоростью:

- idle turn: `turnInPlaceDegreesPerSecond` (240) **только пока** `isTurningInPlace`;
- move: `movingTurnDegreesPerSecond` (720).

Старт turn-in-place: idle и `|yaw| > idleTurnStartAngle` (20°).  
Стоп: `|yaw| < idleTurnStopAngle` (8°) дольше `idleTurnExitHoldTime` (0.2 с).

`BodyFacingRules.ShouldPlayTurnClip`: клип играется, пока `isTurningInPlace` и нет action lock.

### Upper-body aim IK

`HumanoidUpperBodyAimIk` на акторе **выключен**. Arena его не требует и не крутит. Поля профиля `maxUpperBodyAimDegrees` / `turnInPlaceAimIkWeight` остаются в SO (gameplay clamp / legacy), визуал их для IK не использует.

### Выстрел (gameplay, не Animator)

`CrossbowWeaponController` / `BowAimDirectionProvider` могут стрелять по clamped aim. Визуал **не** пульсирует `Fire` и не включает upper layer. Если болт летит — персонаж выглядит как melee.

### Melee

`InterviewArena_MeleeWeapon.asset` — LMB-цепочка `Light1 → Light2 → Light3` (Heavy на индексе 1, в комбо не входит):

| Strike | Trigger | Клип | Длительность | Next |
|--------|---------|------|--------------|------|
| Light1 | `ComboRight1` | `LS_ComboRight_1` | 0.8 с | Light2 |
| Heavy | `Melee` (пусто) | Combo Right 1 | 1.35 с | — |
| Light2 | `ComboRight2` | `LS_ComboRight_2` | 0.85 с | Light3 |
| Light3 | `ComboRight3` | `LS_ComboRight_3` | 0.95 с | — |

Перед ударом `IBodyFacingCommit.SnapPlanarFacing` — мгновенный yaw к цели.  
`movementLockDuration` = длительность удара: WASD и поворот корпуса выключены на весь свинг (`CanMove` / `CanTurn` false). Visual считает action active, обнуляет MoveX/Y. Иначе после короткого lock зажатый S даёт velocity назад → корпус разворачивается.

Враги используют тот же controller и те же стейты Combo Right. `EnemyMeleeAttack` поднимает `MeleeAnimStartVersion` и trigger `ComboRight1`; `ArenaHumanoidVisual` пульсирует его так же, как player melee. Дальний выстрел (melee-only presentation) пульсирует `Melee` (alias Combo Right 1). Новый стейт в Animator не нужен: клипы уже стоят.

`EnemyConfig.attackWindup` (0.24 с) совпадает со стартом hit window игрока Light1 (`0.3 × 0.8` с) на том же клипе. Стейт `Combo Right 1` играет на скорости 1.8 — более длинный windup бьёт уже в recovery клипа. Flash на старте замаха нет: swing VFX в момент overlap, hit flash — из `Health.PlayDamageHit`.

Удар игрока **отменяет** windup врага (`EnemyMeleeAttack.Interrupt`) и включает hitstun (`EnemyConfig.hitstunDuration`, 0.85 с). Пока stun активен, `EnemyBrain` не начинает новый удар и не стреляет. Hit-стейты — презентация; cancel удара принадлежит геймплею, не Animator.

Шаг корпуса — **код, не root motion**. `MeleeStrikeStep` задаёт постоянную planar-скорость на окне `strikeStepStart`…`strikeStepEnd` (дистанция в метрах на удар). Направление — committed strike forward. `ArenaHoverMotor` / `PlayerMotor` не обнуляют XZ, пока `IsStrikeStepActive` (как dash).

| Strike | Шаг | Окно |
|--------|-----|------|
| Light1 | 0.42 м | 0.02–0.22 с |
| Light2 | 0.55 м | 0.02–0.26 с |
| Light3 | 0.78 м | 0.04–0.34 с |

Стейты и клипы ставит **Setup Paladin Melee Combo**. `Melee` / `Melee2` в controller не удалять (alias на удары 1–2).

### Block

Hold **C / Left Ctrl** (геймпад: LT). ПКМ — по-прежнему арбалет.

`MeleeBlockController` — hold-to-block. Пока блок активен: нет WASD, можно поворачиваться и выйти дэшем. ЛКМ снимает блок и бьёт. Удар / hitstun / прыжок блок не дают.

Фронтальный конус (`blockConeHalfAngleDegrees`, 70°) поглощает удар: 0 HP, без knockback и hitstun, остаёшься в `Block Loop`. Удар сбоку/сзади проходит → направленный hit react, блок сбрасывается.

`HitDirection` в `DamageInfo` — куда летит удар. Входящая сторона = противоположность. `HitReactDirectionRules` мапит на Front/Back/Left/Right относительно `LogicalBodyForward`.

Стейты ставит **Setup Paladin Guard And Hit Reacts**. `Hit` не удалять (alias на Hit Right).

Клипы Arena без Animation Events: vendor `SendEvent` на `Longs_Hit_Back` снимается импортом. Hit windows — код, не клип.

### Jump / Dash / Hit

Таймеры анимации живут в геймплее (`PlayerMotor` jump/dash anim timer, `Health.Damaged`), visual только поднимает trigger на rising edge. Локомоция на это время замирает (`IsActionAnimActive`).

Рывок: средние `dashIframeCoverage` (0.85) длительности поглощают обычный урон (`PlayerDashIframeGuard` / `IIncomingHitGuard`). Старт и хвост (~7.5% каждый) уязвимы. Поглощённый удар не снимает HP, не даёт hitstun и не прерывает рывок. Animator не владеет i-frames.

---

## Tuning в `HumanoidAnimationProfileSO`

Текущие значения Vampire/Paladin (менять в ассете, не в коде):

| Поле | Значение | Эффект |
|------|----------|--------|
| `speedNormalization` | 1.35 | при какой скорости MoveX/Y = 1 |
| `parameterDampTime` | 0.08 | damp float-параметров Animator |
| `movingThreshold` | 0.08 | idle vs move (после нормализации) |
| `visualYawOffsetDegrees` | 0 | clip packs already +Z |
| `maxUpperBodyAimDegrees` | 75 | gameplay fire clamp (не IK) |
| `upperBodyAimSmoothSpeed` | 14 | legacy IK, Arena не читает |
| `upperBodyAimWeightBlendSpeed` | 12 | legacy IK, Arena не читает |
| `idleTurnStartAngle` | 20 | старт turn-in-place |
| `idleTurnStopAngle` | 8 | стоп turn-in-place |
| `idleTurnExitHoldTime` | 0.2 | антидребезг стопа |
| `turnInPlaceDegreesPerSecond` | 240 | скорость кода в idle |
| `turnInPlaceAnimatorDampTime` | 0.08 | damp `TurnDirection` |
| `turnInPlaceAimIkWeight` | 0.65 | legacy, Arena IK выключен |
| `movingTurnDegreesPerSecond` | 720 | скорость кода на ходу |
| `applyRootMotion` | false | обязательно |
| `randomizeStartTime` | true | фаза idle при `Initialize` |

`visualYawOffsetMeleeDegrees` — legacy, Arena не использует.

---

## Пакеты клипов

| Пакет | Путь | Что даёт Arena |
|-------|------|----------------|
| Longsword Update pt1 | `LongswordAnimsetPro` | `LongsLP_Idle1`, WalkFwd/Bwd/Left/Right |
| Longsword legacy pt1 | то же | `Longs_TurnL_90`, `Longs_TurnR_90` |
| Longsword Update pt3 | то же | `LS_ComboRight_1`, `LS_HitLegsRight` |
| Longsword Update pt4 | то же | `LS_SprintBash` (dash) |
| Legacy shooter FBX | `LayeredCharacterAnimation/Models/` | jump forward / backward |
| Mixamo Longbow | `Assets/Content/Prefabs/LongbowAimingPack/` | **не используется** Arena-контроллером |

Vampire-префаб из `LongswordAnimsetPro/Prefabs/Vampire.prefab` содержит ragdoll/Cloth. `VampireCombatActorSetup.StripVisualPhysics` снимает Joint / Rigidbody / Collider / Cloth. На рантайме физика только у капсулы игрока.

---

## Editor menus

Пересобирать контроллер/риг, а не править YAML руками:

| Меню | Скрипт |
|------|--------|
| `Learning Architect/Animation/Setup Longbow Combat Controller` | `LongbowAnimationSetup` — melee default, upper shooting stripped |
| `Setup Longbow Aiming Pack Rig` | импорт Mixamo (pack на диске; Arena его не играет) |
| `Setup Longsword Turn-In-Place (Rig + Controller)` | turn clips + state |
| `Apply Longsword Update Actions To Paladin Controller` | melee / dash / hit клипы |
| `Setup Paladin Melee Combo` | `PaladinMelee2AnimatorSetup` — Combo Right 1/2/3 = `LS_ComboRight_*` |
| `Setup Paladin Guard And Hit Reacts` | `PaladinGuardAnimatorSetup` — Block Loop + Hit Front/Back/Left/Right |
| `Setup Vampire As Player Visual` | актор + профиль + bake на игрока |
| `Setup Paladin Combat Animation` | базовый showcase Paladin |
| `Equip Longsword + Speed Melee` | сокет меча, speed стейта Melee |
| `Equip Bow Prop` | `WeaponSocket_L` + prop, **socket inactive** |

После смены prefab/profile: **Learning Architect → Interview Arena → Setup Complete**, затем Validate.

---

## Следующий визуальный выигрыш (не код)

Драйвер сужен до melee Arena live. Дальше — **Editor / Preview / Play Mode** по мечу:

1. Vampire melee: idle / walk F-B-L-R / turn / attack / dash / hit.
2. Те же клипы на Paladin, если retarget сомнителен.
3. Ручной Preview/Play Mode по melee combo 1–2–3.

Порядок работ:

1. Policy + таблица ownership — этот документ.
2. Melee-only `ArenaHumanoidVisual` — сделано (параметры из controller не удалены).
3. Ручной Preview/Play Mode по melee.
4. Только потом [`docs/05_GAME_FEEL_PLAN.md`](docs/05_GAME_FEEL_PLAN.md).

---

## Инварианты

1. Один Animator на акторе. Root Motion выключен.
2. Корпус = `LogicalBodyForward`, не `transform.forward` и не кости клипа.
3. Arena не пишет `Fire` и не ставит `IsBowStance` в `true`. Default state — `Melee Locomotion`.
4. Turn-клип играется в idle turn, пока нет action lock.
5. Visual не делает `Find*` / `GetComponent` в `Update`/`LateUpdate`. Ссылки — Awake / setup / serialized.
6. На visual-акторе нет своих Rigidbody/Cloth. Nested `HumanoidCrowdActor` обязателен на префабе. `HumanoidUpperBodyAimIk` может лежать на префабе, но **disabled**. Игрок: **Bake Humanoid Visual Into Player Prefab**. Враги: **Bake Humanoid Visual Into Enemy Prefab**. Runtime `AddComponent` / `Instantiate` нет.
7. Меч (`WeaponSocket_R`) виден. Лук (`WeaponSocket_L`) скрыт.

### Debug gizmos (`ArenaHumanoidVisual`)

| Цвет | Смысл |
|------|--------|
| зелёный | `LogicalBodyForward` + конус ±75° (gameplay clamp) |
| голубой | cursor aim |
| серый | forward капсулы |
| розовый | forward меша |
| жёлтый | planar velocity |

---

## Тесты (Edit Mode)

| Тест | Что фиксирует |
|------|----------------|
| `BodyFacingRulesTests` | idle → aim, move → velocity, turn-клип при turn, не при action lock |
| `BowAimDirectionProviderTests` | clamp к logical facing, не к корню (gameplay fire) |
| `MeleeComboTransitionTests` | Light2 пишет `Melee2` |
| `MeleeStrikeInterruptTests` | version++ при рестарте |
| `EnemyMeleeAttackTests` | враг поднимает `ComboRight1` version |

Play Mode (ручной smoke):

1. Меч, стоишь: Longsword idle, корпус доворачивает к курсору, turn-клип без отъезда.
2. Меч, бежишь: walk F-B-L-R относительно корпуса.
3. Combo LMB: `ComboRight1` → `ComboRight2` → `ComboRight3`, разные клипы.
4. Jump / dash / hit не оставляют залипший trigger.
5. Лук в руках не виден. Mixamo aim/recoil не играет, даже если gameplay всё ещё стреляет.
6. Враг в melee: `ComboRight1` / `LS_ComboRight_1`, не idle на всём swing.

---

## Связанные файлы

```text
Assets/Content/Modules/InterviewArena/Scripts/Player/
  ArenaHumanoidVisual.cs
  BodyFacingRules.cs
  BowAimDirectionProvider.cs
  ArenaCursorAim.cs
  PlayerCombatStance.cs
  PlayerMotor.cs
  IBodyFacingProvider.cs

Assets/Content/Modules/InterviewArena/Scripts/Combat/
  MeleeStrikeController.cs
  CrossbowWeaponController.cs

Assets/Content/Modules/LayeredCharacterAnimation/Scripts/
  HumanoidAnimationProfileSO.cs
  HumanoidCrowdActor.cs          ← init Animator + showcase params
  HumanoidUpperBodyAimIk.cs      ← disabled on Arena actors
  HumanoidLocomotionAnimation.cs
  HumanoidPlanarAimMath.cs

Assets/Content/Editor/
  LongbowAnimationSetup.cs
  LongswordAnimationSetup.cs
  VampireCombatActorSetup.cs
  PaladinMelee2AnimatorSetup.cs
```
