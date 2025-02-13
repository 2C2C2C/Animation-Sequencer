using System;
using UnityEngine;
using UnityEngine.Events;
#if DOTWEEN_ENABLED
using DG.Tween;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class InvokeCallbackAnimationStep : AnimationStepBase
    {
        [SerializeField]
        private UnityEvent callback = new UnityEvent();

        public UnityEvent Callback
        {
            get => callback;
            set => callback = value;
        }

        public override string DisplayName => "Invoke Callback";

        public override void AddTweenToSequence(Sequence animationSequence)
        {
#if DOTWEEN_ENABLED
            AddTweenToSequence_DGTween(animationSequence);
#elif PRIMETWEEN_ENABLED
            AddTweenToSequence_PrimeTween(animationSequence);
#endif
        }

        public override void ResetToInitialState() { }

        public override string GetDisplayNameForEditor(int index)
        {
            string[] persistentTargetNamesArray = new string[callback.GetPersistentEventCount()];
            for (int i = 0; i < callback.GetPersistentEventCount(); i++)
            {
                if (callback.GetPersistentTarget(i) == null)
                    continue;

                if (string.IsNullOrWhiteSpace(callback.GetPersistentMethodName(i)))
                    continue;

                persistentTargetNamesArray[i] = $"{callback.GetPersistentTarget(i).name}.{callback.GetPersistentMethodName(i)}()";
            }

            string persistentTargetNames = $"{string.Join(", ", persistentTargetNamesArray).Truncate(45)}";
            return $"{index}. {DisplayName}: {persistentTargetNames}";
        }

#if DOTWEEN_ENABLED

        private void AddTweenToSequence_DGTween(Sequence animationSequence)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendCallback(() => callback.Invoke());

            if (FlowType == FlowType.Append)
                animationSequence.Append(sequence);
            else
                animationSequence.Join(sequence);
        }

#endif

#if PRIMETWEEN_ENABLED

        private void AddTweenToSequence_PrimeTween(Sequence animationSequence)
        {
            Tween callbackTween = Tween.Delay(duration: Delay, () => callback.Invoke());
            Sequence sequence = Sequence.Create(callbackTween);

            if (FlowType == FlowType.Append)
                animationSequence.Chain(sequence);
            else
                animationSequence.Insert(0f, sequence);
        }

#endif

    }
}