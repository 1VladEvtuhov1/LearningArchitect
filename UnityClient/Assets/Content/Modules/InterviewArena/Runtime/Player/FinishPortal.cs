using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class FinishPortal : MonoBehaviour
    {
        private bool triggered;

        private void OnTriggerEnter(Collider other)
        {
            if (triggered)
                return;

            PlayerMotor motor = other.GetComponentInParent<PlayerMotor>();
            if (motor == null)
                return;

            triggered = true;
            InterviewArenaRunSession.NotifyRunCompleted();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.5f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}
