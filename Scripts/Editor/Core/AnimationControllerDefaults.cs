#if DOTWEEN_ENABLE
using DG.Tweening;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [CreateAssetMenu(menuName = "Animation Sequencer/Create Animation Sequencer Default", fileName = "AnimationControllerDefaults")]
    public sealed class AnimationControllerDefaults : EditorDefaultResourceSingleton<AnimationControllerDefaults>
    {
        [SerializeField]
        private bool preferUsingPreviousActionEasing = true;
        public bool PreferUsingPreviousActionEasing => preferUsingPreviousActionEasing;

#if DOTWEEN_ENABLED
        [SerializeField]
        private UpdateType updateType = UpdateType.Normal;
        public UpdateType UpdateType => updateType;

        [SerializeField]
        private CustomEase defaultDOTweenEasing = CustomEase.InOutQuad;
        public CustomEase DefaultDOTweenEasing => defaultDOTweenEasing;

        [SerializeField]
        private DOTweenActionBase.AnimationDirection defaultDirection = DOTweenActionBase.AnimationDirection.To;
        public DOTweenActionBase.AnimationDirection DefaultDirection => defaultDirection;
#endif

#if PRIMETWEEN_ENABLED
        [SerializeField]
        private Ease defaultPrimeTweenEase = 0;
        public Ease DefaultPrimeTweenEasing => defaultPrimeTweenEase;
#endif

        [SerializeField]
        private bool preferUsingPreviousDirection = true;
        public bool PreferUsingPreviousDirection => preferUsingPreviousDirection;

        [SerializeField]
        private bool useRelative = false;
        public bool UseRelative => useRelative;

        [SerializeField]
        private bool preferUsingPreviousRelativeValue = true;
        public bool PreferUsingPreviousRelativeValue => preferUsingPreviousRelativeValue;

        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Awake;
        public AutoplayType AutoplayMode => autoplayMode;

        [SerializeField]
        private bool timeScaleIndependent = false;
        public bool TimeScaleIndependent => timeScaleIndependent;

        [SerializeField]
        private bool autoKill = true;
        public bool AutoKill => autoKill;
    }
}
