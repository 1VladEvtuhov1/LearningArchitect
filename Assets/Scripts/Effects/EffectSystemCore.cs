using System;
using System.Diagnostics;

namespace LearningArchitect.Effects
{
    public struct EffectMetrics
    {
        public int totalEffects;
        public int scheduledEffects;
        public int processedEffects;
        public int appliedEntities;
        public int appliedDamage;
        public int dirtyEntities;
        public int droppedCommands;
        public int simulatedTicks;
        public float tickTimeMs;

        public void ResetFrame()
        {
            totalEffects = 0;
            scheduledEffects = 0;
            processedEffects = 0;
            appliedEntities = 0;
            appliedDamage = 0;
            dirtyEntities = 0;
            droppedCommands = 0;
            simulatedTicks = 0;
            tickTimeMs = 0f;
        }
    }

    public struct AddEffectCommand
    {
        public int targetChunk;
        public ushort targetIndex;
        public int damage;
        public int interval;
        public int duration;
    }

    public struct RemoveEffectCommand
    {
        public int targetChunk;
        public ushort targetIndex;
        public int maxRemoveCount;
    }

    public struct EffectCommandBuffer
    {
        public AddEffectCommand[] addEffects;
        public RemoveEffectCommand[] removeEffects;
        public int addCount;
        public int removeCount;
        public int droppedCommands;

        public int AddCapacity
        {
            get { return addEffects == null ? 0 : addEffects.Length; }
        }

        public int RemoveCapacity
        {
            get { return removeEffects == null ? 0 : removeEffects.Length; }
        }

        public void Initialize(int addCapacity, int removeCapacity)
        {
            addEffects = new AddEffectCommand[addCapacity];
            removeEffects = new RemoveEffectCommand[removeCapacity];
            addCount = 0;
            removeCount = 0;
            droppedCommands = 0;
        }

        public bool TryAdd(in AddEffectCommand command)
        {
            if (addCount >= AddCapacity)
            {
                droppedCommands++;
                return false;
            }

            addEffects[addCount] = command;
            addCount++;
            return true;
        }

        public bool TryRemove(in RemoveEffectCommand command)
        {
            if (removeCount >= RemoveCapacity)
            {
                droppedCommands++;
                return false;
            }

            removeEffects[removeCount] = command;
            removeCount++;
            return true;
        }

        public void Clear()
        {
            addCount = 0;
            removeCount = 0;
            droppedCommands = 0;
        }
    }

    public struct DamageAccumulator
    {
        public int[] Values;
        public int[] Dirty;
        public int[] DirtyMarks;
        public int DirtyCount;

        private int _mark;

        public void Initialize(int entityCapacity)
        {
            Values = new int[entityCapacity];
            Dirty = new int[entityCapacity];
            DirtyMarks = new int[entityCapacity];
            DirtyCount = 0;
            _mark = 1;
        }

        public void BeginFrame()
        {
            DirtyCount = 0;
            _mark++;

            if (_mark == int.MaxValue)
            {
                for (int i = 0; i < DirtyMarks.Length; i++)
                    DirtyMarks[i] = 0;

                _mark = 1;
            }
        }

        public bool Add(ushort target, int amount)
        {
            int index = target;
            if ((uint)index >= (uint)Values.Length)
                return false;

            if (DirtyMarks[index] != _mark)
            {
                if (DirtyCount >= Dirty.Length)
                    return false;

                DirtyMarks[index] = _mark;
                Dirty[DirtyCount] = index;
                DirtyCount++;
            }

            Values[index] += amount;
            return true;
        }

        public void ClearDirtyValues()
        {
            for (int i = 0; i < DirtyCount; i++)
                Values[Dirty[i]] = 0;

            DirtyCount = 0;
        }
    }

    public sealed class EffectScheduler
    {
        public int[] BucketHeads;
        public int Mask;

