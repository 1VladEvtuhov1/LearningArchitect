using System;
using System.Collections.Generic;

namespace LearningArchitect.IndieEffects
{
    public struct Entity
    {
        public int Id;

        public Entity(int id)
        {
            Id = id;
        }

        public bool IsValid
        {
            get { return Id >= 0; }
        }
    }

    public struct Health
    {
        public float Value;
        public float MaxValue;
    }

    public struct Shield
    {
        public float Value;
        public float MaxValue;
    }

    public struct Movement
    {
        public float BaseSpeed;
        public float CurrentSpeed;
    }

    public struct DamageOverTime
    {
        public int Target;
        public float DamagePerSecond;
        public float Duration;
    }

    public struct HealOverTime
    {
        public int Target;
        public float HealPerSecond;
        public float Duration;
    }

    public struct ShieldOverTime
    {
        public int Target;
        public float ShieldPerSecond;
        public float Duration;
    }

    public struct SlowModifier
    {
        public int Target;
        public float Multiplier;
        public float Duration;
    }

    public struct IndieEffectMetrics
    {
        public int entities;
        public int dots;
        public int heals;
        public int shields;
        public int slows;
        public int processedEffects;
        public int appliedEntities;
        public float damageApplied;
        public float healingApplied;
        public float shieldApplied;

        public void ResetFrame()
        {
            processedEffects = 0;
            appliedEntities = 0;
            damageApplied = 0f;
            healingApplied = 0f;
            shieldApplied = 0f;
        }
    }

    public sealed class SimulationData
    {
        public readonly List<Health> Healths;
        public readonly List<Shield> Shields;
        public readonly List<Movement> Movements;

        public readonly List<DamageOverTime> Dots;
        public readonly List<HealOverTime> Heals;
        public readonly List<ShieldOverTime> ShieldEffects;
        public readonly List<SlowModifier> Slows;

        public readonly List<float> DamageAccumulator;
        public readonly List<float> HealAccumulator;
        public readonly List<float> ShieldAccumulator;
        public readonly List<float> SlowMultiplierAccumulator;

        public SimulationData(int entityCapacity = 64, int effectCapacity = 128)
        {
            Healths = new List<Health>(entityCapacity);
            Shields = new List<Shield>(entityCapacity);
            Movements = new List<Movement>(entityCapacity);

            Dots = new List<DamageOverTime>(effectCapacity);
            Heals = new List<HealOverTime>(effectCapacity);
            ShieldEffects = new List<ShieldOverTime>(effectCapacity);
            Slows = new List<SlowModifier>(effectCapacity);

            DamageAccumulator = new List<float>(entityCapacity);
            HealAccumulator = new List<float>(entityCapacity);
            ShieldAccumulator = new List<float>(entityCapacity);
            SlowMultiplierAccumulator = new List<float>(entityCapacity);
        }
    }

    public sealed class SimulationSystem
    {
        private readonly SimulationData _data;
        private IndieEffectMetrics _metrics;

        public SimulationData Data
        {
            get { return _data; }
        }

        public IndieEffectMetrics Metrics
        {
            get { return _metrics; }
        }

        public SimulationSystem(int entityCapacity = 64, int effectCapacity = 128)
        {
            _data = new SimulationData(entityCapacity, effectCapacity);
        }

        public Entity CreateEntity(float health, float maxShield = 0f, float baseSpeed = 1f)
        {
            if (health < 0f)
                health = 0f;
            if (maxShield < 0f)
                maxShield = 0f;

            int id = _data.Healths.Count;

            Health healthData;
            healthData.Value = health;
            healthData.MaxValue = health;

            Shield shieldData;
            shieldData.Value = 0f;
            shieldData.MaxValue = maxShield;

            Movement movementData;
            movementData.BaseSpeed = baseSpeed;
            movementData.CurrentSpeed = baseSpeed;

            _data.Healths.Add(healthData);
            _data.Shields.Add(shieldData);
            _data.Movements.Add(movementData);
            _data.DamageAccumulator.Add(0f);
            _data.HealAccumulator.Add(0f);
            _data.ShieldAccumulator.Add(0f);
            _data.SlowMultiplierAccumulator.Add(1f);

            return new Entity(id);
        }

