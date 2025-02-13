using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class GameObjectAnimationStep : AnimationStepBase
    {
        [SerializeField]
        private GameObject target;

        [SerializeField]
        private float duration = 1; // why it is protected, since child class can use dat get property
     
        public GameObject Target => target;
        public float Duration => duration;

        public void SetTarget(GameObject newTarget)
        {
            target = newTarget;
        }
    }
}