        public void Initialize(int wheelSize)
        {
            int size = NextPowerOfTwo(wheelSize);
            BucketHeads = new int[size];
            Mask = size - 1;

            for (int i = 0; i < BucketHeads.Length; i++)
                BucketHeads[i] = -1;
        }

        public int PopCurrentBucket(int currentTick)
        {
            int bucket = currentTick & Mask;
            int head = BucketHeads[bucket];
            BucketHeads[bucket] = -1;
            return head;
        }

        public void Schedule(int effectIndex, int dueTick, ref PeriodicDamageEffects effects)
        {
            if (effects.ScheduleBucket[effectIndex] >= 0)
                Unschedule(effectIndex, ref effects);

            int bucket = dueTick & Mask;
            int head = BucketHeads[bucket];

            effects.ScheduleBucket[effectIndex] = bucket;
            effects.SchedulePrev[effectIndex] = -1;
            effects.ScheduleNext[effectIndex] = head;

            if (head >= 0)
                effects.SchedulePrev[head] = effectIndex;

            BucketHeads[bucket] = effectIndex;
        }

        public void Unschedule(int effectIndex, ref PeriodicDamageEffects effects)
        {
            int bucket = effects.ScheduleBucket[effectIndex];
            if (bucket < 0)
                return;

            int prev = effects.SchedulePrev[effectIndex];
            int next = effects.ScheduleNext[effectIndex];

            if (prev >= 0)
                effects.ScheduleNext[prev] = next;
            else
                BucketHeads[bucket] = next;

            if (next >= 0)
                effects.SchedulePrev[next] = prev;

            effects.ScheduleBucket[effectIndex] = -1;
            effects.SchedulePrev[effectIndex] = -1;
            effects.ScheduleNext[effectIndex] = -1;
        }

        public void RelocateScheduledIndex(int from, int to, ref PeriodicDamageEffects effects)
        {
            int bucket = effects.ScheduleBucket[from];
            if (bucket < 0)
                return;

            int prev = effects.SchedulePrev[from];
            int next = effects.ScheduleNext[from];

            if (prev >= 0)
                effects.ScheduleNext[prev] = to;
            else
                BucketHeads[bucket] = to;

            if (next >= 0)
                effects.SchedulePrev[next] = to;
        }

        private static int NextPowerOfTwo(int value)
        {
            int result = 1;
            while (result < value)
                result <<= 1;

            return result;
        }
    }

    public struct PeriodicDamageEffects
    {
        public ushort[] Target;
        public int[] NextTick;
        public int[] Interval;
        public int[] Damage;
        public int[] ExpireTick;

        public int[] ScheduleNext;
        public int[] SchedulePrev;
        public int[] ScheduleBucket;
        public int[] RemoveFlag;

        public int Count;
        public int PendingRemoveCount;

        public int Capacity
        {
            get { return Target == null ? 0 : Target.Length; }
        }

        public void Initialize(int capacity)
        {
            Target = new ushort[capacity];
            NextTick = new int[capacity];
            Interval = new int[capacity];
            Damage = new int[capacity];
            ExpireTick = new int[capacity];
            ScheduleNext = new int[capacity];
            SchedulePrev = new int[capacity];
            ScheduleBucket = new int[capacity];
            RemoveFlag = new int[capacity];
            Count = 0;
            PendingRemoveCount = 0;

            for (int i = 0; i < capacity; i++)
            {
                ScheduleNext[i] = -1;
                SchedulePrev[i] = -1;
                ScheduleBucket[i] = -1;
            }
        }

        public bool TryAdd(
            ushort targetIndex,
            int currentTick,
            int damagePerTick,
            int intervalTicks,
            int durationTicks,
            EffectScheduler scheduler)
        {
            if (Count >= Capacity || durationTicks <= 0 || damagePerTick <= 0)
                return false;

            if (intervalTicks < 1)
                intervalTicks = 1;

            int index = Count;
            Target[index] = targetIndex;
            NextTick[index] = currentTick + intervalTicks;
            Interval[index] = intervalTicks;
            Damage[index] = damagePerTick;
            ExpireTick[index] = currentTick + durationTicks;
            RemoveFlag[index] = 0;
            ScheduleNext[index] = -1;
            SchedulePrev[index] = -1;
            ScheduleBucket[index] = -1;
            Count++;

            scheduler.Schedule(index, GetNextWakeTick(index), ref this);
            return true;
        }

