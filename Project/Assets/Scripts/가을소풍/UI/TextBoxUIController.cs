using System;
using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using UnityEngine;

public class TextBoxUIController : MonoBehaviour
{
    [Header("TextBox Frame Move Settings")] 
   
    [SerializeField]
    private Transform frame;
    [SerializeField]
    private Transform leftPosition;
    [SerializeField]
    private Transform rightPosition;

    private float elapsedForFrameMove;
    public float frameMoveDuration;
    
        
    [Header("TMP")]
    [SerializeField] private TMP_Text tmpBodyLeft;
    [SerializeField] private TMP_Text tmpBodyRight;

    [SerializeField] private Transform leftAnimFirst;
    [SerializeField] private Transform leftAnimSecond;
    [SerializeField] private Transform rightAnimal;


    private Transform _defaultPositionLf;
    private Transform _defaultPositionLs;
    private Transform _defaultPositionR;

    public float waitTimeToStart;
    public float textPrintingSpeed = 0.03f;


    public string stringLeft;
    public string stringRight;

    public float textPrintingIntervalBtTwoTmps;

    // 코루틴 WaitForSeconds 캐싱 자료사전
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    private void Awake()
    {
        UIManager.HowToPlayUIFinishedEvent -= OnUIFinished;
        UIManager.HowToPlayUIFinishedEvent += OnUIFinished;
        
        _defaultPositionLf = leftAnimFirst.transform;
        _defaultPositionLs = leftAnimSecond.transform;
        _defaultPositionR = rightAnimal.transform;

        LeftEvent -= OnLeftLetterTyping;
        RightEvent -= OnRightLetterTyping;
        LeftEvent -= MoveFrameOnLeftEvent;
        RightEvent -= MoveFrameOnRightEvent;
        
        LeftEvent += OnLeftLetterTyping;
        RightEvent += OnRightLetterTyping;
        LeftEvent += MoveFrameOnLeftEvent;
        RightEvent += MoveFrameOnRightEvent;

        tmpBodyLeft.text = "";
        tmpBodyRight.text = "";
    }

    private void Start()
    {
        StartCoroutine(TypeInCoroutine(tmpBodyLeft, tmpBodyRight, stringLeft, stringRight));
    }

    private void OnDestroy()
    {
        UIManager.HowToPlayUIFinishedEvent -= OnUIFinished;
        LeftEvent -= OnLeftLetterTyping;
        RightEvent -= OnRightLetterTyping;
        LeftEvent -= MoveFrameOnLeftEvent;
        RightEvent -= MoveFrameOnRightEvent;
    }

    //메소드 목록 -----------------------------------------------------------

    public static event Action LeftEvent;
    public static event Action RightEvent;

    public IEnumerator TypeInCoroutine(TMP_Text tmp1, TMP_Text tmp2, string strL, string strR)
    {
        yield return GetWaitForSeconds(waitTimeToStart);

        while (true)
        {
           
            LeftEvent?.Invoke();
            
            tmp1.text = "";
            var strTypingLength = strL.GetTypingLength(); // 최대 타이핑 수 구함
            for (var i = 0; i <= strTypingLength; i++)
            {
                tmp1.text = strL.Typing(i);
                yield return new WaitForSeconds(textPrintingSpeed);
            }

            yield return GetWaitForSeconds(textPrintingIntervalBtTwoTmps);


            RightEvent?.Invoke();

            tmp2.text = "";
            var strTypingLength2 = strR.GetTypingLength();
            for (var i = 0; i <= strTypingLength2; i++)
            {
                tmp2.text = strR.Typing(i);
                yield return new WaitForSeconds(textPrintingSpeed);
            }

            yield return GetWaitForSeconds(textPrintingIntervalBtTwoTmps);
            
            
        }
    }

    private Coroutine _moveFrameCoroutine;
    private void MoveFrameOnRightEvent()
    {
        _moveFrameCoroutine = 
            StartCoroutine(MoveFrame(leftPosition, rightPosition , frameMoveDuration));
    }

    private bool _isFirstUIPlayed;
    private void MoveFrameOnLeftEvent()
    {
        //첫번째 프레임의 불필요한 움직임 방지용 _isFirstUIPlayed bool활용.
        if (_isFirstUIPlayed)
        {
            _moveFrameCoroutine = 
                StartCoroutine(MoveFrame(rightPosition, leftPosition , frameMoveDuration));
        }
        else
        {
            
            _isFirstUIPlayed = true;
        }
    }
       

