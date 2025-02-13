#if PRIMETWEEN_ENABLED
using System;
using UnityEngine;
using PrimeTween;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class PrimeTweenActionBase
    {
        public enum AnimationDirection
        {
            To,
            From,
        }

        [SerializeField]
        protected AnimationDirection direction;

        [SerializeField]
        protected bool isRelative;

        [SerializeField]
        protected Ease ease = Ease.InOutCirc;

        protected virtual bool UseDefaultStartValue => false;

        public AnimationDirection Direction
        {
            get => direction;
            set => direction = value;
        }

        public bool IsRelative
        {
            get => isRelative;
            set => isRelative = value;
        }

        public Ease Ease
        {
            get => ease;
            set => ease = value;
        }

        public virtual Type TargetComponentType { get; }
        public abstract string DisplayName { get; }

        protected abstract Sequence GenerateTween_Internal(GameObject target, float duration);

        public Sequence GenerateTween(GameObject target, float duration)
        {
            Sequence sequence = GenerateTween_Internal(target, duration);
            // TODO @Hiko 
            //if (direction == AnimationDirection.From)
            //    // tween.SetRelative() does not work for From variant of "Move To Anchored Position", it must be set
            //    // here instead. Not sure if this is a bug in DOTween or expected behaviour...
            //    sequence.From(isRelative: isRelative);

            //sequence.SetEase(ease);
            //sequence.SetRelative(isRelative);
            return sequence;
        }

        public abstract void ResetToInitialState();

        public abstract void SampleTo(GameObject target, float normalizedProgress);
    }
}
#endif