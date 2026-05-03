using System;
namespace LearningArchitect.Core
{
    public sealed class ModuleRuntimeHost
    {
        private IModule activeModule;
        private IShowcaseStressTarget activeStressTarget;
        private IShowcaseMetricsSource activeMetricsSource;

        public int ActiveItemCount
        {
            get
            {
                if (!IsAlive(activeStressTarget))
                    return 0;

                return activeStressTarget.ActiveCount;
            }
        }

        public void Activate(ActiveVariantHandle handle)
        {
            activeModule = handle.Module;
            activeStressTarget = handle.StressTarget;
            activeMetricsSource = handle.MetricsSource;
            ValidateHandle();

            activeModule.Enter();
        }

        public void Deactivate()
        {
            if (!IsAlive(activeModule))
            {
                ClearActiveReferences();
                return;
            }

            activeModule.Exit();

            ClearActiveReferences();
        }

        public void SetStressLevel(int level)
        {
            if (!IsAlive(activeStressTarget))
                return;

            activeStressTarget.SetStressLevel(level);
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot(int stressLevel)
        {
            ShowcaseMetricsSnapshot snapshot = IsAlive(activeMetricsSource)
                ? activeMetricsSource.GetMetricsSnapshot()
                : ShowcaseMetricsSnapshot.Empty;

            if (!snapshot.HasSimulationCount)
                snapshot = snapshot.WithSimulationCount(stressLevel);

            if (!snapshot.HasVisibleCount)
                snapshot = snapshot.WithVisibleCount(ActiveItemCount);

            return snapshot;
        }

        private void ClearActiveReferences()
        {
            activeModule = null;
            activeStressTarget = null;
            activeMetricsSource = null;
        }

        private void ValidateHandle()
        {
            if (!IsAlive(activeModule))
                throw new InvalidOperationException($"{nameof(ActiveVariantHandle)} requires {nameof(IModule)}.");

            if (!IsAlive(activeStressTarget))
                throw new InvalidOperationException($"{nameof(ActiveVariantHandle)} requires {nameof(IShowcaseStressTarget)}.");
        }

        private static bool IsAlive(object target)
        {
            if (target == null)
                return false;

            return target is not UnityEngine.Object unityObject || unityObject != null;
        }
    }
}
