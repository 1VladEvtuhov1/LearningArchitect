# 05. Game Feel — план полировки отклика

Roadmap по **combat feel / commitment / input forgiveness** для Interview Arena.
Итерации маленькими PR → Play Mode smoke после каждого.

**Уже в коде:** coyote time, jump cut, dash, bow/melee stance, turn-in-place, upper-body IK,
melee snap + startup root, hit react trigger, i-frames, strike lunge, **input buffer TTL (без consume rules)**.

**Принцип:** сначала **action lock / priority**, потом буфер и полировка анимаций.
Пока нет единого `CanMove?` / `CanAttack?` — не трогаем combo, directional melee, foot IK, fast turn.

---

## PR 1 — Action lock & priority gates ✅

| # | Задача | Статус | Файлы |
|---|--------|--------|-------|
| 1.1 | `CharacterActionLock` + `PlayerActionCoordinator` | ✅ | `Runtime/Player/CharacterAction*.cs` |
| 1.2 | Melee → startup / full-strike locks | ✅ | `MeleeStrikeController` |
| 1.3 | Dash / Jump locks | ✅ | `PlayerMotor` |
| 1.4 | Hitstun lock при уроне | ✅ | `PlayerHealthHitstun`, `PlayerConfig` |
| 1.5 | Gates: motor + combat не consume при block | ✅ | `PlayerMotor`, `PlayerCombat`, `ArenaHoverMotor` |
| 1.6 | Visual читает busy только для animator / IK | ✅ | `ArenaHumanoidVisual` |

**Приоритет (высший выигрывает при consume):** `dash > hitstun > attack > jump > locomotion`

**DoD:** нельзя dash + melee + jump одновременно; урон прерывает dash; буфер не consume при block.

---

## PR 2 — Input buffer consume rules

| # | Задача | Статус |
|---|--------|--------|
| 2.1 | Consume только у разрешённого действия с наивысшим приоритетом | ⬜ |
| 2.2 | Jump buffer (до приземления, поверх coyote) | ⬜ |

**DoD:** `melee → dash` в одном окне → срабатывает dash, melee остаётся в буфере или истекает.

---

## PR 3 — Melee recovery slow

| # | Задача | Статус |
|---|--------|--------|
| 3.1 | Фазы startup / active / recovery в lock + SO | ⬜ |
| 3.2 | Recovery: движение замедлено, не frozen | ⬜ |

---

## PR 4 — Hitstun polish

| # | Задача | Статус |
|---|--------|--------|
| 4.1 | Speed penalty + turn lock тюнинг | ⬜ |
| 4.2 | Симметрия с attack commitment | ⬜ |

---

## PR 5 — Locomotion damp & остальное

| # | Задача | Статус |
|---|--------|--------|
| 5.1 | Locomotion damp при входе в action | ⬜ |
| 5.2 | Bow movement penalty | ⬜ |
| 5.3 | Recoil micro-root | ⬜ |
| 5.4 | Dash i-frames, hit stop | ⬜ |

---

## Не трогать до PR 3

- Directional melee, combo chain, foot IK  
- Fast turn вместо snap, turn-in-place доработки  
- Soft aim assist, camera punch  

---

## Тюнинг вручную (Play Mode)

`InterviewArena_PlayerConfig`, `InterviewArena_MeleeWeapon`:

- attack startup / recovery, dash duration, hitstun duration  
- input buffer TTL, recovery speed multiplier, IK weight during action  

---

## Порядок работы

1. PR → smoke (WASD, aim, ЛКМ/ПКМ, dash, jump, получение урона).  
2. Обновить статус в таблице.  
3. Тюнинг SO глазами.

См. [`04_BACKLOG.md`](04_BACKLOG.md), [`SETUP.md`](../SETUP.md).
