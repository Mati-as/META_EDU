
using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    private TMP_Text tmpText;

    [FormerlySerializedAs("startTimeOffset")]
    public float startTimeOffsetSeconds; // 게임시작후 몇 초 후 UI재생할 건지

    public float textPrintingSpeed;


    public Dictionary<string, string> animalNameToKorean = new();
    public string roundInstruction;

    [Header("On Correct")] [Space(10f)] public float onCorrectOffsetSeconds;
    public string onCorrectMessage;
    public string onFinishMessage = "동물친구들을 모두 찾았어요!";

    [Header("Reference")] [Space(10f)] [SerializeField]
    private GameManager _gameManager;
    
    // UI status
    public static bool isIntroUIFinished;
    public static void FinishIntroUI()
    {
        isIntroUIFinished = true;
    }


    /*
    아래 코루틴 변수들은 IEnumerator 컨테이너 역할만 담당합니다.
    어떤 함수가 사용되는지는 StartCoroutine에서확인 및 디버깅 해야합니다.
    */
    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;
    private Coroutine _coroutineD;
    private Coroutine[] _coroutines;
    
    private void Awake()
    {
        SetCoroutine();
        SubscribeGameManagerEvents();
        tmpText = GetComponentInChildren<TMP_Text>();
        tmpText.text = string.Empty;
    }

    private void Start()
    {
        foreach(AnimalData animalData in _gameManager.allAnimals)
        {
            animalNameToKorean.Add(animalData.englishName,animalData.koreanName);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }


    private bool _isQuizPlaying;
    private bool _isCorrectMessagePlaying;
    private UnityEvent _changeUI;


    
    //---------------------------------------------------
    private void SetCoroutine()
    {
        _coroutines = new Coroutine[4];
        _coroutines[0] = _coroutineA;
        _coroutines[1] = _coroutineB;
        _coroutines[2] = _coroutineC;
        _coroutines[3] = _coroutineD;

    }
    
    private void StopCoroutineWithNullCheck(Coroutine[] coroutines)
    {
        Debug.Log("코루틴 종료");
        foreach (Coroutine cR in coroutines)
        {
            if (cR  != null)
            {
                StopCoroutine(cR);
            }
        }
    }

    public void PlayOnCorrectMessage()
    {
       
        _isCorrectMessagePlaying = false;
        Debug.Log("정답이에요 문구 업데이트");
        if (_isCorrectMessagePlaying == false)
        {
            
            _isCorrectMessagePlaying = true; //중복재생 방지
            onCorrectMessage = $"{animalNameToKorean[GameManager.answer]}" +
                               $"{EulOrReul(animalNameToKorean[GameManager.answer])}" + " 찾았어요!";
            
            _coroutines[0] = StartCoroutine(TypeIn(onCorrectMessage, onCorrectOffsetSeconds));
        }
    }

    public void PlayQuizMessage()
    {
        _isCorrectMessagePlaying = false; // PlayOnCorrectMessage 재생을 위한 초기화.
     

        if (GameManager.isGameStarted && _isQuizPlaying == false && !GameManager.isCorrected && !_isCorrectMessagePlaying)
        {
            StopCoroutineWithNullCheck(_coroutines);
#if DEFINE_TEST
            Debug.Log($"퀴즈 업데이트, 정답 : {animalNameToKorean[GameManager.answer]}");
#endif

            _isQuizPlaying = true;
            roundInstruction = $"{animalNameToKorean[GameManager.answer]}의 그림자를 찾아보세요";
            _coroutines[0] = StartCoroutine(TypeIn(roundInstruction, startTimeOffsetSeconds));
        }
    }

    public void PlayFinishMessage()
    {
        _isCorrectMessagePlaying = false; // PlayOnCorrectMessage 재생을 위한 초기화.
        
      
        if (_isQuizPlaying == false && GameManager.isGameFinished)
        {
            StopCoroutineWithNullCheck(_coroutines);
            _isQuizPlaying = true;
            roundInstruction = onFinishMessage;
            _coroutines[0] = StartCoroutine(TypeIn(roundInstruction, startTimeOffsetSeconds));
        }
    }

    public void InitializeMessage()
    {
#if DEFINE_TEST
        Debug.Log("퀴즈 초기화");
#endif

        _isQuizPlaying = false;
        _coroutines[0] = StartCoroutine(TypeIn(string.Empty, 0));
    }


    public IEnumerator TypeIn(string str, float offset)
    {
        tmpText.text = ""; // 초기화
        yield return new WaitForSeconds(offset); // 1초 대기

        var strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
        for (var i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            tmpText.text = str.Typing(i); // 타이핑
            yield return new WaitForSeconds(textPrintingSpeed);
        } // 0.03초 대기


        yield return new WaitForNextFrameUnit();
    }

    private Coroutine _typeInCoroutine;
    
    // 주어진 문자가 한글 범위 내에 있고, 받침이 있다면 true를 반환합니다.
    // 주어진 문자열의 마지막 문자가 한글 범위 내에 있고, 받침이 있다면 true를 반환합니다.
    public string EulOrReul(string str)
    {
        return LastCharHasJongsung(str) ? "을" : "를";
    }

    // 주어진 문자열의 마지막 문자가 한글 범위 내에 있고, 받침이 있다면 true를 반환합니다.
    private bool LastCharHasJongsung(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;

        char c = str[str.Length - 1];
        if (c < '가' || c > '힣') return false;

        int charCode = (int)c - 0xAC00;
        int jong = charCode % 28;

        return jong != 0;
    }
    
    private void OnGameStart()
    {
        
    }

    private void OnRoundReady()
    {
        InitializeMessage();
    }
   
    private void OnRoundStarted()
    {
        PlayQuizMessage();
    }

    private void OnCorrect()
    {
        Debug.Log("OnCorrerctMessage");
        PlayOnCorrectMessage();
    }
    
    private void OnRoundFinished()
    {
        
    }
    
    private void OnGameFinished()
    {
        PlayFinishMessage();
    }
    
    
    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
        
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onCorrectedEvent += OnCorrect;

        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundFinishedEvent += OnRoundFinished;

        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onRoundStartedEvent += OnRoundStarted;
        
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }
    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }


}