        public bool AddDot(Entity target, float damagePerSecond, float duration)
        {
            if (!IsValidTarget(target.Id) || damagePerSecond <= 0f || duration <= 0f)
                return false;

            DamageOverTime effect;
            effect.Target = target.Id;
            effect.DamagePerSecond = damagePerSecond;
            effect.Duration = duration;
            _data.Dots.Add(effect);
            return true;
        }

        public bool AddHeal(Entity target, float healPerSecond, float duration)
        {
            if (!IsValidTarget(target.Id) || healPerSecond <= 0f || duration <= 0f)
                return false;

            HealOverTime effect;
            effect.Target = target.Id;
            effect.HealPerSecond = healPerSecond;
            effect.Duration = duration;
            _data.Heals.Add(effect);
            return true;
        }

        public bool AddShield(Entity target, float shieldPerSecond, float duration)
        {
            if (!IsValidTarget(target.Id) || shieldPerSecond <= 0f || duration <= 0f)
                return false;

            ShieldOverTime effect;
            effect.Target = target.Id;
            effect.ShieldPerSecond = shieldPerSecond;
            effect.Duration = duration;
            _data.ShieldEffects.Add(effect);
            return true;
        }

        public bool AddSlow(Entity target, float multiplier, float duration)
        {
            if (!IsValidTarget(target.Id) || duration <= 0f)
                return false;

            if (multiplier < 0f)
                multiplier = 0f;
            if (multiplier > 1f)
                multiplier = 1f;

            SlowModifier effect;
            effect.Target = target.Id;
            effect.Multiplier = multiplier;
            effect.Duration = duration;
            _data.Slows.Add(effect);
            return true;
        }

        public void Tick(float dt)
        {
            if (dt < 0f)
                dt = 0f;

            _metrics.ResetFrame();
            _metrics.entities = _data.Healths.Count;
            _metrics.dots = _data.Dots.Count;
            _metrics.heals = _data.Heals.Count;
            _metrics.shields = _data.ShieldEffects.Count;
            _metrics.slows = _data.Slows.Count;

            ResetAccumulators();
            Compute(dt);
            Apply();
            Cleanup();
        }

        public Health GetHealth(Entity entity)
        {
            if (!IsValidTarget(entity.Id))
                return default;

            return _data.Healths[entity.Id];
        }

        public Shield GetShield(Entity entity)
        {
            if (!IsValidTarget(entity.Id))
                return default;

            return _data.Shields[entity.Id];
        }

        public Movement GetMovement(Entity entity)
        {
            if (!IsValidTarget(entity.Id))
                return default;

            return _data.Movements[entity.Id];
        }

        private void Compute(float dt)
        {
            ComputeDamageOverTime(dt);
            ComputeHealOverTime(dt);
            ComputeShieldOverTime(dt);
            ComputeSlowModifiers(dt);
        }

        private void ComputeDamageOverTime(float dt)
        {
            for (int i = 0; i < _data.Dots.Count; i++)
            {
                DamageOverTime effect = _data.Dots[i];
                _data.DamageAccumulator[effect.Target] += effect.DamagePerSecond * dt;
                effect.Duration -= dt;
                _data.Dots[i] = effect;
                _metrics.processedEffects++;
            }
        }

        private void ComputeHealOverTime(float dt)
        {
            for (int i = 0; i < _data.Heals.Count; i++)
            {
                HealOverTime effect = _data.Heals[i];
                _data.HealAccumulator[effect.Target] += effect.HealPerSecond * dt;
                effect.Duration -= dt;
                _data.Heals[i] = effect;
                _metrics.processedEffects++;
            }
        }

        private void ComputeShieldOverTime(float dt)
        {
            for (int i = 0; i < _data.ShieldEffects.Count; i++)
            {
                ShieldOverTime effect = _data.ShieldEffects[i];
                _data.ShieldAccumulator[effect.Target] += effect.ShieldPerSecond * dt;
                effect.Duration -= dt;
                _data.ShieldEffects[i] = effect;
                _metrics.processedEffects++;
            }
        }

        private void ComputeSlowModifiers(float dt)
        {
            for (int i = 0; i < _data.Slows.Count; i++)
            {
                SlowModifier effect = _data.Slows[i];
                float currentMultiplier = _data.SlowMultiplierAccumulator[effect.Target];
                if (effect.Multiplier < currentMultiplier)
                    _data.SlowMultiplierAccumulator[effect.Target] = effect.Multiplier;

                effect.Duration -= dt;
                _data.Slows[i] = effect;
                _metrics.processedEffects++;
            }
        }

