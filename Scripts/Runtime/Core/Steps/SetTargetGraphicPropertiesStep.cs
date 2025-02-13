#if DOTWEEN_ENABLED
using DG.Tweening;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class SetTargetGraphicPropertiesStep : AnimationStepBase
    {
        [SerializeField]
        private Graphic targetGraphic;
        [SerializeField]
        private Color targetColor = Color.white;
        [SerializeField]
        private Color defaultStartColor;
        [SerializeField]
        private bool useDefaultStartValue = false;

        private Color originalColor;

        public override string DisplayName => "Set Target Graphic Properties";

        public override void AddTweenToSequence(Sequence animationSequence)
        {
#if DOTWEEN_ENABLED
            Sequence behaviourSequence = DOTween.Sequence();
            behaviourSequence.SetDelay(Delay);

            behaviourSequence.AppendCallback(() =>
            {
                originalColor = targetGraphic.color; 
                targetGraphic.color = targetColor;
            });
            if (FlowType == FlowType.Join)
                animationSequence.Join(behaviourSequence);
            else
                animationSequence.Append(behaviourSequence);
#elif PRIMETWEEN_ENABLED
            AddTweenToSequence_PrimeTween(animationSequence);
#endif
        }

        public override void ResetToInitialState()
        {
            targetGraphic.color = originalColor;
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (targetGraphic != null)
                display = targetGraphic.name;

            return $"{index}. Set {display} Properties";
        }

#if PRIMETWEEN_ENABLED

        private void AddTweenToSequence_PrimeTween(Sequence animationSequence)
        {
            if (targetGraphic == null)
            {
                Debug.LogError($"{nameof(SetTargetGraphicPropertiesStep)} does not have targetGraphic component");
                return;
            }

            if (useDefaultStartValue)
                targetGraphic.color = originalColor = defaultStartColor;
            else
                originalColor = targetGraphic.color;

            Tween callbackTween = Tween.Delay(duration: Delay, () => targetGraphic.color = targetColor);

            if (FlowType == FlowType.Append)
                animationSequence.Chain(callbackTween);
            else
                animationSequence.Insert(0f, callbackTween);
        }

#endif

    }
}
