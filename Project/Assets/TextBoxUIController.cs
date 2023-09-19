using System;
using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class TextBoxUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text tmpBodyLeft;
    [SerializeField]
    private TMP_Text tmpBodyRight;

    [SerializeField]
    private Transform leftAnimFirst;
    [SerializeField]
    private Transform leftAnimSecond;
    [SerializeField]
    private Transform rightAnimal;


    private Transform _defaultPositionLf;
    private Transform _defaultPositionLs;
    private Transform _defaultPositionR;

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

    private void Awake()
    {
        _defaultPositionLf = leftAnimFirst.transform;
        _defaultPositionLs = leftAnimSecond.transform;
        _defaultPositionR = rightAnimal.transform;

        LeftEvent -= OnLeftLetterTyping;
        RightEvent -= OnRightLetterTyping;
        LeftEvent += OnLeftLetterTyping;
        RightEvent += OnRightLetterTyping;
        
        tmpBodyLeft.text = "";
        tmpBodyRight.text = "";
    }

    private void Start()
    {
        StartCoroutine(TypeInCoroutine(tmpBodyLeft, tmpBodyRight, stringLeft, stringRight));
        
    }
    
    public static event Action LeftEvent;
    public static event Action RightEvent;
    public IEnumerator TypeInCoroutine(TMP_Text tmp1,TMP_Text tmp2, string strL, string strR)
    {
        yield return GetWaitForSeconds(waitTimeToStart); 
        
        while (true)
        {
            tmp1.text = "";
            var strTypingLength = strL.GetTypingLength(); // 최대 타이핑 수 구함
            LeftEvent?.Invoke();
            for (var i = 0; i <= strTypingLength; i++)
            {
                tmp1.text = strL.Typing(i); 
                yield return new WaitForSeconds(textPrintingSpeed);
            }
           

            
            
            yield return GetWaitForSeconds(textPrintingIntervalBtTwoTmps);

            
            tmp2.text = ""; 
            var strTypingLength2 = strR.GetTypingLength(); 
            RightEvent?.Invoke();
            for (var i = 0; i <= strTypingLength2; i++)
            {
               
                tmp2.text = strR.Typing(i); 
                yield return new WaitForSeconds(textPrintingSpeed);
            } 
        
            yield return GetWaitForSeconds(textPrintingIntervalBtTwoTmps); 
        }
        
    }

   
    [Range(0,10)]
    public float offsetA;
    [Range(0,10)]
    public float offsetB;
    [Range(0,10)]
    public float offsetC;


   public float waveSpeed;
   private Coroutine _coroutineA;
   private Coroutine _coroutineB;

    private void OnLeftLetterTyping()
    {
       
      
        Debug.Log("UI Animal Is Moving..");
        _coroutineA = StartCoroutine(LeftCoroutine());
        if (_coroutineB != null)
        {
            StopCoroutine(_coroutineB);
        }
     
        
    }
    private void OnRightLetterTyping()
    {
       
        _coroutineB = StartCoroutine(RightCoroutine());
        if (_coroutineA != null)
        {
            StopCoroutine(_coroutineA);
        }
        
      
    }

  
    IEnumerator LeftCoroutine()
    {
        while (true)
        {
            leftAnimFirst.position =  MoveLikeWave(_defaultPositionLf.position,
                _defaultPositionLf.position + new Vector3(0, offsetA, 0),waveSpeed,offsetA);
        
            leftAnimSecond.position =   MoveLikeWave(_defaultPositionLs.position,
                _defaultPositionLs.position + new Vector3(0, offsetB, 0),waveSpeed,offsetB);

            //rightAnimal.position = MoveDown(rightAnimal.position, _defaultPositionR.position, 1);
            yield return null;
        }
     
    }
    IEnumerator RightCoroutine()
    {
        while (true)
        {
            rightAnimal.position = MoveLikeWave(_defaultPositionR.position,
                _defaultPositionR.position + new Vector3(0, offsetC, 0),waveSpeed,offsetC);
        
            //leftAnimFirst.position = MoveDown(leftAnimFirst.position, _defaultPositionLf.position, 1);
            //leftAnimSecond.position = MoveDown(leftAnimSecond.position, _defaultPositionLs.position, 1);
            yield return null;
        }
       
    }
    private Vector3 MoveLikeWave(Vector3 startPosition, Vector3 arrivingPosition, float moveSpeed,float offsetA)
    {
        t += Time.deltaTime * moveSpeed;
        float waveOffset = Mathf.Sin(t) * offsetA;
        return startPosition + new Vector3(0, waveOffset, 0);
        
    }

    private float t;
    private float t2;
    private Vector3 MoveDown(Vector3 startPosition, Vector3 arrivingPosition, float moveTime)
    {
        t2 = Time.deltaTime / moveTime;
        
        float lerp;
        lerp = Lerp2D.EaseInOutQuint(0, 1, t);
        
        return Vector3.Lerp(startPosition,
            (arrivingPosition + new Vector3(0, offsetA, 0)), lerp);
        
    }

    
    
}
