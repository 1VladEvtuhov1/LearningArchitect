using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Thin scene entry: delegates to <see cref="InterviewArenaRuntimeContext"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InterviewArenaRuntimeContext))]
    public sealed class InterviewArenaBootstrap : MonoBehaviour
    {
        [SerializeField] private InterviewArenaRuntimeContext context;

        private void Awake()
        {
            context = context != null ? context : GetComponent<InterviewArenaRuntimeContext>();
            Result wireResult = context.TryWirePlayerAndCamera();
            if (!wireResult.Succeeded)
                Debug.LogError("[InterviewArena] " + wireResult.Error, context);
        }
    }
}
