using System;
using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Inventory
{
    public sealed class InventoryPackedVariant : MonoBehaviour, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private const int MaxStack = 16;
        private const int TypeCount = 5;

        [SerializeField] private int slotCount = 4096;
        [SerializeField] private int visibleCount = 220;
        [SerializeField] private int[] visibleCountStressPresets = { 1024, 4096, 8192 };
        [SerializeField] private int[] visibleCountPresets = { 180, 240, 320 };
        [SerializeField] private int visualLimit = 320;
        [SerializeField] private float slotSpacing = 0.46f;
        [SerializeField] private float visualRefreshInterval = 0.06f;

        private MaterialPropertyBlock propertyBlock;

        private int[] itemTypes;
        private int[] amounts;
        private float[] restockCooldowns;
        private Transform[] visuals;
        private Renderer[] renderers;
        private int operationsCursor;
        private float nextVisualRefreshTime;
        private int operationsPerFrame;
        private float simulationTimeMs;

        public int ActiveCount => visuals == null ? 0 : visuals.Length;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            Rebuild(slotCount);
        }

        private void Update()
        {
            if (amounts == null)
                return;

            long startedAt = Stopwatch.GetTimestamp();
            SimulateInventory(Time.deltaTime);
            simulationTimeMs = (float)((Stopwatch.GetTimestamp() - startedAt) * 1000d / Stopwatch.Frequency);
            RefreshVisuals(false);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(slotCount, ActiveCount, operationsPerFrame, simulationTimeMs);
        }

        public void SetStressLevel(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (slotCount == count && amounts != null && amounts.Length == count)
                return;

            slotCount = count;
            Rebuild(slotCount);
        }

        private void Rebuild(int targetSlotCount)
        {
            ClearVisuals();

            itemTypes = new int[targetSlotCount];
            amounts = new int[targetSlotCount];
            restockCooldowns = new float[targetSlotCount];

            for (int i = 0; i < targetSlotCount; i++)
            {
                itemTypes[i] = i % TypeCount;
                amounts[i] = 2 + (i % (MaxStack - 1));
                restockCooldowns[i] = 0.08f * (i % 6);
            }

            int visibleSlots = Mathf.Min(targetSlotCount, Mathf.Min(ResolveVisibleCount(targetSlotCount), visualLimit));
            visuals = new Transform[visibleSlots];
            renderers = new Renderer[visibleSlots];
            for (int i = 0; i < visibleSlots; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LearningArchitect.Core.ShowcasePrimitiveMaterialUtility.Apply(marker);
                marker.name = "Inventory Slot Packed " + i;
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
            int operations = Mathf.Max(24, slotCount / 18);
            operationsPerFrame = operations;
            for (int i = 0; i < slotCount; i++)
            {
                if (amounts[i] > 0)
                    continue;

                restockCooldowns[i] -= deltaTime;
                if (restockCooldowns[i] > 0f)
                    continue;

                itemTypes[i] = (itemTypes[i] + 2 + (i % 3)) % TypeCount;
                amounts[i] = 2 + ((operationsCursor + i) % (MaxStack - 1));
                restockCooldowns[i] = 0.26f + ((i % 8) * 0.02f);
            }

            for (int i = 0; i < operations; i++)
            {
                int sourceIndex = (operationsCursor + (i * 11)) % slotCount;
                int targetIndex = (operationsCursor + 5 + (i * 17)) % slotCount;
                if (sourceIndex == targetIndex || amounts[sourceIndex] <= 0)
                    continue;

                TransferPackedSlot(sourceIndex, targetIndex);
            }

            operationsCursor = (operationsCursor + operations) % slotCount;
        }

        private void TransferPackedSlot(int sourceIndex, int targetIndex)
        {
            if (amounts[targetIndex] == 0)
            {
                int moveAmount = Mathf.Min(3, amounts[sourceIndex]);
                itemTypes[targetIndex] = itemTypes[sourceIndex];
                amounts[targetIndex] = moveAmount;
                amounts[sourceIndex] -= moveAmount;
                if (amounts[sourceIndex] == 0)
                    restockCooldowns[sourceIndex] = 0.12f + ((sourceIndex + targetIndex) % 7) * 0.03f;
                return;
            }

            if (itemTypes[targetIndex] == itemTypes[sourceIndex] && amounts[targetIndex] < MaxStack)
            {
                int moveAmount = Mathf.Min(2, Mathf.Min(amounts[sourceIndex], MaxStack - amounts[targetIndex]));
                amounts[targetIndex] += moveAmount;
                amounts[sourceIndex] -= moveAmount;
                if (amounts[sourceIndex] == 0)
                    restockCooldowns[sourceIndex] = 0.14f + ((sourceIndex + targetIndex) % 5) * 0.04f;
                return;
            }

            if (((sourceIndex ^ targetIndex ^ operationsCursor) & 15) == 0)
            {
                int oldType = itemTypes[sourceIndex];
                int oldAmount = amounts[sourceIndex];
                itemTypes[sourceIndex] = itemTypes[targetIndex];
                amounts[sourceIndex] = amounts[targetIndex];
                itemTypes[targetIndex] = oldType;
                amounts[targetIndex] = oldAmount;
            }
        }

        private void RefreshVisuals(bool force)
        {
            if (!force && Time.unscaledTime < nextVisualRefreshTime)
                return;

            nextVisualRefreshTime = Time.unscaledTime + visualRefreshInterval;
            if (visuals == null)
                return;

            int columns = Mathf.Max(8, Mathf.CeilToInt(Mathf.Sqrt(visuals.Length)));
            float originOffset = (columns - 1) * slotSpacing * 0.5f;

            for (int i = 0; i < visuals.Length; i++)
            {
                int slotIndex = (i + operationsCursor) % slotCount;
                int row = i / columns;
                int column = i % columns;
                float normalized = amounts[slotIndex] / (float)MaxStack;
                float height = Mathf.Lerp(0.06f, 0.46f, normalized);

                Transform visual = visuals[i];
                visual.localPosition = new Vector3((column * slotSpacing) - originOffset, height * 0.5f, (row * slotSpacing) - originOffset);
                visual.localScale = new Vector3(0.24f, height, 0.24f);

                Color color = amounts[slotIndex] <= 0
                    ? new Color(0.14f, 0.16f, 0.18f)
                    : Color.Lerp(GetTypeColor(itemTypes[slotIndex]), Color.white, 0.15f + (normalized * 0.1f));

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
                0 => new Color(0.98f, 0.7f, 0.26f),
                1 => new Color(0.3f, 0.88f, 0.58f),
                2 => new Color(0.25f, 0.72f, 0.98f),
                3 => new Color(0.76f, 0.46f, 0.94f),
                _ => new Color(0.98f, 0.34f, 0.42f)
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