        private void Apply()
        {
            for (int i = 0; i < _data.Healths.Count; i++)
            {
                Health health = _data.Healths[i];
                Shield shield = _data.Shields[i];
                Movement movement = _data.Movements[i];

                float shieldGain = _data.ShieldAccumulator[i];
                float damage = _data.DamageAccumulator[i];
                float healing = _data.HealAccumulator[i];
                float slowMultiplier = _data.SlowMultiplierAccumulator[i];
                float incomingDamage = damage;
                bool changed = false;

                if (shieldGain > 0f)
                {
                    float before = shield.Value;
                    shield.Value += shieldGain;
                    if (shield.Value > shield.MaxValue)
                        shield.Value = shield.MaxValue;

                    float actualShieldGain = shield.Value - before;
                    _metrics.shieldApplied += actualShieldGain;
                    if (actualShieldGain != 0f)
                        changed = true;
                }

                if (damage > 0f)
                {
                    float absorbed = damage;
                    if (absorbed > shield.Value)
                        absorbed = shield.Value;

                    shield.Value -= absorbed;
                    damage -= absorbed;
                    if (absorbed != 0f)
                        changed = true;

                    if (damage > 0f)
                    {
                        float before = health.Value;
                        health.Value -= damage;
                        if (health.Value < 0f)
                            health.Value = 0f;

                        float actualHealthDamage = before - health.Value;
                        _metrics.damageApplied += actualHealthDamage;
                        if (actualHealthDamage != 0f)
                            changed = true;
                    }
                }

                if (healing > 0f)
                {
                    float before = health.Value;
                    health.Value += healing;
                    if (health.Value > health.MaxValue)
                        health.Value = health.MaxValue;

                    _metrics.healingApplied += health.Value - before;
                    if (health.Value != before)
                        changed = true;
                }

                movement.CurrentSpeed = movement.BaseSpeed * slowMultiplier;
                if (movement.CurrentSpeed != movement.BaseSpeed)
                    changed = true;

                if (changed ||
                    incomingDamage != 0f ||
                    healing != 0f ||
                    shieldGain != 0f ||
                    slowMultiplier != 1f)
                {
                    _metrics.appliedEntities++;
                }

                _data.Healths[i] = health;
                _data.Shields[i] = shield;
                _data.Movements[i] = movement;
            }
        }

        private void Cleanup()
        {
            RemoveExpiredDots();
            RemoveExpiredHeals();
            RemoveExpiredShields();
            RemoveExpiredSlows();
        }

        private void RemoveExpiredDots()
        {
            for (int i = _data.Dots.Count - 1; i >= 0; i--)
            {
                if (_data.Dots[i].Duration <= 0f)
                    RemoveAtSwapBack(_data.Dots, i);
            }
        }

        private void RemoveExpiredHeals()
        {
            for (int i = _data.Heals.Count - 1; i >= 0; i--)
            {
                if (_data.Heals[i].Duration <= 0f)
                    RemoveAtSwapBack(_data.Heals, i);
            }
        }

        private void RemoveExpiredShields()
        {
            for (int i = _data.ShieldEffects.Count - 1; i >= 0; i--)
            {
                if (_data.ShieldEffects[i].Duration <= 0f)
                    RemoveAtSwapBack(_data.ShieldEffects, i);
            }
        }

        private void RemoveExpiredSlows()
        {
            for (int i = _data.Slows.Count - 1; i >= 0; i--)
            {
                if (_data.Slows[i].Duration <= 0f)
                    RemoveAtSwapBack(_data.Slows, i);
            }
        }

        private void ResetAccumulators()
        {
            for (int i = 0; i < _data.Healths.Count; i++)
            {
                _data.DamageAccumulator[i] = 0f;
                _data.HealAccumulator[i] = 0f;
                _data.ShieldAccumulator[i] = 0f;
                _data.SlowMultiplierAccumulator[i] = 1f;
            }
        }

        private bool IsValidTarget(int id)
        {
            return (uint)id < (uint)_data.Healths.Count;
        }

        private static void RemoveAtSwapBack<T>(List<T> list, int index)
        {
            int last = list.Count - 1;
            list[index] = list[last];
            list.RemoveAt(last);
        }
    }
}
