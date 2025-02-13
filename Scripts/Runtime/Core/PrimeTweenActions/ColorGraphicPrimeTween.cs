#if PRIMETWEEN_ENABLED
using System;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class ColorGraphicPrimeTween : PrimeTweenActionBase
    {
        [SerializeField]
        private Color color;
        [SerializeField]
        private Color defaultStartColor = Color.white;

        private Graphic targetGraphic;
        private Color previousColor;

        protected override bool UseDefaultStartValue => true;
        public override Type TargetComponentType => typeof(Graphic);
        public override string DisplayName => "Color Graphic";

        protected override Sequence GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetGraphic == null && !target.TryGetComponent<Graphic>(out targetGraphic))
            {
                Debug.LogError($"{target} does not have {TargetComponentType} component");
                return Sequence.Create();
            }

            previousColor = UseDefaultStartValue ? (targetGraphic.color = defaultStartColor) : targetGraphic.color;
            Tween colorTween = Tween.Color(targetGraphic, previousColor, color, duration: duration, ease: Ease);
            Sequence graphicTween = Sequence.Create(colorTween);
            return graphicTween;
        }

        public override void ResetToInitialState()
        {
            if (targetGraphic == null)
                return;

            targetGraphic.color = UseDefaultStartValue ? previousColor = defaultStartColor : previousColor;
        }

        public override void SampleTo(GameObject target, float normalizedProgress)
        {
            if (targetGraphic == null && !target.TryGetComponent<Graphic>(out targetGraphic))
            {
                Debug.LogError($"{target} does not have {TargetComponentType} component");
                return;
            }

            if (UseDefaultStartValue)
                targetGraphic.color = previousColor = defaultStartColor;
            else
                previousColor = targetGraphic.color;

            float temp = Easing.Evaluate(normalizedProgress, Ease);
            targetGraphic.color = Color.Lerp(previousColor, color, temp);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Work around a Unity bug where updating the colour does not cause any visual change outside of PlayMode.
                // https://forum.unity.com/threads/editor-scripting-force-color-update.798663/
                // https://discussions.unity.com/t/updating-an-image-color-in-an-editor-script/839412/4
                Vector3 localScale = targetGraphic.transform.localScale;
                targetGraphic.transform.localScale = 2 * localScale;
                targetGraphic.transform.localScale = localScale;
            }
#endif
        }

    }
}
#endif