#if PRIMETWEEN_ENABLED
using System;
using System.Linq;
using UnityEngine;
using PrimeTween;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class PrimeTweenAnimationStep : GameObjectAnimationStep, ISamplableAnimationStep
    {
        public override string DisplayName => "Prime Tween Target";
        [SerializeField]
        private int loopCount;
        public int LoopCount
        {
            get => loopCount;
            set => loopCount = value;
        }

        [SerializeField]
        private CycleMode loopType;
        public CycleMode LoopType
        {
            get => loopType;
            set => loopType = value;
        }

        [SerializeReference]
        private PrimeTweenActionBase[] actions;
        public PrimeTweenActionBase[] Actions
        {
            get => actions;
            set => actions = value;
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            Sequence stepTweens;
            if (1 < loopCount)
            {
                stepTweens = Sequence.Create(cycleMode: loopType, cycles: loopCount);
            }
            else
            {
                stepTweens = Sequence.Create();
            }
            for (int i = 0; i < actions.Length; i++)
            {
                Sequence wrappedTween = actions[i].GenerateTween(Target, Duration);
                if (i == 0 && HasDelay)
                {
                    stepTweens = stepTweens.ChainDelay(Delay);
                }
                stepTweens.Group(wrappedTween);
            }

            if (FlowType == FlowType.Join)
                animationSequence.Group(stepTweens);
            else
                animationSequence.Chain(stepTweens);
        }

        public override void ResetToInitialState()
        {
            for (int i = actions.Length - 1; i >= 0; i--)
            {
                actions[i].ResetToInitialState();
            }
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string targetName = "NULL";
            if (Target != null)
                targetName = Target.name;

            return $"{index}. {targetName}: {String.Join(", ", actions.Select(action => action.DisplayName)).Truncate(45)}";
        }

        public bool TryGetActionAtIndex<T>(int index, out T result) where T : PrimeTweenActionBase
        {
            if (index < 0 || index > actions.Length - 1)
            {
                result = null;
                return false;
            }

            result = actions[index] as T;
            return result != null;
        }

        public void SampleTo(float stepStartAtSeconds, float sampleTargetSeconds)
        {
            if (sampleTargetSeconds <= stepStartAtSeconds)
            {
                ResetToInitialState();
                return;
            }
            for (int i = 0, length = actions.Length; i < length; i++)
            {
                float temp = sampleTargetSeconds - stepStartAtSeconds;
                actions[i].SampleTo(Target, Mathf.Clamp01((temp - Delay) / Duration));
            }
        }

    }
}
#endif