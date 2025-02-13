using System;using UnityEngine;
#if DOTWEEN_ENABLEDusing Sequence = DG.TWeen.Sequence;
#elif PRIMETWEEN_ENABLED                         using Sequence = PrimeTween.Sequence;
#endif
namespace BrunoMikoski.AnimationSequencer{    [Serializable]    public abstract class AnimationStepBase    {        [SerializeField]        private float delay;
        [SerializeField]        private FlowType flowType;        public float Delay => delay;        public bool HasDelay => 0f < Delay;        public FlowType FlowType => flowType;        public abstract string DisplayName { get; }        public abstract void AddTweenToSequence(Sequence animationSequence);        public abstract void ResetToInitialState();        public virtual string GetDisplayNameForEditor(int index)        {            return $"{index}. {this}";        }    }}