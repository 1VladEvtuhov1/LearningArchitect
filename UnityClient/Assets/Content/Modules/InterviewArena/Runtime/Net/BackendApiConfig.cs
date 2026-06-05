using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        fileName = "InterviewArena_BackendApiConfig",
        menuName = "Learning Architect/Interview Arena/Backend API Config")]
    public sealed class BackendApiConfig : ScriptableObject
    {
        [SerializeField] private bool useOnlineFlow = true;
        [SerializeField] private string baseUrl = "http://localhost:5000";
        [SerializeField] private float timeoutSeconds = 15f;

        public bool UseOnlineFlow => useOnlineFlow;
        public string BaseUrl => baseUrl;
        public float TimeoutSeconds => timeoutSeconds;
    }
}
