#if DOTWEEN_ENABLED
using DG.Tweening;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class SetGameObjectActiveStep : AnimationStepBase, IDelayActionStep
    {
        public override string DisplayName => "Set Game Object Active";

        [SerializeField]
        private GameObject targetGameObject;
        [SerializeField]
        private bool targetActiveValue;

        public GameObject TargetGameObject
        {
            get => targetGameObject;
            set => targetGameObject = value;
        }

        public bool Active
        {
            get => targetActiveValue;
            set => targetActiveValue = value;
        }

        private bool wasActive;

        public override void AddTweenToSequence(Sequence animationSequence)
        {
#if DOTWEEN_ENABLED
            wasActive = targetGameObject.activeSelf;
            if (wasActive == active)
                return;

            Sequence behaviourSequence = DOTween.Sequence();
            behaviourSequence.SetDelay(Delay);

            behaviourSequence.AppendCallback(DoAction);
            if (FlowType == FlowType.Join)
                animationSequence.Join(behaviourSequence);
            else
                animationSequence.Append(behaviourSequence);
#elif PRIMETWEEN_ENABLED
            AddTweenToSequence_PrimeTween(animationSequence);
#endif
        }

        public override void ResetToInitialState()
        {
            targetGameObject.SetActive(wasActive);
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (targetGameObject != null)
                display = targetGameObject.name;

            return $"{index}. Set {display} Active: {targetActiveValue}";
        }

        public void DoAction()
        {
            targetGameObject.SetActive(targetActiveValue);
        }

        public void DoRevertAction()
        {
            targetGameObject.SetActive(wasActive);
        }

        private void AddTweenToSequence_PrimeTween(Sequence animationSequence)
        {
            Tween callbackTween = Tween.Delay(duration: Delay, DoAction);
            Sequence sequence = Sequence.Create(callbackTween);

            if (FlowType == FlowType.Append)
                animationSequence.Chain(sequence);
            else
                animationSequence.Insert(0f, sequence);
        }
    }
}