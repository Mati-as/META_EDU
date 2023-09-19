using System;
using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;



public class TextBoxUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text tmpBodyLeft;
    [SerializeField]
    private TMP_Text tmpBodyRight;

    public float waitTimeToStart;
    public float textPrintingSpeed = 0.03f;


    public string stringLeft;
    public string stringRight;
    
    public float textPrintingIntervalBtTwoTmps; 
    // 코루틴 WaitForSeconds 캐싱 자료사전
    private Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds))
        {
            waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        }
        return waitForSecondsCache[seconds];
    }

    private readonly int GAME_STOP = 0;

    private void Start()
    {
        GameManager.SetTimeScale(GAME_STOP);
        StartCoroutine(TypeIn(tmpBodyLeft, tmpBodyRight, stringLeft, stringRight));
    }

    public IEnumerator TypeIn(TMP_Text tmp1,TMP_Text tmp2, string strL, string strR)
    {
        GetWaitForSeconds(waitTimeToStart); 
        while (true)
        {
            tmp1.text = "";
            var strTypingLength = strL.GetTypingLength(); // 최대 타이핑 수 구함
            for (var i = 0; i <= strTypingLength; i++)
            {
               
                tmp1.text = strL.Typing(i); 
                yield return new WaitForSeconds(textPrintingSpeed);
            } 

            
            GetWaitForSeconds(textPrintingIntervalBtTwoTmps);

            
            tmp2.text = ""; // 초기화
            var strTypingLength2 = strR.GetTypingLength(); // 최대 타이핑 수 구함
            for (var i = 0; i <= strTypingLength2; i++)
            {
               
                tmp2.text = strR.Typing(i); // 타이핑
                yield return new WaitForSeconds(textPrintingSpeed);
            } 
        
            GetWaitForSeconds(textPrintingIntervalBtTwoTmps); 
        }
        
    }
    
}
