#if PRIMETWEEN_ENABLED
using UnityEngine;
using PrimeTween;
using System;
using System.Collections;

namespace BrunoMikoski.AnimationSequencer
{
    // Prime Tween controller part
    public partial class AnimationSequencerController
    {
        private Action onTweenStartCallback;
        private Action onTweenEndCallback;

        public bool HasValidSequence => playingSequence.isAlive;
        public bool IsPlaying => HasValidSequence && !playingSequence.isPaused;
        public bool IsPaused => HasValidSequence && playingSequence.isPaused;

        public void Play()
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            Play(null);
        }

        public void Play(Action onCompleteCallback)
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            ClearPlayingSequence();

            onFinishedEvent.RemoveAllListeners();

            if (onCompleteCallback != null)
                onFinishedEvent.AddListener(onCompleteCallback.Invoke);

            playingSequence = GenerateSequence();
        }

        public void PlayForward(bool resetFirst = true, Action onCompleteCallback = null)
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            if (!playingSequence.isAlive)
                Play();

            onFinishedEvent.RemoveAllListeners();

            if (onCompleteCallback != null)
                onFinishedEvent.AddListener(onCompleteCallback.Invoke);

            if (resetFirst)
                SetProgress(0);
        }

        public void SetTime(float seconds, bool andPlay = true)
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            // sample to specific time first than play?

            if (!playingSequence.isAlive)
                Play();

            //playingSequence.Goto(seconds, andPlay);
        }

        public void SetProgress(float targetProgress, bool andPlay = true)
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            // sample to specific progress first than play?
            if (!playingSequence.isAlive)
                Play();

            targetProgress = Mathf.Clamp01(targetProgress);
            float duration = playingSequence.duration;
            float finalTime = targetProgress * duration;
            SetTime(finalTime, andPlay);
        }

        public void Pause()
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            if (playingSequence.isAlive && !playingSequence.isPaused)
                playingSequence.isPaused = true;
        }

        public void Resume()
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            if (playingSequence.isAlive && playingSequence.isPaused)
                playingSequence.isPaused = false;
        }

        public void Complete(bool withCallbacks = true)
        {
            throw new NotImplementedException("TODO force complete prime tween and ensure callback fired");

            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            if (!playingSequence.isAlive)
                return;
        }

        public void Kill(bool complete = false)
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            if (!IsPlaying)
                return;

            playingSequence.Stop();
        }

        // for prime tween, we should only use this for runtime play
        public virtual Sequence GenerateSequence()
        {
            Sequence sequence = Sequence.Create(cycleMode: loopType, cycles: loops);
            sequence.timeScale = playbackSpeed;

            // play forward
            if (null == onTweenStartCallback)
                onTweenStartCallback = () => { onStartEvent.Invoke(); };
            sequence.InsertCallback(0f, onTweenStartCallback);
            //sequence.AppendCallback(() =>
            //{
            //      nStartEvent.Invoke();
            //});

            for (int i = 0; i < animationSteps.Length; i++)
            {
                AnimationStepBase animationStepBase = animationSteps[i];
                animationStepBase.AddTweenToSequence(sequence);
            }

            // maybe we should cache this action instance since we gonna use it every time
            if (null == onTweenEndCallback)
                onTweenEndCallback = () => { onFinishedEvent.Invoke(); };
            sequence.ChainCallback(onTweenEndCallback);

            return sequence;
        }

        public void ResetToInitialState()
        {
            progress = -1.0f;
            for (int i = animationSteps.Length - 1; i >= 0; i--)
            {
                animationSteps[i].ResetToInitialState();
            }
        }

        public void ClearPlayingSequence()
        {
            if (!Application.isPlaying)
                return; // current Prime can only support runtime play

            if (playingSequence.isAlive)
                playingSequence.Stop();

            playingSequence = default;
        }

        /// <summary>
        /// sample anim to a specify progress but do not play
        /// </summary>
        /// <param name="normalizedProgress"></param>
        public void SetProgress(float normalizedProgress)
        {
            // TODO @Hiko cuz prime tween does not support editor play
            // so we need to grab all anim data, and sample to the actual position then apply anim value
            if (Application.isPlaying)
            {
                if (!playingSequence.isAlive)
                    Play();

                playingSequence.isPaused = true;
                playingSequence.progress = normalizedProgress;
                progress = normalizedProgress;
                return;
            }

            SampleSteps(normalizedProgress);
        }

        public float TempCalculateRootSequenceDuration()
        {
            FlowType prevStepFlowType = FlowType.Join;
            float tempFullDuration = 0f;
            float stepStartAt = 0f;

            for (int i = 0, length = animationSteps.Length; i < length; i++)
            {
                // calculate current possiable start pos
                AnimationStepBase baseStep = animationSteps[i];
                if (FlowType.Append == prevStepFlowType)
                    stepStartAt = tempFullDuration;

                // current step duration
                float stepDuration = 0f;
                if (baseStep is GameObjectAnimationStep animationStep)
                    stepDuration += baseStep.Delay + animationStep.Duration;
                else
                    stepDuration += baseStep.Delay;

                float currentEndDuration = stepStartAt + stepDuration;
                if (currentEndDuration > tempFullDuration)
                    tempFullDuration = currentEndDuration;

                prevStepFlowType = baseStep.FlowType;
            }
            return tempFullDuration;
        }

        private void SampleSteps(float normalizedProgress)
        {
            FlowType prevStepFlowType = FlowType.Join;
            float fullDuration = (float)TempCalculateRootSequenceDuration();
            float sampleTargetSeconds = Mathf.Clamp01(normalizedProgress) * fullDuration;
            float tempFullDuration = 0f;
            float stepStartAtSeconds = 0f;
            for (int i = 0, length = animationSteps.Length; i < length; i++)
            {
                AnimationStepBase baseStep = animationSteps[i];
                // calculate current possiable start pos
                if (FlowType.Append == prevStepFlowType)
                    stepStartAtSeconds = tempFullDuration;


                float stepDelaySeconds = baseStep.HasDelay ? baseStep.Delay : 0f;
                if (baseStep is IDelayActionStep delayStep) // action steps used to enable/disable stuff
                {
                    float actionHappenAt = stepStartAtSeconds + stepDelaySeconds;
                    if (actionHappenAt < sampleTargetSeconds) // has not reached action step
                        delayStep.DoRevertAction();
                    else if (actionHappenAt >= sampleTargetSeconds) // reached action step
                        delayStep.DoAction();
                }
                else if (baseStep is ISamplableAnimationStep samplableStep) // should be tween anim steps
                {
                    samplableStep.SampleTo(stepStartAtSeconds, sampleTargetSeconds);
                }

                // current step duration
                float stepDuration = 0f;
                if (baseStep is GameObjectAnimationStep animationStep)
                    stepDuration += baseStep.Delay + animationStep.Duration;
                else
                    stepDuration += baseStep.Delay;

                float currentEndDuration = stepStartAtSeconds + stepDuration;
                if (currentEndDuration > tempFullDuration)
                    tempFullDuration = currentEndDuration;

                prevStepFlowType = baseStep.FlowType;
            }

        }

    }
}
#endif