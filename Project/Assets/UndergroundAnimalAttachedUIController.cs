using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using KoreanTyper;
using Unity.VisualScripting;


public class UndergroundAnimalAttachedUIController : MonoBehaviour
{
   enum messageID
   {
      OnEnable,
      OnNextAnimal
   }
   
   enum tweenParam
   {
      Scale,
      Move
   }
   enum waitTimeName
   {
   InitialOffset, 
   MessagePlayingTime, 
   IntervalBetweenFirstMessageAndSecond
   }
   
   private TMP_Text _tmp;
   private GameObject UIGameObj;
   private Vector3 _initialLocation;
   private Coroutine _typingCoroutine;
   [Header("UI parts")]
   public float textPrintingSpeed;

   [Header("Tween Parameters")] 
   public float maximizedSize;

   private Vector3 _maximizedSizeVec;
   [Space(10)]
   [NamedArrayAttribute(new string[]
   {
      "scale", "move", "3rd", "4th"
   })]
   public float[] durations  = new float[4];
   
   [NamedArrayAttribute(new string[]
   {
      "initialOffset", "messagePlayingTime", "intervalBetweenFirstMessageAndSecond", "4th"
   })]
   public float[] waitTimes  = new float[4];
   
    
   [Space(10)]
   [NamedArrayAttribute(new string[]
   {
      "OnEnable", "OnNextAnimal", "3rd", "4th"
   })]
 
   [TextArea]
   private string OnNext;
   public string[] messages  = new string[4];

   void Awake()
   {
      _maximizedSizeVec = maximizedSize * Vector3.one;
      
      _tmp = GetComponentInChildren<TMP_Text>();
      _tmp.text = $"{gameObject.name} 찾았다!";
       
      
      //초기위치 정보 저장 후, 부모객체 위치로 이동 및 OnEnable에서 다시 UI재생위치로 이동.
      UIGameObj = transform.GetChild(transform.childCount - 1).gameObject;
      _initialLocation = UIGameObj.transform.position;
      UIGameObj.transform.position = gameObject.transform.position;
       
   }
   
   
   void Start()
   {
     
      gameObject.SetActive(false);
   }

   private void OnDisable()
   {
      UIGameObj.transform.localScale = Vector3.zero;
   }

   private bool _isFirstlyEnabled;
   void OnEnable()
   {
      if (!_isFirstlyEnabled)
      {
         _isFirstlyEnabled = true;
         return;
      }
      
      
      
      _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0));

      LeanTween.move(UIGameObj, _initialLocation, durations[(int)tweenParam.Move]);
      
      LeanTween.scale(UIGameObj, _maximizedSizeVec, durations[(int)tweenParam.Scale])
         .setEaseInOutBounce()
         .setOnComplete(()=>  
            LeanTween.delayedCall
               (waitTimes[(int)waitTimeName.IntervalBetweenFirstMessageAndSecond]
               ,PlayNextMessageAnim)
         );
      
   }

   
   
   
   private void PlayNextMessageAnim()
   {
      StopCoroutine(_typingCoroutine);
      _tmp.text = "다음 동물친구를 \n 찾아보자!";
         
         //messages[(int)messageID.OnNextAnimal];
      _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0));

      LeanTween.delayedCall(10f, () =>
         {
            LeanTween.scale(gameObject, Vector3.zero, durations[(int)tweenParam.Scale])
               .setEaseInOutBounce();


            LeanTween.scale(UIGameObj, Vector3.zero, durations[(int)tweenParam.Scale])
               .setEaseInOutBounce()
               .setOnComplete(() =>
               {
                  StopCoroutine(_typingCoroutine);
                  gameObject.SetActive(false);
               });
         }
      );
   }
   
   public IEnumerator TypeIn(string str, float offset)
   {
      Debug.Log("제시문 하단 종료 코루틴 ....");
      _tmp.text = ""; // 초기화
      yield return new WaitForSeconds(offset); // 1초 대기

      var strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
      for (var i = 0; i <= strTypingLength; i++)
      {
         // 반복문
         _tmp.text = str.Typing(i); // 타이핑
         yield return new WaitForSeconds(textPrintingSpeed);
      } // 0.03초 대기


      yield return new WaitForNextFrameUnit();
   }
}