        public int RemoveByTarget(ushort targetIndex, int maxRemoveCount, EffectScheduler scheduler)
        {
            int removed = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (Target[i] != targetIndex)
                    continue;

                RemoveAtSwapBack(i, scheduler);
                removed++;

                if (maxRemoveCount > 0 && removed >= maxRemoveCount)
                    break;
            }

            return removed;
        }

        public void MarkRemove(int index)
        {
            if ((uint)index >= (uint)Count || RemoveFlag[index] != 0)
                return;

            RemoveFlag[index] = 1;
            PendingRemoveCount++;
        }

        public void RemoveAtSwapBack(int index, EffectScheduler scheduler)
        {
            if ((uint)index >= (uint)Count)
                return;

            int last = Count - 1;
            scheduler.Unschedule(index, ref this);

            if (index != last)
            {
                scheduler.RelocateScheduledIndex(last, index, ref this);

                Target[index] = Target[last];
                NextTick[index] = NextTick[last];
                Interval[index] = Interval[last];
                Damage[index] = Damage[last];
                ExpireTick[index] = ExpireTick[last];
                ScheduleNext[index] = ScheduleNext[last];
                SchedulePrev[index] = SchedulePrev[last];
                ScheduleBucket[index] = ScheduleBucket[last];
                RemoveFlag[index] = RemoveFlag[last];
            }

            ScheduleNext[last] = -1;
            SchedulePrev[last] = -1;
            ScheduleBucket[last] = -1;
            RemoveFlag[last] = 0;
            Count = last;
        }

