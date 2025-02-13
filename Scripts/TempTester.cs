using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class TempTester : MonoBehaviour
{
    public Image imageA;
    public Image imageB;
    public Color targetColor;
    public float duration;
    public float delay = 2f;

    //private void Start()
    //{
    //    Sequence outterBig = Sequence.Create(cycleMode: CycleMode.Restart, cycles: 3);

    //    outterBig.ChainDelay(delay);

    //    Tween colorTween1 = Tween.Color(imageA, targetColor, duration: duration, ease: PrimeTween.Ease.InOutQuad);
    //    Sequence sequence = Sequence.Create(colorTween1);
    //    sequence.SetRemainingCycles(-1);
    //    outterBig.Chain(sequence);

    //    Tween colorTween2 = Tween.Color(imageB, targetColor, duration: duration, ease: PrimeTween.Ease.InOutQuad);
    //    sequence = Sequence.Create(colorTween2);
    //    outterBig.Chain(sequence);
    //}


    [ContextMenu(nameof(Start))]
    private void Start()
    {
        Sequence outterBig = Sequence.Create(cycleMode: CycleMode.Yoyo, cycles: 1);
        Tween colorTween1 = Tween.Color(imageA, Color.blue, targetColor, duration: duration, ease: Ease.InOutQuad);
        Sequence inner = Sequence.Create(colorTween1);
        outterBig.Chain(inner);
        //outterBig.Group(colorTween1);
    }

}
