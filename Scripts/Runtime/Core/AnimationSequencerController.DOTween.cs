#if DOTWEEN_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
#if UNITASK_ENABLED
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace BrunoMikoski.AnimationSequencer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Animation Sequencer Controller", 200)]
    public partial class AnimationSequencerController : MonoBehaviour
    {
        [SerializeField]
        private float playbackSpeed = 1f;
        public float PlaybackSpeed => playbackSpeed;
        [SerializeField]
        private int loops = 0;
        [SerializeField]
        private LoopType loopType = LoopType.Restart;
        [SerializeField]
        private bool autoKill = true;

#if UNITY_EDITOR
        private bool requiresReset = false;
#endif

        public bool HasValidSequence => playingSequence != null;
        public bool IsPlaying => playingSequence != null && playingSequence.IsActive() && playingSequence.IsPlaying();
        public bool IsPaused => playingSequence != null && playingSequence.IsActive() && !playingSequence.IsPlaying();

        public void Play()
        {
            Play(null);
        }

        public void Play(Action onCompleteCallback)
        {
            ClearPlayingSequence();

            onFinishedEvent.RemoveAllListeners();

            if (onCompleteCallback != null)
                onFinishedEvent.AddListener(onCompleteCallback.Invoke);

            playingSequence = GenerateSequence();
            playingSequence.Play();
        }

        public void PlayForward(bool resetFirst = true, Action onCompleteCallback = null)
        {
            if (playingSequence == null)
                Play();

            playTypeInternal = PlayType.Forward;
            onFinishedEvent.RemoveAllListeners();

            if (onCompleteCallback != null)
                onFinishedEvent.AddListener(onCompleteCallback.Invoke);

            if (resetFirst)
                SetProgress(0);

            playingSequence.PlayForward();
        }

        public void SetProgress(float targetProgress, bool andPlay = true)
        {
            if (playingSequence == null)
                Play();

            targetProgress = Mathf.Clamp01(targetProgress);

            float duration = playingSequence.Duration();
            float finalTime = targetProgress * duration;
            SetTime(finalTime, andPlay);
        }

        public void Pause()
        {
            if (!IsPlaying)
                return;

            playingSequence.Pause();
        }

        public void Resume()
        {
            if (HasValidSequence && !playingSequence.IsPlaying())
                return;

            playingSequence.Play();
        }

        public void Complete(bool withCallbacks = true)
        {
            if (playingSequence == null)
                return;

            playingSequence.Complete(withCallbacks);
        }

        public void Kill(bool complete = false)
        {
            if (!IsPlaying)
                return;

            playingSequence.Kill(complete);
        }

        public Sequence GenerateSequence()
        {
            Sequence sequence = DOTween.Sequence();

            // Various edge cases exists with OnStart() and OnComplete(), some of which can be solved with OnRewind(),
            // but it still leaves callbacks unfired when reversing direction after natural completion of the animation.
            // Rather than using the in-built callbacks, we simply bookend the Sequence with AppendCallback to ensure
            // a Start and Finish callback is always fired.
            sequence.AppendCallback(() =>
            {
                onStartEvent.Invoke();
            });

            for (int i = 0; i < animationSteps.Length; i++)
            {
                AnimationStepBase animationStepBase = animationSteps[i];
                animationStepBase.AddTweenToSequence(sequence);
            }

            sequence.SetTarget(this);
            sequence.SetAutoKill(autoKill);
            sequence.SetUpdate(updateType, timeScaleIndependent);
            sequence.OnUpdate(() => { onProgressEvent.Invoke(); });
            // See comment above regarding bookending via AppendCallback.
            sequence.AppendCallback(() =>
            {
                onFinishedEvent.Invoke();
            });

            int targetLoops = loops;

            if (!Application.isPlaying)
            {
                if (loops == -1)
                {
                    targetLoops = 10;
                    Debug.LogWarning("Infinity sequences on editor can cause issues, using 10 loops while on editor.");
                }
            }

            sequence.SetLoops(targetLoops, loopType);
            sequence.timeScale = playbackSpeed;
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
            DOTween.Kill(this);
            DOTween.Kill(playingSequence);
            playingSequence = null;
        }

        //private void Update()
        //{
        //    if (progress == -1.0f)
        //        return;

        //    SetProgress(progress);
        //}

#if UNITY_EDITOR

        // Unity Event Function called when component is added or reset.
        private void Reset()
        {
            requiresReset = true;
        }

        // Used by the CustomEditor so it knows when to reset to the defaults.
        public bool IsResetRequired()
        {
            return requiresReset;
        }

        // Called by the CustomEditor once the reset has been completed 
        public void ResetComplete()
        {
            requiresReset = false;
        }

#endif

        //public bool TryGetStepAtIndex<T>(int index, out T result) where T : AnimationStepBase
        //{
        //    if (index < 0 || index > animationSteps.Length - 1)
        //    {
        //        result = null;
        //        return false;
        //    }

        //    result = animationSteps[index] as T;
        //    return result != null;
        //}

        //public void ReplaceTarget<T>(GameObject targetGameObject) where T : GameObjectAnimationStep
        //{
        //    for (int i = animationSteps.Length - 1; i >= 0; i--)
        //    {
        //        AnimationStepBase animationStepBase = animationSteps[i];
        //        if (animationStepBase == null)
        //            continue;

        //        if (animationStepBase is not T gameObjectAnimationStep)
        //            continue;

        //        gameObjectAnimationStep.SetTarget(targetGameObject);
        //    }
        //}

        //public void ReplaceTargets(params (GameObject original, GameObject target)[] replacements)
        //{
        //    for (int i = 0; i < replacements.Length; i++)
        //    {
        //        (GameObject original, GameObject target) replacement = replacements[i];
        //        ReplaceTargets(replacement.original, replacement.target);
        //    }
        //}

        //public void ReplaceTargets(GameObject originalTarget, GameObject newTarget)
        //{
        //    for (int i = animationSteps.Length - 1; i >= 0; i--)
        //    {
        //        AnimationStepBase animationStepBase = animationSteps[i];
        //        if (animationStepBase == null)
        //            continue;

        //        if (animationStepBase is not GameObjectAnimationStep gameObjectAnimationStep)
        //            continue;

        //        if (gameObjectAnimationStep.Target == originalTarget)
        //            gameObjectAnimationStep.SetTarget(newTarget);
        //    }
        //}

    }
}
#endif