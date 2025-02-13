#if PRIMETWEEN_ENABLED
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PreviewerState = BrunoMikoski.AnimationSequencer.PrimeTweenStepsPreviewer.PreviewerState;

namespace BrunoMikoski.AnimationSequencer
{
    public static class TempPrimeTweenEditorPreview
    {
        private static Dictionary<AnimationSequencerController, PrimeTweenStepsPreviewer> s_collection = new Dictionary<AnimationSequencerController, PrimeTweenStepsPreviewer>();

        [MenuItem("Tools/AnimationSequencer/StopAllEditorPreview")]
        public static void StopAll()
        {
            foreach (KeyValuePair<AnimationSequencerController, PrimeTweenStepsPreviewer> pair in s_collection)
            {
                pair.Value.Stop();
            }
            s_collection.Clear();
        }

        public static void Stop(AnimationSequencerController target)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result))
            {
                result.Dispose();
            }
            s_collection.Remove(target);
        }

        public static bool IsPreviewing(AnimationSequencerController target)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result))
            {
                return PreviewerState.Playing == result.state || PreviewerState.Paused == result.state;
            }
            return false;
        }

        public static void Start(AnimationSequencerController target)
        {
            if (IsPreviewing(target))
            {
                Debug.LogError("Target is already running a preview");
                return;
            }
            Stop(target);
            PrimeTweenStepsPreviewer previewer = new PrimeTweenStepsPreviewer(target);
            s_collection.Add(target, previewer);
            previewer.Play();
        }

        public static void Pause(AnimationSequencerController target)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result) && !result.hasDisposed)
            {
                result.Pause();
            }
        }

        public static void Resume(AnimationSequencerController target)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result) && !result.hasDisposed)
            {
                result.Resume();
            }
        }

        public static bool TryGetProgress(AnimationSequencerController target, out float progress)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result))
            {
                progress = result.hasDisposed ? 0f : result.normalizedPreviewProgress;
                return true;
            }
            progress = 0f;
            return false;
        }

        public static bool IsPaused(AnimationSequencerController target)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result))
            {
                return PreviewerState.Paused == result.state;
            }
            return false;
        }

        public static void SampleProgress(AnimationSequencerController target, float normalizedProgressDelta)
        {
            if (s_collection.TryGetValue(target, out PrimeTweenStepsPreviewer result) && !result.hasDisposed)
            {
                float value = Mathf.Clamp01(normalizedProgressDelta + normalizedProgressDelta);
                result.ChangeProgress(value);
            }
        }

    }

    public class PrimeTweenStepsPreviewer : IDisposable
    {
        public enum PreviewerState
        {
            Disposed = 0,
            Stop = 1,
            Playing = 2,
            Paused = 3,
        }

        private AnimationSequencerController previewTarget;
        private AnimationStepBase[] animSteps;

        private Dictionary<AnimationStepBase, AnimationSequencerController> previewerForChildSteps;

        // preview related data
        // some action steps like 'obj enable' can not be sampled properly, so we need to keep track of them
        private HashSet<AnimationStepBase> recordedActionSteps;
        private float normalizedProgress = 0f;

        private double duration = 0f;
        private double previewCurrentTime = 0f;
        private double editorPreviewCurrentEditorTime;

        // loop
        private int requredTotalLoopCount = 0; // -1 means infinite
        private int completedLoops = 0;
        private CycleMode loopMode = CycleMode.Restart;

        public bool isPreviewRunning => (hasDisposed || PreviewerState.Stop == state);
        public bool hasDisposed { get; private set; } = false;
        public PreviewerState state { get; private set; } = PreviewerState.Stop;
        public float normalizedPreviewProgress
        {
            get
            {
                switch (loopMode)
                {
                    case CycleMode.Yoyo:
                        if (1 == completedLoops % 2)
                        {
                            return 1 - normalizedProgress;
                        }
                        break;
                    default:
                        break;
                }
                return normalizedProgress;
            }
        }

        public PrimeTweenStepsPreviewer(AnimationSequencerController animationSequencer)
        {
            previewTarget = animationSequencer;
            state = PreviewerState.Stop;
            completedLoops = 0;
            hasDisposed = false;
            recordedActionSteps = new HashSet<AnimationStepBase>();
        }

        public void Dispose()
        {
            ThrowIfDisposed();
            hasDisposed = true;
            previewTarget = null;
            animSteps = null;
            state = PreviewerState.Disposed;
            EditorApplication.update -= EditorUpdate;
        }

        public void Pause()
        {
            ThrowIfDisposed();
            if (isPreviewRunning && PreviewerState.Playing == state)
            {
                state = PreviewerState.Paused;
            }
        }

        public void Resume()
        {
            ThrowIfDisposed();
            if (isPreviewRunning && PreviewerState.Paused == state)
            {
                state = PreviewerState.Playing;
            }
        }

        public void Stop()
        {
            EditorApplication.update -= EditorUpdate;
            recordedActionSteps.Clear();
            state = PreviewerState.Stop;
        }

        public void Play()
        {
            ThrowIfDisposed();
            if (PreviewerState.Stop != state)
            {
                return;
            }

            // prepared cached data
            animSteps = previewTarget.AnimationSteps;

            previewCurrentTime = 0f;
            duration = previewTarget.TempCalculateRootSequenceDuration();
            editorPreviewCurrentEditorTime = EditorApplication.timeSinceStartup;

            completedLoops = 0;
            requredTotalLoopCount = previewTarget.Loops;
            loopMode = (CycleMode)previewTarget.LoopType;

            normalizedProgress = 0f;
            state = PreviewerState.Playing;

            EditorApplication.update += EditorUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="normalizedDeltaProgress">value (-1,1)</param>
        public void ChangeProgress(float normalizedDeltaProgress)
        {
            ThrowIfDisposed();
            normalizedProgress = Mathf.Clamp01(normalizedDeltaProgress);
        }

        private void ThrowIfDisposed()
        {
            if (hasDisposed)
            {
                throw new ObjectDisposedException(nameof(PrimeTweenStepsPreviewer));
            }
        }

        private void EditorUpdate()
        {
            if (hasDisposed)
            {
                EditorApplication.update -= EditorUpdate;
                return;
            }

            if (PreviewerState.Stop == state)
            {
                return;
            }

            double current = EditorApplication.timeSinceStartup;
            float editorDeltaTime = (float)(current - editorPreviewCurrentEditorTime);
            editorPreviewCurrentEditorTime = current;

            if (PreviewerState.Paused == state)
            {
                return;
            }

            previewCurrentTime += editorDeltaTime;
            normalizedProgress = Mathf.Clamp01((float)(previewCurrentTime / duration));

            if (previewCurrentTime > duration)
            {
                if (0 == requredTotalLoopCount)
                {
                    Stop();
                }

                completedLoops++;
                recordedActionSteps.Clear();
                if (0 < requredTotalLoopCount && completedLoops >= requredTotalLoopCount)
                {
                    Stop();
                }
                else
                {
                    previewCurrentTime -= duration;
                }
            }

            SampleSteps(normalizedPreviewProgress);
        }

        /// <summary>
        /// sample anim to a specify progress but do not play
        /// </summary>
        /// <param name="normalizedProgress"></param>
        private void SampleSteps(float normalizedProgress)
        {
            FlowType prevStepFlowType = FlowType.Join;
            float fullDuration = (float)duration;
            float sampleTargetSeconds = Mathf.Clamp01(normalizedProgress) * fullDuration;
            float tempFullDuration = 0f;
            float stepStartAtSeconds = 0f;
            for (int i = 0, length = animSteps.Length; i < length; i++)
            {
                AnimationStepBase baseStep = animSteps[i];
                // calculate current possiable start pos
                if (FlowType.Append == prevStepFlowType)
                {
                    stepStartAtSeconds = tempFullDuration;
                }

                float stepDelaySeconds = baseStep.HasDelay ? baseStep.Delay : 0f;
                if (baseStep is PlaySequenceAnimationStep)
                {
                    // TODO preview child sequence step
                    //Debug.LogError($"");
                }
                else if (baseStep is IDelayActionStep delayStep) // action steps used to enable/disable stuff
                {
                    float actionHappenAt = stepStartAtSeconds + stepDelaySeconds;
                    if (actionHappenAt < sampleTargetSeconds && recordedActionSteps.Contains(baseStep)) // has not reached action step
                    {
                        recordedActionSteps.Remove(baseStep);
                        delayStep.DoRevertAction();
                    }
                    else if (actionHappenAt >= sampleTargetSeconds && !recordedActionSteps.Contains(baseStep)) // reached action step
                    {
                        recordedActionSteps.Add(baseStep);
                        delayStep.DoAction();
                    }
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
                {
                    tempFullDuration = currentEndDuration;
                }

                prevStepFlowType = baseStep.FlowType;
            }

        }

    }

}
#endif