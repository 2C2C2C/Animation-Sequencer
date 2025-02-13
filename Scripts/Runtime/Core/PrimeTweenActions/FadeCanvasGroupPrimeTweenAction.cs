#if PRIMETWEEN_ENABLED
using System;
using UnityEngine;
using PrimeTween;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class FadeCanvasGroupPrimeTweenAction : PrimeTweenActionBase
    {
        [SerializeField]
        private float alpha;
        [SerializeField]
        private float defaultStartValue = 1f;

        private CanvasGroup canvasGroup;
        private float previousFade;

        protected override bool UseDefaultStartValue => true;
        public override Type TargetComponentType => typeof(CanvasGroup);
        public override string DisplayName => "Fade Canvas Group";

        public float Alpha
        {
            get => alpha;
            set => alpha = value;
        }

        protected override Sequence GenerateTween_Internal(GameObject target, float duration)
        {
            if (canvasGroup == null && !target.TryGetComponent<CanvasGroup>(out canvasGroup))
            {
                Debug.LogError($"{target} does not have {TargetComponentType} component");
                return Sequence.Create();
            }

            previousFade = UseDefaultStartValue ? (canvasGroup.alpha = defaultStartValue) : canvasGroup.alpha;
            Tween innerTween = Tween.Alpha(canvasGroup, alpha, duration);
            return Sequence.Create(innerTween);
        }

        public override void ResetToInitialState()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = UseDefaultStartValue ? previousFade = defaultStartValue : previousFade;
        }

        public override void SampleTo(GameObject target, float normalizedProgress)
        {
            if (canvasGroup == null)
            {
                if (!target.TryGetComponent<CanvasGroup>(out canvasGroup))
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return;
                }
            }

            if (UseDefaultStartValue)
            {
                previousFade = defaultStartValue;
                canvasGroup.alpha = previousFade;
            }
            float temp = Easing.Evaluate(normalizedProgress, Ease);
            canvasGroup.alpha = Mathf.Lerp(previousFade, alpha, temp);
        }
    }
}
#endif