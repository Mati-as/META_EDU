using System;
using DG.Tweening;
using UnityEngine;

public enum LightColor
{
    Red, Orange, Green
}

public class TrafficLightController : MonoBehaviour
{
    [SerializeField] private float redDuration = 15f;
    [SerializeField] private float greenDuration = 20f;

    public LightColor CurrentColor
    {
        get; private set;
    }
    public event Action<LightColor> OnLightChanged;

    private Sequence _lightSequence;

    public void ChangeTrafficLight()
    {
        _lightSequence = DOTween.Sequence()
            .AppendCallback(() => ChangeTo(LightColor.Green))
            .AppendInterval(greenDuration)
            .AppendCallback(() => ChangeTo(LightColor.Red))
            .AppendInterval(redDuration)
            .SetLoops(-1, LoopType.Restart);
    }

    private void ChangeTo(LightColor color)
    {
        CurrentColor = color;
        OnLightChanged?.Invoke(color);
    }


}
