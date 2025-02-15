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

        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Awake;
        [SerializeField]
        private float playbackSpeed = 1f;
        [SerializeField]
        private int loops = 0;
        [SerializeField]
        private LoopType loopType;

        [SerializeField, Range(0, 1)]
        private float progress = -1;

        private Sequence playingSequence;

        [SerializeField]
        private UnityEvent onStartEvent = new UnityEvent();
        [SerializeField]
        private UnityEvent onFinishedEvent = new UnityEvent();
        [SerializeField]
        private UnityEvent onProgressEvent = new UnityEvent();

        public UnityEvent OnStartEvent => onStartEvent;
        public UnityEvent OnFinishedEvent => onFinishedEvent;
        public UnityEvent OnProgressEvent => onProgressEvent;

        public Sequence PlayingSequence => playingSequence;
        public float TempProgress => progress;
        public int LoopType => (int)loopType;
        public int Loops => loops;

        private void Awake()
        {
            progress = -1;
            if (autoplayMode != AutoplayType.Awake)
                return;

            Play();
        }

        private void OnEnable()
        {
            if (autoplayMode != AutoplayType.OnEnable)
                return;

            Play();
        }

        private void Start()
        {
            if (autoplayMode != AutoplayType.OnStart)
                return;

            Play();
        }

        private void OnDisable()
        {
            if (autoplayMode != AutoplayType.OnEnable)
                return;

#if DOTWEEN_ENABLED
            if (playingSequence == null)
                return;
#elif PRIMETWEEN_ENABLED
            if (!playingSequence.isAlive)
                return;

#endif
            // Reset the object to its initial state so that if it is re-enabled the start values are correct for
            ClearPlayingSequence();
            // regenerating the Sequence.
            ResetToInitialState();
        }

        private void OnDestroy()
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

    }
}
