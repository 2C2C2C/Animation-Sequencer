namespace BrunoMikoski.AnimationSequencer
{
    /* @Hiko
     * Since prime tween does not supprt editor play, we need to manually sample animation steps for our simplify preview.
     * Any animation step uses prime tween should implement this interface.
     */
    public interface ISamplableAnimationStep
    {
        /// <param name="stepStartAtSeconds">The step start at this seconds</param>
        /// <param name="sampleTargetSeconds">The sample position seconds we need</param>
        public void SampleTo(float stepStartAtSeconds, float sampleTargetSeconds);
    }
}