using System;
using System.Collections;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseTransitionController : MonoBehaviour
    {
        public CanvasGroup transitionOverlay;
        public float transitionDuration = 0.18f;

        private Coroutine transitionCoroutine;

        private void Awake()
        {
            InitializeOverlay();
        }

        public bool CanAnimate
        {
            get { return transitionOverlay != null && transitionDuration > 0f; }
        }

        public void InitializeOverlay()
        {
            if (transitionOverlay == null)
                transitionOverlay = FindOverlay();

            if (transitionOverlay == null)
                return;

            transitionOverlay.alpha = 0f;
            transitionOverlay.blocksRaycasts = false;
            transitionOverlay.interactable = false;
        }

        public void Play(Action midpointAction)
        {
            if (!CanAnimate || !isActiveAndEnabled)
            {
                midpointAction?.Invoke();
                return;
            }

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(PlayTransition(midpointAction));
        }

        private IEnumerator PlayTransition(Action midpointAction)
        {
            yield return Fade(transitionOverlay.alpha, 1f);
            midpointAction?.Invoke();
            yield return Fade(1f, 0f);
            transitionCoroutine = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            float duration = Mathf.Max(0.01f, transitionDuration);
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);
                t = t * t * (3f - 2f * t);
                transitionOverlay.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            transitionOverlay.alpha = to;
        }

        private CanvasGroup FindOverlay()
        {
            CanvasGroup[] groups = GetComponentsInChildren<CanvasGroup>(true);
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] != null && groups[i].name == "TransitionOverlay")
                    return groups[i];
            }

            return null;
        }
    }
}
