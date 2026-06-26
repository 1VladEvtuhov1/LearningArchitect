using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    [DisallowMultipleComponent]
    public sealed class HumanoidCrowdActor : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform visualRoot;

        private Vector3 smoothedForward = Vector3.forward;
        private bool initialized;

        public Transform ActorRoot => transform;
        public Animator Animator => animator;

        private void Awake()
        {
            ResolveReferences();
        }

        public void Initialize(HumanoidAnimationProfileSO profile, float normalizedTime)
        {
            ResolveReferences();
            ApplyProfile(profile);

            if (animator != null && profile != null && profile.RandomizeStartTime && animator.runtimeAnimatorController != null)
            {
                animator.Rebind();
                animator.Update(0f);
                animator.Play(0, 0, Mathf.Repeat(normalizedTime, 1f));
                animator.Update(0f);
            }

            smoothedForward = transform.forward.sqrMagnitude > 0.0001f ? transform.forward : Vector3.forward;
            initialized = true;
        }

        public void ApplyFrame(HumanoidAnimationProfileSO profile, Vector3 localPosition, Vector3 velocity, float deltaTime, bool isShooting = false)
        {
            if (!initialized)
                Initialize(profile, 0f);

            transform.localPosition = localPosition;

            Vector3 flatVelocity = velocity;
            flatVelocity.y = 0f;

            bool usesDirectionalLocomotion = profile != null &&
                !string.IsNullOrWhiteSpace(profile.MoveXParameter);

            if (!usesDirectionalLocomotion)
            {
                Vector3 desiredForward = flatVelocity.sqrMagnitude > 0.0001f
                    ? flatVelocity.normalized
                    : smoothedForward;

                float turnAmount = 0f;
                if (smoothedForward.sqrMagnitude > 0.0001f && desiredForward.sqrMagnitude > 0.0001f)
                    turnAmount = Mathf.Clamp(Vector3.SignedAngle(smoothedForward, desiredForward, Vector3.up) / 90f, -1f, 1f);

                float turnResponsiveness = profile == null ? 8f : profile.TurnResponsiveness;
                float smoothing = 1f - Mathf.Exp(-turnResponsiveness * Mathf.Max(0f, deltaTime));
                smoothedForward = Vector3.Slerp(smoothedForward, desiredForward, smoothing);

                if (smoothedForward.sqrMagnitude > 0.0001f)
                    transform.localRotation = Quaternion.LookRotation(smoothedForward, Vector3.up);

                ApplyAnimatorParameters(profile, flatVelocity.magnitude, turnAmount, deltaTime, isShooting);
                return;
            }

            HumanoidLocomotionAnimation.ResolveLocalMove(
                flatVelocity,
                transform.forward,
                profile.SpeedNormalization,
                out float moveX,
                out float moveY,
                out float speed01);

            float turn = 0f;
            if (flatVelocity.sqrMagnitude > 0.01f)
            {
                Vector3 forward = transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.0001f)
                {
                    turn = Mathf.Clamp(
                        Vector3.SignedAngle(forward.normalized, flatVelocity.normalized, Vector3.up) / 90f,
                        -1f,
                        1f);
                }
            }

            ApplyAnimatorParameters(profile, speed01, moveX, moveY, turn, deltaTime, isShooting);
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
                visualRoot = transform;

            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }

        private void ApplyProfile(HumanoidAnimationProfileSO profile)
        {
            if (profile == null || animator == null)
                return;

            if (profile.AnimatorController != null)
                animator.runtimeAnimatorController = profile.AnimatorController;

            if (profile.Avatar != null)
                animator.avatar = profile.Avatar;

            animator.applyRootMotion = profile.ApplyRootMotion;
            animator.updateMode = profile.UpdateMode;
            animator.cullingMode = profile.CullingMode;
        }

        private void ApplyAnimatorParameters(
            HumanoidAnimationProfileSO profile,
            float speed01,
            float moveX,
            float moveY,
            float turnAmount,
            float deltaTime,
            bool isShooting)
        {
            if (animator == null || profile == null)
                return;

            float dampTime = profile.ParameterDampTime;
            bool isMoving = speed01 >= profile.MovingThreshold;

            SetFloat(profile.SpeedParameter, speed01, dampTime, deltaTime);
            SetFloat(profile.MoveXParameter, moveX, dampTime, deltaTime);
            SetFloat(profile.MoveYParameter, moveY, dampTime, deltaTime);
            SetFloat(profile.TurnParameter, turnAmount, dampTime, deltaTime);
            SetBool(profile.MovingParameter, isMoving);
            SetBool(profile.GroundedParameter, true);
            SetBool(profile.ShootingParameter, isShooting);
        }

        private void ApplyAnimatorParameters(
            HumanoidAnimationProfileSO profile,
            float speed,
            float turnAmount,
            float deltaTime,
            bool isShooting)
        {
            float speed01 = Mathf.Clamp01(speed / profile.SpeedNormalization);
            ApplyAnimatorParameters(profile, speed01, 0f, speed01, turnAmount, deltaTime, isShooting);
        }

        private void SetFloat(string parameterName, float value, float dampTime, float deltaTime)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                return;

            if (dampTime > 0f)
                animator.SetFloat(parameterName, value, dampTime, deltaTime);
            else
                animator.SetFloat(parameterName, value);
        }

        private void SetBool(string parameterName, bool value)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                return;

            animator.SetBool(parameterName, value);
        }
    }
}
