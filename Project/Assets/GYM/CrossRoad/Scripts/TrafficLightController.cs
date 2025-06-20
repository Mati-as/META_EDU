using System;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.UI;

public enum LightColor
{
    Red, Green
}

public class TrafficLightController : MonoBehaviour
{
    [SerializeField] private float redDuration = 20f;
    [SerializeField] private float greenDuration = 20f;


    [SerializeField] private Text leftTime;                          //드래그앤드롭
    [SerializeField] private Text rightTime;                         //드래그앤드롭

    private Color redTextColor = new Color32(209, 80, 61, 255);
    private Color greenTextColor = new Color32(50, 139, 89, 255);

    [SerializeField] private Image leftTrafficSignal;                //드래그앤드롭
    [SerializeField] private Image rightTrafficSignal;               //드래그앤드롭

    private Sprite RedTrafficSignalImg;
    private Sprite GreenTrafficSignalImg;

    [SerializeField] private Image GameOverBG;
    [SerializeField] private Image greenGameOver;
    [SerializeField] private Image redGameOver;

    public LightColor CurrentColor
    {
        get; private set;
    }
    public event Action<LightColor> OnLightChanged;

    public Sequence lightSequence;

    private void Awake()
    {
        RedTrafficSignalImg = Resources.Load<Sprite>("CrossRoad/Image/RedTrafficSignal");
        GreenTrafficSignalImg = Resources.Load<Sprite>("CrossRoad/Image/GreenTrafficSignal");
    }

    public void EndGame()
    {
        lightSequence.Kill();
    }

    public void ChangeTrafficLight()    //시간 바꾸는 로직 변경
    {
        float gTime = greenDuration;
        float rTime = redDuration;

        lightSequence = DOTween.Sequence()
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("건너기 전에 주위를 살피고 건너요", "0_건너기_전에_주위를_살피고_건너요")))
        .JoinCallback(() =>
        {
            leftTrafficSignal.sprite = GreenTrafficSignalImg;
            rightTrafficSignal.sprite = GreenTrafficSignalImg;
            leftTime.color = greenTextColor;
            rightTime.color = greenTextColor;
            leftTime.text = greenDuration.ToString();
            rightTime.text = greenDuration.ToString();
            GameOverBG.DOFade(1, 1);
            greenGameOver.DOFade(1, 1);
        })
        .AppendInterval(4f)
        .AppendCallback(() =>
        {
            GameOverBG.DOFade(0, 1);
            greenGameOver.DOFade(0, 1);
        })
        .AppendInterval(1f)
        .AppendCallback(() => ChangeTo(LightColor.Green))
        .AppendInterval(gTime)
        .Join(
            DOVirtual.Float(
                gTime, 0, gTime, v =>
                {
                    int seconds = Mathf.FloorToInt(v);
                    
                    leftTime.text = seconds.ToString();
                    rightTime.text = seconds.ToString();
                }
            ).SetEase(Ease.Linear)
        )
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("빨간 불에는 건너지 않아요", "1_빨간_불에는_건너지_않아요_")))
        .JoinCallback(() =>
        {
            leftTrafficSignal.sprite = RedTrafficSignalImg;
            rightTrafficSignal.sprite = RedTrafficSignalImg;
            leftTime.color = redTextColor;
            rightTime.color = redTextColor;
            leftTime.text = redDuration.ToString();
            rightTime.text = redDuration.ToString(); 
            GameOverBG.DOFade(1, 1);
            redGameOver.DOFade(1, 1);
        })
        .AppendInterval(4f)
        .AppendCallback(() =>
        {
            GameOverBG.DOFade(0, 1);
            redGameOver.DOFade(0, 1);
        })
        .AppendInterval(1f)
        .AppendCallback(() => ChangeTo(LightColor.Red))
        .AppendInterval(rTime)
        .Join(DOVirtual.Float(rTime, 0, rTime, v =>
                {
                    int seconds = Mathf.FloorToInt(v);
                    leftTime.text = seconds.ToString();
                    rightTime.text = seconds.ToString();
                }
            ).SetEase(Ease.Linear)
        )

        .SetLoops(20, LoopType.Restart);
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
