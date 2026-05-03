using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Inventory
{
    public sealed class InventoryObjectsVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const int MaxStack = 12;
        private const int TypeCount = 5;

        private sealed class InventorySlotState
        {
            public int ItemType;
            public int Amount;
            public float RestockCooldown;
        }

        [SerializeField] private int slotCount = 1024;
        [SerializeField] private int visibleCount = 180;
        [SerializeField] private int[] visibleCountStressPresets = { 256, 1024, 2048 };
        [SerializeField] private int[] visibleCountPresets = { 256, 220, 280 };
        [SerializeField] private int visualLimit = 280;
        [SerializeField] private float slotSpacing = 0.48f;
        [SerializeField] private float visualRefreshInterval = 0.08f;

        private MaterialPropertyBlock propertyBlock;

        private InventorySlotState[] slots;
        private Transform[] visuals;
        private Renderer[] renderers;
        private int operationsCursor;
        private float nextVisualRefreshTime;
        private float moduleCpuMs;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            Rebuild(slotCount);
        }

        private void Update()
        {
            if (slots == null)
                return;

            long startedAt = Stopwatch.GetTimestamp();
            SimulateInventory(Time.deltaTime);
            moduleCpuMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
            RefreshVisuals(false);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(slotCount, ActiveCount, moduleCpuMs);
        }

        public void SetStressLevel(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (slotCount == count && slots != null && slots.Length == count)
                return;

            slotCount = count;
            Rebuild(slotCount);
        }

        private void Rebuild(int targetSlotCount)
        {
            ClearVisuals();

            slots = new InventorySlotState[targetSlotCount];
            for (int i = 0; i < targetSlotCount; i++)
            {
                slots[i] = new InventorySlotState
                {
                    ItemType = i % TypeCount,
                    Amount = 1 + (i % MaxStack),
                    RestockCooldown = 0.12f * (i % 5)
                };
            }

            int visibleSlots = Mathf.Min(targetSlotCount, Mathf.Min(ResolveVisibleCount(targetSlotCount), visualLimit));
            visuals = new Transform[visibleSlots];
            renderers = new Renderer[visibleSlots];
            for (int i = 0; i < visibleSlots; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(marker);
                marker.name = "Inventory Slot Object " + i;
                marker.transform.SetParent(transform, false);
                visuals[i] = marker.transform;
                renderers[i] = marker.GetComponent<Renderer>();
            }

            operationsCursor = 0;
            nextVisualRefreshTime = 0f;
            RefreshVisuals(true);
        }

        private int ResolveVisibleCount(int targetSlotCount)
        {
            if (visibleCountStressPresets == null || visibleCountPresets == null)
                return visibleCount;

            int pairCount = Mathf.Min(visibleCountStressPresets.Length, visibleCountPresets.Length);
            if (pairCount == 0)
                return visibleCount;

            for (int i = 0; i < pairCount; i++)
            {
                if (visibleCountStressPresets[i] == targetSlotCount)
                    return Mathf.Max(1, visibleCountPresets[i]);
            }

            int bestIndex = 0;
            int smallestDistance = Mathf.Abs(visibleCountStressPresets[0] - targetSlotCount);
            for (int i = 1; i < pairCount; i++)
            {
                int distance = Mathf.Abs(visibleCountStressPresets[i] - targetSlotCount);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    bestIndex = i;
                }
            }

            return Mathf.Max(1, visibleCountPresets[bestIndex]);
        }

        private void SimulateInventory(float deltaTime)
        {
            int operations = Mathf.Max(18, slotCount / 30);
            for (int i = 0; i < slotCount; i++)
            {
                InventorySlotState slot = slots[i];
                if (slot.Amount == 0)
                {
                    slot.RestockCooldown -= deltaTime;
                    if (slot.RestockCooldown <= 0f)
                    {
                        slot.ItemType = (slot.ItemType + 1 + (i % 3)) % TypeCount;
                        slot.Amount = 1 + ((operationsCursor + i) % MaxStack);
                        slot.RestockCooldown = 0.45f + ((i % 7) * 0.03f);
                    }
                }
            }

            for (int i = 0; i < operations; i++)
            {
                int sourceIndex = (operationsCursor + (i * 7)) % slotCount;
                int targetIndex = (operationsCursor + 3 + (i * 13)) % slotCount;
                if (sourceIndex == targetIndex)
                    continue;

                TransferObjectSlot(slots[sourceIndex], slots[targetIndex], sourceIndex, targetIndex);
            }

            operationsCursor = (operationsCursor + operations) % slotCount;
        }

        private void TransferObjectSlot(InventorySlotState source, InventorySlotState target, int sourceIndex, int targetIndex)
        {
            if (source.Amount <= 0)
                return;

            if (target.Amount == 0)
            {
                int moveAmount = Mathf.Min(2 + (sourceIndex % 2), source.Amount);
                target.ItemType = source.ItemType;
                target.Amount = moveAmount;
                source.Amount -= moveAmount;
                if (source.Amount == 0)
                    source.RestockCooldown = 0.24f + ((sourceIndex + targetIndex) % 5) * 0.05f;
                return;
            }

            if (target.ItemType == source.ItemType && target.Amount < MaxStack)
            {
                int moveAmount = Mathf.Min(1 + ((sourceIndex + targetIndex) & 1), Mathf.Min(source.Amount, MaxStack - target.Amount));
                target.Amount += moveAmount;
                source.Amount -= moveAmount;
                if (source.Amount == 0)
                    source.RestockCooldown = 0.18f + ((sourceIndex + targetIndex) % 6) * 0.04f;
                return;
            }

            if (((sourceIndex + targetIndex + operationsCursor) & 7) == 0)
            {
                int oldType = source.ItemType;
                int oldAmount = source.Amount;
                source.ItemType = target.ItemType;
                source.Amount = target.Amount;
                target.ItemType = oldType;
                target.Amount = oldAmount;
            }
        }

        private void RefreshVisuals(bool force)
        {
            if (!force && Time.unscaledTime < nextVisualRefreshTime)
                return;

            nextVisualRefreshTime = Time.unscaledTime + visualRefreshInterval;
            if (visuals == null)
                return;

            int columns = Mathf.Max(6, Mathf.CeilToInt(Mathf.Sqrt(visuals.Length)));
            float originOffset = (columns - 1) * slotSpacing * 0.5f;

            for (int i = 0; i < visuals.Length; i++)
            {
                Transform visual = visuals[i];
                InventorySlotState slot = slots[i];

                int row = i / columns;
                int column = i % columns;
                float height = Mathf.Lerp(0.08f, 0.42f, slot.Amount / (float)MaxStack);
                visual.localPosition = new Vector3((column * slotSpacing) - originOffset, height * 0.5f, (row * slotSpacing) - originOffset);
                visual.localScale = new Vector3(0.26f, height, 0.26f);

                Color color = slot.Amount <= 0 ? new Color(0.16f, 0.18f, 0.2f) : GetTypeColor(slot.ItemType);
                propertyBlock.Clear();
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                renderers[i].SetPropertyBlock(propertyBlock);
            }
        }

        private void ClearVisuals()
        {
            if (visuals == null)
                return;

            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] != null)
                    DestroyVisual(visuals[i].gameObject);
            }
        }

        private static Color GetTypeColor(int typeIndex)
        {
            return typeIndex switch
            {
                0 => new Color(0.98f, 0.66f, 0.25f),
                1 => new Color(0.32f, 0.82f, 0.52f),
                2 => new Color(0.27f, 0.67f, 0.98f),
                3 => new Color(0.82f, 0.42f, 0.95f),
                _ => new Color(0.96f, 0.3f, 0.44f)
            };
        }

        private static void DestroyVisual(GameObject visual)
        {
#if UNITY_EDITOR
            DestroyImmediate(visual);
#else
            Destroy(visual);
#endif
        }
    }
}
