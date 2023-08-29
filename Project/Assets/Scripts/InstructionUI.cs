using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InstructionUI : MonoBehaviour
{
   
    private TMP_Text tmpText;
   
    public float startTimeOffset; // 게임시작후 몇 초 후 UI재생할 건지
    public float textPrintingSpeed;

    
    public Dictionary<string, string>  animalNameToKorean = new();
    public string roundInstruction;
    private void Awake()
    {
        animalNameToKorean.Add("tortoise" , "거북이");
        animalNameToKorean.Add("cat", "고양이");
        animalNameToKorean.Add("rabbit","토끼");
        animalNameToKorean.Add("dog", "강아지");
        animalNameToKorean.Add("parrot", "앵무새");
        animalNameToKorean.Add("mouse","쥐");
        
        tmpText = GetComponentInChildren<TMP_Text>();
        tmpText.text = string.Empty;
    }


    private bool _instructionUIStarted;

    private void Update()
    {
        if (GameManager.isRoundStarted)
            if (_instructionUIStarted == false)
            {
                if (animalNameToKorean.TryGetValue(GameManager.answer, out var value))
                {
                    roundInstruction = $"{value}의 그림자를 찾아보세요";
                }
                 
                    StartCoroutine(TypingCoroutine(roundInstruction));
                    _instructionUIStarted = true;
                
            }
    }

    public IEnumerator TypingCoroutine(string str)
    {
        tmpText.text = ""; // 초기화
        yield return new WaitForSeconds(startTimeOffset); // 1초 대기

        var strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
        for (var i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            tmpText.text = str.Typing(i); // 타이핑
            yield return new WaitForSeconds(textPrintingSpeed);
        } // 0.03초 대기


        yield return new WaitForNextFrameUnit();
    }
}