using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoreanTyper;
using TMPro;

public class InstructionUI : MonoBehaviour
{
    
    private string testString = "말의 그림자를 찾아보세요";
    private TMP_Text tmpText;

    private void Awake()
    {
        tmpText = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
     
            
            StartCoroutine(TypingCoroutine(testString));
        
    }

    void Update()
    {
      
    }

    public IEnumerator TypingCoroutine(string str)
    {
        tmpText.text = ""; // 초기화
        yield return new WaitForSeconds(3f); // 1초 대기

        int strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
        for (int i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            tmpText.text = str.Typing(i); // 타이핑
            yield return new WaitForSeconds(0.03f);
        } // 0.03초 대기
    }
}
