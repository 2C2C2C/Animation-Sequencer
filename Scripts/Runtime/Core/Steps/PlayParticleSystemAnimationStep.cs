using System;
#if DOTWEEN_ENABLED
using DG.Tweening;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class PlayParticleSystemAnimationStep : AnimationStepBase, IDelayActionStep
    {
        [SerializeField]
        private ParticleSystem particleSystem;
        public ParticleSystem ParticleSystem
        {
            get => particleSystem;
            set => particleSystem = value;
        }

        [SerializeField]
        private float duration = 1;
        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        [SerializeField]
        private bool stopEmittingWhenOver;
        public bool StopEmittingWhenOver
        {
            get => stopEmittingWhenOver;
            set => stopEmittingWhenOver = value;
        }

        public override string DisplayName => "Play Particle System";

        public override void AddTweenToSequence(Sequence animationSequence)
        {
#if DOTWEEN_ENABLED
            animationSequence.SetDelay(Delay);
            animationSequence.AppendCallback(DoAction);
            
            animationSequence.AppendInterval(duration);
            animationSequence.AppendCallback(FinishParticles);
#elif PRIMETWEEN_ENABLED
            AddTweenToSequence_PrimeTween(animationSequence);
#endif
        }

        public override void ResetToInitialState()
        {
            if (null != particleSystem)
            {
                particleSystem.Stop();
            }
        }

        private void FinishParticles()
        {
            if (stopEmittingWhenOver)
            {
                particleSystem.Stop();
            }
        }

        public void SetTarget(ParticleSystem newTarget)
        {
            particleSystem = newTarget;
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (particleSystem != null)
                display = particleSystem.name;
            return $"{index}. Play {display} particle system";
        }

#if PRIMETWEEN_ENABLED

        private void AddTweenToSequence_PrimeTween(Sequence animationSequence)
        {
            Tween callbackTween = Tween.Delay(duration: Delay, DoAction);
            Sequence sequence = Sequence.Create(callbackTween);

            if (FlowType == FlowType.Append)
                animationSequence.Chain(sequence);
            else
                animationSequence.Insert(0f, sequence);
        }

#endif

        public void DoAction()
        {
            particleSystem.Play();
        }

        public void DoRevertAction()
        {
            particleSystem.Stop();
        }
    }
}