using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;

public class InstructionUI : MonoBehaviour
{
    
    private string testString = "강아지의 그림자를 찾아보세요";
    private TMP_Text tmpText;
    public float startTimeOffset; // 게임시작후 몇 초 후 UI재생할 건지
    public float textPrintingSpeed; 

    private void Awake()
    {
        tmpText = GetComponentInChildren<TMP_Text>();
        tmpText.text = string.Empty;
    }
    

    private bool _instructionUIStarted;
    void Update()
    {
        if (GameManager.IsGameStarted)
        {
            if (_instructionUIStarted == false)
            {
                StartCoroutine(TypingCoroutine(testString));
                _instructionUIStarted = true;
            }
            
        }
       
    }

    public IEnumerator TypingCoroutine(string str)
    {
            tmpText.text = ""; // 초기화
            yield return new WaitForSeconds(startTimeOffset); // 1초 대기

            int strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
            for (int i = 0; i <= strTypingLength; i++)
            {
                // 반복문
                tmpText.text = str.Typing(i); // 타이핑
                yield return new WaitForSeconds(textPrintingSpeed);
            } // 0.03초 대기
        

        yield return new WaitForNextFrameUnit();

    }
}
