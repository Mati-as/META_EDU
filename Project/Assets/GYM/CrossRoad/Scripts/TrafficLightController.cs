using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum LightColor
{
    Red, Orange, Green
}

public class TrafficLightController : MonoBehaviour
{
    [SerializeField] private float redDuration = 10f;
    [SerializeField] private float greenDuration = 10f;


    [SerializeField] private Text leftTime;                          //드래그앤드롭
    [SerializeField] private Text rightTime;                         //드래그앤드롭

    private Color redTextColor = new Color32(209, 80, 61, 255);
    private Color greenTextColor = new Color32(50, 139, 89, 255);

    private float greenChangeTime;
    private float redChangeTime;

    public LightColor CurrentColor
    {
        get; private set;
    }
    public event Action<LightColor> OnLightChanged;

    public Sequence lightSequence;

    public void ChangeTrafficLight()    //시간 바꾸는 로직 변경
    {
        lightSequence = DOTween.Sequence()
            .AppendCallback(() =>
            {
                ChangeTo(LightColor.Green);
                greenChangeTime = greenDuration + 1f;
            })
            .AppendInterval(greenChangeTime)
            .AppendCallback(() =>
            {
                ChangeTo(LightColor.Red);
                redChangeTime = redDuration + 1f;
            })
            .AppendInterval(redChangeTime)
            .SetLoops(20);
    }

    private void ChangeTo(LightColor color)
    {
        CurrentColor = color;
        OnLightChanged?.Invoke(color);
    }

    private void UpdateCountdownText(float time, LightColor color)  //시간 체크하는 용도
    {
        // 초 단위 정수만 보고 싶으면 Mathf.CeilToInt
        int seconds = Mathf.CeilToInt(time);
        switch (color)
        {
            case LightColor.Red:
                leftTime.color = redTextColor;
                rightTime.color = redTextColor;
                break;
            case LightColor.Green:
                leftTime.color = greenTextColor;
                rightTime.color = greenTextColor;
                break;
        }
        leftTime.text = seconds.ToString();
        rightTime.text = seconds.ToString();
    }

}
