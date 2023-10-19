using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using KoreanTyper;
using Unity.VisualScripting;
using UnityEngine.Serialization;

#if UNITY_EDITOR 
using MyCustomizedEditor;
#endif
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
   private Vector3 _textBoxInitialPosition;

   private Coroutine _typingCoroutine;
   [Header("UI parts")]
   public float textPrintingSpeed;

   [FormerlySerializedAs("maximizedSize")] [Header("Tween Parameters")] 
   public float UImaximizedSize;

   public float animalMaximizedSize;

   private Vector3 _maximizedSizeVec;
   [Space(10)]
#if UNITY_EDITOR 
   [NamedArrayAttribute(new string[]
   {
      "scale", "move", "3rd", "4th"
   })]
#endif
 
   public float[] durations  = new float[4];
   
   
#if UNITY_EDITOR 
   [NamedArrayAttribute(new string[]
   {
      "initialOffset", "messagePlayingTime", "intervalBetweenFirstMessageAndSecond", "4th"
   })]
#endif
   public float[] waitTimes  = new float[4];
   
    

#if UNITY_EDITOR 
   [NamedArrayAttribute(new string[]
   {
      "OnEnable", "OnNextAnimal", "3rd", "4th"
   })]
#endif
 

   private string OnNext;
   //public string[] messages  = new string[4];

   
   [TextArea]
   public string firstMessage;
   [TextArea]
   public string secondMessage;
   void Awake()
   {  
      //초기위치 정보 저장 후, 부모객체 위치로 이동 및 OnEnable에서 다시 UI재생위치로 이동.
      UIGameObj = transform.GetChild(transform.childCount - 1).gameObject;
      _textBoxInitialPosition = UIGameObj.transform.position;
      UIGameObj.transform.position = gameObject.transform.position;
   
      gameObject.transform.localScale = Vector3.zero;
      
      
      _maximizedSizeVec = UImaximizedSize * Vector3.one;
      
      
      _tmp = GetComponentInChildren<TMP_Text>();
      _tmp.text = $"{gameObject.name} 찾았다!";
     
       
   }
    
   // void Start()
   // {
   //    // gameObject.SetActive(false);
   // }

   private void OnDisable()
   {
      UIGameObj.transform.localScale = Vector3.zero;
   }

   private bool _isFirstlyEnabled = false;
   void OnEnable()
   {
       if (!GroundGameManager.isGameStartedbool)
       {
         gameObject.SetActive(false);
       }
       
       
       else
       {
           
          //animal control section;
          LeanTween.scale(gameObject, Vector3.one * animalMaximizedSize, durations[(int)tweenParam.Scale])
             .setEaseInOutBounce();
      
          //AttachedUI control section;
          _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0));

          LeanTween.move(UIGameObj, _textBoxInitialPosition, durations[(int)tweenParam.Move]);
          LeanTween.scale(UIGameObj, _maximizedSizeVec, durations[(int)tweenParam.Scale])
             .setEaseInOutBounce()
             .setOnComplete(() =>
                {
                   Debug.Log("sizeIncreasing Function worked");
                   LeanTween.delayedCall
                   (waitTimes[(int)waitTimeName.IntervalBetweenFirstMessageAndSecond]
                      , PlayNextMessageAnim);
                }
               
             );
       }
      
      
   }



   private float _waitTimeToDisappear=2.25f;
   
   private void PlayNextMessageAnim()
   {
      StopCoroutine(_typingCoroutine);
      
      if (gameObject.name != "여우")
      {
         _tmp.text = secondMessage;
      }
      else
      {  
         _tmp.text = "동물친구들을 \n 모두 찾았어!";
      }
      
      _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0));

      if (gameObject.name != "여우" && GroundGameManager.isGameFinishedbool == false)
      {
          
         Debug.Log("동물 사라지는 중");
          
         LeanTween.delayedCall(_waitTimeToDisappear, () =>
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
      else
      {
         
      }
   
         //messages[(int)messageID.OnNextAnimal];
      

    
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
