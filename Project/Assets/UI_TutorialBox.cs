using System;
using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class UI_TutorialBox : MonoBehaviour
{
    [Header("Text Printing Settings")]
    public float textPrintingSpeed = 0.03f;
    [Space(10f)]
    [Header("Rect Positions")]
    [SerializeField]
    private RectTransform frame;
    [SerializeField]
    private RectTransform leftFrameAncPosition;
    [SerializeField]
    private RectTransform rightFrameAncPosition;
    [Space(10f)]
    [Header("TMP")]
    [SerializeField] private TMP_Text left_TMP_Component;
    [TextArea]
    public string left_TMP_Text;
    [SerializeField] private TMP_Text right_TMP_Component;
    [TextArea]
    public string right_TMP_Text;
    [Space(10f)] [Header("Interval")] 
    public float interval;
    public RectTransform[] onLeftUI_Objects;
    private Vector2[] onLeftUI_defaultRectPositions;
    public RectTransform[] onRightUI_Objects;
    private Vector2[] onRight_defaultRectPositions;
    
    [Range(0, 10)] public float[] waveAmounts;
 


    // 코루틴 WaitForSeconds 캐싱 자료사전
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }
    
    private Coroutine _textPrintingCoroutine;
    
   // public float waveSpeed;

    private bool _isSideChanged;
    
    private void Awake()
    {
        onLeftUI_defaultRectPositions = new Vector2[onLeftUI_Objects.Length];
        onRight_defaultRectPositions = new Vector2[onRightUI_Objects.Length];
        
        
        for (int i = 0; i < onLeftUI_Objects.Length ; i++)
        {
            onLeftUI_defaultRectPositions[i] = onLeftUI_Objects[i].anchoredPosition;
        }
        
        for (int i = 0; i < onLeftUI_Objects.Length ; i++)
        {
            onRight_defaultRectPositions[i] = onRightUI_Objects[i].anchoredPosition;
        }
        
    }

    private void Start()
    {
        OnLeft();
    }

   
    private void OnLeft()
    {
        Debug.Log("Left Anim재생중..");
        _textPrintingCoroutine = StartCoroutine(TypeInCoroutine(left_TMP_Component, left_TMP_Text));
   
        
        onLeftUI_Objects[0].DOShakePosition(interval, new Vector2(0,  waveAmounts[0]),vibrato,0);
        if (onLeftUI_Objects.Length >= 2)
        {
            onLeftUI_Objects[1].DOShakePosition(interval, new Vector2(0,  waveAmounts[1]),vibrato,0);
        }
         // 예를 들기 위해 사용되는 RectTransform 변수
        
        // OPTION1
        // frame.DOMove(leftFrameAncPosition.anchoredPosition, 1.5f)
        //     .SetDelay(3f) // 3초 대기
        //     .OnComplete(OnRight);
        
        // OPTION2
        // DoMove후 n초대기 한다음 콜백하고 싶은 경우.
        DOTween.Sequence()
            .Append(frame.DOAnchorPos(leftFrameAncPosition.anchoredPosition, 1.5f))
            .AppendInterval(interval)
            .OnComplete(()=>
            {
                Debug.Log("닷트윈 애니메이션 중단!");
                foreach (RectTransform rect in onLeftUI_Objects)
                {
                    rect.DOKill();// 애니메이션 중단
                }
                OnRight();
            });
            
    }
   
    // private void DtMove(RectTransform rect,Vector2 defaultAncPos, float waveAmount)
    // {
    //     rect.DOMove(defaultAncPos + Vector2.one * waveAmount, waveSpeed)
    //         .OnComplete(() =>
    //         {
    //             if (_isSideChanged == true)
    //             {
    //                 Debug.Log("닷트윈 애니메이션 중단!");
    //                 rect.DOKill();// 애니메이션 중단
    //                 _isSideChanged = false;
    //                 return;
    //             }
    //             DtMove(rect, defaultAncPos, - waveAmount);
    //         });
    // }

    public int vibrato;
    private void OnRight()
    {
        Debug.Log("Right Anim재생중..");
        _textPrintingCoroutine = StartCoroutine(TypeInCoroutine(right_TMP_Component, right_TMP_Text));
        
        onRightUI_Objects[0].DOShakePosition(interval, new Vector2(0,  waveAmounts[0]),vibrato,0);
        if (onRightUI_Objects.Length >= 2)
        {
            onRightUI_Objects[1].DOShakePosition(interval, new Vector2(0,  waveAmounts[1]),vibrato,0);
        }
        
        DOTween.Sequence()
            .Append(frame.DOAnchorPos(rightFrameAncPosition.anchoredPosition, 1.5f))
            .AppendInterval(interval)
            .OnComplete(()=>
            {
                foreach (RectTransform rect in onRightUI_Objects)
                {
                    rect.DOKill();// 애니메이션 중단
                }
                OnLeft();
            });
    }


    public IEnumerator TypeInCoroutine(TMP_Text tmp1, string strL)
    {
        tmp1.text = "";

        var strTypingLength = strL.GetTypingLength(); // 최대 타이핑 수 구함
        for (var i = 0; i <= strTypingLength; i++)
        {
            tmp1.text = strL.Typing(i);
            yield return new WaitForSeconds(textPrintingSpeed);

            if (i >= strTypingLength)
            {
                tmp1.text = strL;
                yield break;
            }
        }
    }
}
    

// 레거시 함수. 10-26-23
// onLeftUI_Objects[0]
//         .DOMove(onLeftUI_defaultRectPositions[0] + Vector2.one * waveAmounts[0], waveSpeed)
//         .OnComplete(()=>
//         {
//             onLeftUI_Objects[0]
//                 .DOMove(onLeftUI_defaultRectPositions[0] + Vector2.one * waveAmounts[0], waveSpeed);
//         });
//     index++;
