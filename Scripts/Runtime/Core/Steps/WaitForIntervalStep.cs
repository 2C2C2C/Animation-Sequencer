#if DOTWEEN_ENABLED
using DG.Tweening;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class WaitForIntervalStep : AnimationStepBase
    {
        public override string DisplayName => "Wait for Interval";

        [SerializeField]
        private float interval;
        public float Interval
        {
            get => interval;
            set => interval = value;
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
#if DOTWEEN_ENABLED
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendInterval(interval);

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
#elif PRIMETWEEN_ENABLED
            AddTweenToSequence_PrimeTween(animationSequence);
#endif
        }

        public override void ResetToInitialState() { }

        public override string GetDisplayNameForEditor(int index)
        {
            return $"{index}. Wait {interval} seconds";
        }


#if PRIMETWEEN_ENABLED

        private void AddTweenToSequence_PrimeTween(Sequence animationSequence)
        {
            Tween callbackTween = Tween.Delay(duration: Delay, () => { });
            Sequence sequence = Sequence.Create(callbackTween);

            if (FlowType == FlowType.Append)
                animationSequence.Chain(sequence);
            else
                animationSequence.Insert(0f, sequence);
        }

#endif

    }
}