        public int GetNextWakeTick(int index)
        {
            int next = NextTick[index];
            int expire = ExpireTick[index];
            return next < expire ? next : expire;
        }
    }

    public sealed class Chunk
    {
        public readonly int chunkIndex;
        public readonly int entityCount;
        public readonly int globalEntityStart;

        public int[] Health;
        public int[] MaxHealth;

        public DamageAccumulator Damage;
        public PeriodicDamageEffects Effects;
        public EffectScheduler Scheduler;

        public int lastScheduledEffects;
        public int lastProcessedEffects;
        public int lastAppliedEntities;
        public int lastAppliedDamage;
        public int lastDirtyEntities;

        public Chunk(int chunkIndex, int globalEntityStart, int entityCount, int effectCapacity, int schedulerWheelSize)
        {
            this.chunkIndex = chunkIndex;
            this.globalEntityStart = globalEntityStart;
            this.entityCount = entityCount;

            Health = new int[entityCount];
            MaxHealth = new int[entityCount];
            Damage.Initialize(entityCount);
            Effects.Initialize(effectCapacity);
            Scheduler = new EffectScheduler();
            Scheduler.Initialize(schedulerWheelSize);
        }

        public void InitializeHealth(int value)
        {
            for (int i = 0; i < entityCount; i++)
            {
                Health[i] = value;
                MaxHealth[i] = value;
            }
        }

        public void BeginTick()
        {
            lastScheduledEffects = 0;
            lastProcessedEffects = 0;
            lastAppliedEntities = 0;
            lastAppliedDamage = 0;
            lastDirtyEntities = 0;
            Damage.BeginFrame();
        }
    }

    public abstract class ChunkEffectProcessor
    {
        public abstract void TickScheduledEffects(Chunk chunk, int currentTick, ref EffectMetrics metrics);
        public abstract void Cleanup(Chunk chunk, int currentTick);
    }

    public sealed class PeriodicDamageProcessor : ChunkEffectProcessor
    {
        public override void TickScheduledEffects(Chunk chunk, int currentTick, ref EffectMetrics metrics)
        {
            ref PeriodicDamageEffects effects = ref chunk.Effects;
            int effectIndex = chunk.Scheduler.PopCurrentBucket(currentTick);

            bool hasGroupedDamage = false;
            ushort groupedTarget = 0;
            int groupedDamage = 0;

            while (effectIndex >= 0)
            {
                int nextScheduled = effects.ScheduleNext[effectIndex];
                effects.ScheduleNext[effectIndex] = -1;
                effects.SchedulePrev[effectIndex] = -1;
                effects.ScheduleBucket[effectIndex] = -1;

                metrics.scheduledEffects++;
                chunk.lastScheduledEffects++;

                if ((uint)effectIndex >= (uint)effects.Count)
                {
                    effectIndex = nextScheduled;
                    continue;
                }

                if (currentTick >= effects.ExpireTick[effectIndex])
                {
                    FlushGroupedDamage(chunk, ref hasGroupedDamage, ref groupedTarget, ref groupedDamage);
                    effects.MarkRemove(effectIndex);
                    effectIndex = nextScheduled;
                    continue;
                }

                int nextTick = effects.NextTick[effectIndex];
                if (currentTick < nextTick)
                {
                    chunk.Scheduler.Schedule(effectIndex, effects.GetNextWakeTick(effectIndex), ref effects);
                    effectIndex = nextScheduled;
                    continue;
                }

                int interval = effects.Interval[effectIndex];
                int fireCount = 1 + ((currentTick - nextTick) / interval);
                int lastValidTick = effects.ExpireTick[effectIndex] - 1;

                if (nextTick + ((fireCount - 1) * interval) > lastValidTick)
                    fireCount = 1 + ((lastValidTick - nextTick) / interval);

                if (fireCount > 0)
                {
                    ushort target = effects.Target[effectIndex];
                    int amount = effects.Damage[effectIndex] * fireCount;

                    if (hasGroupedDamage && target == groupedTarget)
                    {
                        groupedDamage += amount;
                    }
                    else
                    {
                        FlushGroupedDamage(chunk, ref hasGroupedDamage, ref groupedTarget, ref groupedDamage);
                        hasGroupedDamage = true;
                        groupedTarget = target;
                        groupedDamage = amount;
                    }

                    metrics.processedEffects++;
                    chunk.lastProcessedEffects++;
                    effects.NextTick[effectIndex] = nextTick + (fireCount * interval);
                }

                chunk.Scheduler.Schedule(effectIndex, effects.GetNextWakeTick(effectIndex), ref effects);
                effectIndex = nextScheduled;
            }

            FlushGroupedDamage(chunk, ref hasGroupedDamage, ref groupedTarget, ref groupedDamage);
        }

        public override void Cleanup(Chunk chunk, int currentTick)
        {
            ref PeriodicDamageEffects effects = ref chunk.Effects;
            if (effects.PendingRemoveCount == 0)
                return;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (effects.RemoveFlag[i] != 0)
                    effects.RemoveAtSwapBack(i, chunk.Scheduler);
            }

            effects.PendingRemoveCount = 0;
        }

        private static void FlushGroupedDamage(
            Chunk chunk,
            ref bool hasGroupedDamage,
            ref ushort groupedTarget,
            ref int groupedDamage)
        {
            if (!hasGroupedDamage)
                return;

            chunk.Damage.Add(groupedTarget, groupedDamage);
            hasGroupedDamage = false;
            groupedDamage = 0;
        }
    }

    public sealed class EffectManager
    {
        public const int DefaultChunkSize = 256;
        public const int DefaultSchedulerWheelSize = 256;

        private readonly Chunk[] _chunks;
        private readonly ChunkEffectProcessor[] _processors;
        private EffectCommandBuffer _commands;
        private EffectMetrics _metrics;
        private double _timeAccumulator;
        private readonly double _fixedDelta;
        private readonly int _chunkSize;
        private readonly int _maxTicksPerFrame;

        public int CurrentTick { get; private set; }
        public int EntityCount { get; private set; }
        public int ChunkCount { get { return _chunks.Length; } }
        public EffectMetrics Metrics { get { return _metrics; } }
        public Chunk[] Chunks { get { return _chunks; } }

        public EffectManager(
            int entityCount,
            int effectCapacityPerChunk,
            int commandCapacity,
            int tickRate = 60,
            int chunkSize = DefaultChunkSize,
            int maxTicksPerFrame = 4,
            int schedulerWheelSize = DefaultSchedulerWheelSize)
        {
            if (entityCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(entityCount));
            if (effectCapacityPerChunk <= 0)
                throw new ArgumentOutOfRangeException(nameof(effectCapacityPerChunk));
            if (commandCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(commandCapacity));
            if (tickRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickRate));
            if (chunkSize <= 0 || chunkSize > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            if (schedulerWheelSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(schedulerWheelSize));

            EntityCount = entityCount;
            _chunkSize = chunkSize;
            _fixedDelta = 1.0 / tickRate;
            _maxTicksPerFrame = maxTicksPerFrame < 1 ? 1 : maxTicksPerFrame;

            int chunkCount = (entityCount + chunkSize - 1) / chunkSize;
            _chunks = new Chunk[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                int start = i * chunkSize;
                int count = entityCount - start;
                if (count > chunkSize)
                    count = chunkSize;

                _chunks[i] = new Chunk(i, start, count, effectCapacityPerChunk, schedulerWheelSize);
            }

            _processors = new ChunkEffectProcessor[]
            {
                new PeriodicDamageProcessor()
            };

            _commands.Initialize(commandCapacity, commandCapacity);
            CurrentTick = 0;
        }

        public void InitializeHealth(int value)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i].InitializeHealth(value);
        }

        public bool TryQueueAddPeriodicDamage(int globalTargetEntity, int damage, int intervalTicks, int durationTicks)
        {
            if (!TryGetChunkLocal(globalTargetEntity, out int chunkIndex, out ushort localIndex))
                return false;

            AddEffectCommand command;
            command.targetChunk = chunkIndex;
            command.targetIndex = localIndex;
            command.damage = damage;
            command.interval = intervalTicks;
            command.duration = durationTicks;
            return _commands.TryAdd(command);
        }

        public bool TryQueueRemovePeriodicDamage(int globalTargetEntity, int maxRemoveCount)
        {
            if (!TryGetChunkLocal(globalTargetEntity, out int chunkIndex, out ushort localIndex))
                return false;

            RemoveEffectCommand command;
            command.targetChunk = chunkIndex;
            command.targetIndex = localIndex;
            command.maxRemoveCount = maxRemoveCount;
            return _commands.TryRemove(command);
        }

        public void FlushPendingCommands()
        {
            RouteCommands();
        }

        public void Tick(float deltaTime)
        {
            long start = Stopwatch.GetTimestamp();
            _metrics.ResetFrame();

            if (deltaTime < 0f)
                deltaTime = 0f;

            _timeAccumulator += deltaTime;

            int ticks = 0;
            while (_timeAccumulator >= _fixedDelta && ticks < _maxTicksPerFrame)
            {
                _timeAccumulator -= _fixedDelta;
                CurrentTick++;
                SimulateOneTick();
                ticks++;
            }

            if (ticks == _maxTicksPerFrame && _timeAccumulator >= _fixedDelta)
                _timeAccumulator = 0.0;

            _metrics.simulatedTicks = ticks;
            _metrics.droppedCommands += _commands.droppedCommands;
            _metrics.totalEffects = CountEffects();
            _metrics.tickTimeMs = ElapsedMilliseconds(start, Stopwatch.GetTimestamp());
        }

        public int GetHealth(int globalEntity)
        {
            if (!TryGetChunkLocal(globalEntity, out int chunkIndex, out ushort localIndex))
                return 0;

            return _chunks[chunkIndex].Health[localIndex];
        }

        public int GetMaxHealth(int globalEntity)
        {
            if (!TryGetChunkLocal(globalEntity, out int chunkIndex, out ushort localIndex))
                return 0;

            return _chunks[chunkIndex].MaxHealth[localIndex];
        }

        private void SimulateOneTick()
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i].BeginTick();

            for (int p = 0; p < _processors.Length; p++)
            {
                ChunkEffectProcessor processor = _processors[p];
                for (int c = 0; c < _chunks.Length; c++)
                    processor.TickScheduledEffects(_chunks[c], CurrentTick, ref _metrics);
            }

            for (int c = 0; c < _chunks.Length; c++)
                ApplyDamage(_chunks[c]);

            for (int p = 0; p < _processors.Length; p++)
            {
                ChunkEffectProcessor processor = _processors[p];
                for (int c = 0; c < _chunks.Length; c++)
                    processor.Cleanup(_chunks[c], CurrentTick);
            }

            RouteCommands();
        }

        private void RouteCommands()
        {
            for (int i = 0; i < _commands.addCount; i++)
            {
                AddEffectCommand command = _commands.addEffects[i];
                if (!IsValidChunkTarget(command.targetChunk, command.targetIndex))
                {
                    _commands.droppedCommands++;
                    continue;
                }

                Chunk chunk = _chunks[command.targetChunk];
                if (!chunk.Effects.TryAdd(command.targetIndex, CurrentTick, command.damage, command.interval, command.duration, chunk.Scheduler))
                    _commands.droppedCommands++;
            }

            for (int i = 0; i < _commands.removeCount; i++)
            {
                RemoveEffectCommand command = _commands.removeEffects[i];
                if (!IsValidChunkTarget(command.targetChunk, command.targetIndex))
                {
                    _commands.droppedCommands++;
                    continue;
                }

                Chunk chunk = _chunks[command.targetChunk];
                chunk.Effects.RemoveByTarget(command.targetIndex, command.maxRemoveCount, chunk.Scheduler);
            }

            _commands.Clear();
        }

        private void ApplyDamage(Chunk chunk)
        {
            ref DamageAccumulator accumulator = ref chunk.Damage;
            int dirtyCount = accumulator.DirtyCount;
            _metrics.dirtyEntities += dirtyCount;
            chunk.lastDirtyEntities = dirtyCount;

            for (int i = 0; i < dirtyCount; i++)
            {
                int target = accumulator.Dirty[i];
                int amount = accumulator.Values[target];
                if (amount <= 0)
                    continue;

                int nextHealth = chunk.Health[target] - amount;
                if (nextHealth < 0)
                    nextHealth = 0;

                chunk.Health[target] = nextHealth;
                _metrics.appliedDamage += amount;
                _metrics.appliedEntities++;
                chunk.lastAppliedDamage += amount;
                chunk.lastAppliedEntities++;
            }

            accumulator.ClearDirtyValues();
        }

        private bool TryGetChunkLocal(int globalEntity, out int chunkIndex, out ushort localIndex)
        {
            chunkIndex = -1;
            localIndex = 0;

            if ((uint)globalEntity >= (uint)EntityCount)
                return false;

            chunkIndex = globalEntity / _chunkSize;
            localIndex = (ushort)(globalEntity - (chunkIndex * _chunkSize));
            return true;
        }

        private bool IsValidChunkTarget(int chunkIndex, ushort localIndex)
        {
            return (uint)chunkIndex < (uint)_chunks.Length && localIndex < _chunks[chunkIndex].entityCount;
        }

        private int CountEffects()
        {
            int total = 0;
            for (int i = 0; i < _chunks.Length; i++)
                total += _chunks[i].Effects.Count;

            return total;
        }

        private static float ElapsedMilliseconds(long start, long end)
        {
            return (float)((end - start) * 1000.0 / Stopwatch.Frequency);
        }
    }
}
