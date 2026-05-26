using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class InterviewArenaCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 5.5f, -9f);
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float lookAhead = 0.35f;

        private Vector3 positionVelocity;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 focus = target.position + target.forward * lookAhead;
            Vector3 desiredPosition = focus + offset;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime);

            Quaternion desiredRotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime / Mathf.Max(rotationSmoothTime, 0.0001f));
        }
    }
}
