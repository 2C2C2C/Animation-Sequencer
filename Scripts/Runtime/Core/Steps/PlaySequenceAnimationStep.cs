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
    public sealed class PlaySequenceAnimationStep : AnimationStepBase, IDelayActionStep
    {
        private Sequence createdSequence;

        public override string DisplayName => "Play Sequence";

        [SerializeField]
        private AnimationSequencerController sequencer;
        public AnimationSequencerController Sequencer
        {
            get => sequencer;
            set => sequencer = value;
        }

        public Sequence CreatedSequence => createdSequence;

        public override void AddTweenToSequence(Sequence animationSequence)
        {
#if DOTWEEN_ENABLED
            Sequence sequence = sequencer.GenerateSequence();
            sequence.SetDelay(Delay);
            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
            createdSequence = sequence;
#elif PRIMETWEEN_ENABLED
            AddTweenToSequence_PrimeTween(animationSequence);
#endif
        }

        public override void ResetToInitialState()
        {
            sequencer.ResetToInitialState();
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (sequencer != null)
                display = sequencer.name;
            return $"{index}. Play {display} Sequence";
        }

        public void SetTarget(AnimationSequencerController newTarget)
        {
            sequencer = newTarget;
        }

#if PRIMETWEEN_ENABLED

        private void AddTweenToSequence_PrimeTween(Sequence animationSequence)
        {
            DoAction();

            if (FlowType == FlowType.Append)
                animationSequence.Chain(createdSequence);
            else
                animationSequence.Insert(0f, createdSequence);
        }

#endif

        public void DoAction()
        {
#if DOTWEEN_ENABLED
            sequencer.Play();
            return;
#elif PRIMETWEEN_ENABLED
            if (Application.isPlaying) // TODO handle this in the editor
            {
                Tween callbackTween = Tween.Delay(duration: Delay, DoAction);
                createdSequence = Sequence.Create(callbackTween);
                createdSequence.Chain(sequencer.GenerateSequence());
            }
            else
            {
                // editor previewer would handle this
            }
#endif
        }

        public void DoRevertAction()
        {
            if (Application.isPlaying)
            {
                sequencer.Kill();
            }
            else
            {
                // editor previewer would handle this
            }
        }

    }
}