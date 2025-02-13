using System;
using UnityEngine;
using UnityEngine.Events;
// To separate differnt stuff
#if DOTWEEN_ENABLE
using UpdateType = DG.Tween.UpdateType;
using LoopType = DG.Tween.LoopType;
using Sequence = DG.Tween.Sequence;
#elif PRIMETWEEN_ENABLED
using LoopType = PrimeTween.CycleMode;
using Sequence = PrimeTween.Sequence;
#endif
using System.Collections;
using System.Collections.Generic;
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
        [SerializeReference]
        private AnimationStepBase[] animationSteps = Array.Empty<AnimationStepBase>();
        public AnimationStepBase[] AnimationSteps => animationSteps;

        //[SerializeField]
        //private UpdateType updateType = UpdateType.Normal;
        [SerializeField]
        private bool timeScaleIndependent = false;
        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Awake;
        [SerializeField]
        protected bool startPaused;
        [SerializeField]
        private float playbackSpeed = 1f;
        public float PlaybackSpeed => playbackSpeed;
        [SerializeField]
        private int loops = 0;
        [SerializeField]
        private LoopType loopType;
        [SerializeField]
        private bool autoKill = true; // prime tween will be auto kill, so we need to move it to somewhere or force true value

        [SerializeField]
        private UnityEvent onStartEvent = new UnityEvent();
        [SerializeField]
        private UnityEvent onFinishedEvent = new UnityEvent();
        [SerializeField]
        private UnityEvent onProgressEvent = new UnityEvent();

        public UnityEvent OnStartEvent => onStartEvent;
        public UnityEvent OnFinishedEvent => onFinishedEvent;
        public UnityEvent OnProgressEvent => onProgressEvent;

        [SerializeField, Range(0, 1)]
        private float progress = -1;

        private Sequence playingSequence;
        public Sequence PlayingSequence => playingSequence;
        public float TempProgress => progress;
        public int Loops => loops;
        public int LoopType => (int)loopType;

        protected virtual void Awake()
        {
            progress = -1;
            if (autoplayMode != AutoplayType.Awake)
                return;

            Autoplay();
        }

        protected virtual void OnEnable()
        {
            if (autoplayMode != AutoplayType.OnEnable)
                return;

            Autoplay();
        }

        protected virtual void Start()
        {
            if (autoplayMode != AutoplayType.OnStart)
                return;
            Autoplay();
        }

        protected virtual void OnDisable()
        {
            if (autoplayMode != AutoplayType.OnEnable)
                return;

#if DOTWEEN_ENABLED
            //if (playingSequence == null)
            //    return;
#elif PRIMETWEEN_ENABLED
            if (!playingSequence.isAlive)
                return;

#endif
            // Reset the object to its initial state so that if it is re-enabled the start values are correct for
            ClearPlayingSequence();
            // regenerating the Sequence.
            ResetToInitialState();
        }

        protected virtual void OnDestroy()
        {
            ClearPlayingSequence();
        }

        //private void Update()
        //{
        //    // TODO @Hiko would conflict with actual tween progress in runtime
        //    if (progress == -1.0f)
        //        return;

        //    SetProgress(progress);
        //}

        private void Autoplay()
        {
            Play();
            if (startPaused)
            {
#if DOTWEEN_ENABLED
                playingSequence.Pause();
#elif PRIMETWEEN_ENABLED
                playingSequence.isPaused = true;
#endif
            }
        }

#if UNITASK_ENABLED

        public async UniTask PlayAsync(CancellationToken cancellationTokenSource = default)
        {
            if (cancellationTokenSource == default)
                cancellationTokenSource = this.GetCancellationTokenOnDestroy();
            
            await PlayEnumerator().ToUniTask(PlayerLoopTiming.Update, cancellationTokenSource);
        }

#endif
    }
}