    private IEnumerator MoveFrame(Transform start, Transform arrival, float duration)
    {
        elapsedForFrameMove = 0f;
        
        while (true)
        {
            elapsedForFrameMove += Time.deltaTime;
            float lerp = Lerp2D.EaseOutQuint(0, 1, elapsedForFrameMove / duration);
            frame.position = Vector3.Lerp(start.position, arrival.position, lerp);
            yield return null;
        }
        
    }


    [Range(0, 10)] public float offsetA;
    [Range(0, 10)] public float offsetB;
    [Range(0, 10)] public float offsetC;


    public float waveSpeed;
    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;

    private void OnLeftLetterTyping()
    {
        Debug.Log("UI Animal Is Moving..");
        _coroutineA = StartCoroutine(LeftCoroutine());
        if (_coroutineB != null) StopCoroutine(_coroutineB);
    }

    private void OnRightLetterTyping()
    {
        _coroutineB = StartCoroutine(RightCoroutine());
        if (_coroutineA != null) StopCoroutine(_coroutineA);
    }

    
    public void OnUIFinished()
    {
        if (_coroutineA != null) StopCoroutine(_coroutineA);
        if (_coroutineB != null) StopCoroutine(_coroutineB);
        if (_moveFrameCoroutine != null) StopCoroutine(_moveFrameCoroutine);

        _coroutineC = StartCoroutine(MoveDownUIBoxCoroutine());
    }

    // 코루틴 및 함수 목록..

    private IEnumerator MoveDownUIBoxCoroutine()
    {
        Debug.Log("UI is going down....");
        while (true)
        {
            MoveDownUITBox();
            yield return null;

            //if (GameManager.isGameStarted) gameObject.SetActive(false);
        }
    }

    [SerializeField] private Transform uiMoveDownPosition;

    private float uiMoveElapsed;
    public float uiMoveTimeTotal;

    private void MoveDownUITBox()
    {
        uiMoveElapsed += Time.deltaTime;
        var lerp = Lerp2D.EaseInOutBack(transform.position,
            uiMoveDownPosition.position, uiMoveElapsed / uiMoveTimeTotal);
        var position = new Vector3(lerp.x, lerp.y, transform.position.z);
        transform.position = position;
    }

    private IEnumerator LeftCoroutine()
    {
        sinElapsed = 0f;
        while (true)
        {
            leftAnimFirst.position = MoveLikeWave(_defaultPositionLf.position,
                _defaultPositionLf.position + new Vector3(0, offsetA, 0), waveSpeed, offsetA);

            leftAnimSecond.position = MoveLikeWave(_defaultPositionLs.position,
                _defaultPositionLs.position + new Vector3(0, offsetB, 0), waveSpeed, offsetB);

            //rightAnimal.position = MoveDown(rightAnimal.position, _defaultPositionR.position, 1);
            yield return null;
        }
    }

    private IEnumerator RightCoroutine()
    {
        sinElapsed = 0f;
        while (true)
        {
            rightAnimal.position = MoveLikeWave(_defaultPositionR.position,
                _defaultPositionR.position + new Vector3(0, offsetC, 0), waveSpeed, offsetC);

            //leftAnimFirst.position = MoveDown(leftAnimFirst.position, _defaultPositionLf.position, 1);
            //leftAnimSecond.position = MoveDown(leftAnimSecond.position, _defaultPositionLs.position, 1);
            yield return null;
        }
    }

    private Vector3 MoveLikeWave(Vector3 startPosition, Vector3 arrivingPosition, float moveSpeed, float offsetA)
    {
        sinElapsed += Time.deltaTime * moveSpeed;
        var waveOffset = Mathf.Sin(sinElapsed) * offsetA;
        return startPosition + new Vector3(0, waveOffset, 0);
    }

    private float sinElapsed;
    private float t2;

    private Vector3 MoveDown(Vector3 startPosition, Vector3 arrivingPosition, float moveTime)
    {
        t2 = Time.deltaTime / moveTime;

        float lerp;
        lerp = Lerp2D.EaseInOutQuint(0, 1, sinElapsed);

        return Vector3.Lerp(startPosition,
            arrivingPosition + new Vector3(0, offsetA, 0), lerp);
    }
}