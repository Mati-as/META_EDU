
using System;
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
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }
    
    
    
    [Header("Reference")]  [Space(10f)]
    [SerializeField]
    private StoryUIController storyUIController;
    [SerializeField]
    private TextBoxUIController textBoxUIController;
    [SerializeField]
    private UIAudioController uiAudioController;
    
    [Header("Overall Settings")]  [Space(10f)]
    public float startTimeOffsetSeconds; // 게임시작후 몇 초 후 UI재생할 건지
    public float textPrintingSpeed;
    public Dictionary<string, string> animalNameToKorean = new();
    public string roundInstruction;
  
    [SerializeField]
    private TMP_Text instructionTMP;
    [Header("On Correct")] [Space(10f)] public float onCorrectOffsetSeconds;
    public string onCorrectMessage;
    public string onFinishMessage = "동물친구들을 모두 찾았어요!";

    [FormerlySerializedAs("_gameManager")]
    [Header("Reference")] [Space(10f)] 
    [SerializeField]
    private GameManager gameManager;
    
    

    
    // UI status---------------------------
     /*
     게임 플레이와 직접적으로 연관 없는 플레이 시작 시 인트로 UI에 관련한 Status입니다.
     Lerp, UI종료와 UI의 안의 RectTransform 객체의 움직임은 UI Status를 기준으로합니다.
     */
     
     
    public static event Action HowToPlayUIFinishedEvent;
    public static event Action StoryUIQuitEvent;
    public static event Action GameFinishedUIEvent;
    public static bool isHowToPlayUIFinished;
    
    /// <summary>
    /// 버튼이벤트에서 컨트롤 할 예정.
    /// </summary>
    /// <param name="value"></param>
    public static void SetFalseAndTriggerStartButtonEvent()
    {
        isHowToPlayUIFinished = true;
    }
    
    
    /// <summary>
    /// UI 버튼
    /// </summary>
    public static void InvokeFinishIntroUI()
    {
        HowToPlayUIFinishedEvent?.Invoke();
    }
    public static void InvokeStoryUIQuitEvent()
    {
        StoryUIQuitEvent?.Invoke();
    }
    public static void InvokeGameFinishedUIEvent()
    {
        GameFinishedUIEvent?.Invoke();
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
        instructionTMP.text = string.Empty;
    }

    private void Start()
    {
        foreach(AnimalData animalData in gameManager.allAnimals)
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
        Debug.Log("제시문 하단 종료 이벤트 발생!");

        _isQuizPlaying = false;
        if (_isQuizPlaying == false && GameManager.isGameFinished)
        {
            Debug.Log(" 지시문 종료");
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
        Debug.Log("제시문 하단 종료 코루틴 ....");
        instructionTMP.text = ""; // 초기화
        yield return new WaitForSeconds(offset); // 1초 대기

        var strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
        for (var i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            instructionTMP.text = str.Typing(i); // 타이핑
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
        //StoryUI 비활성화 상태이기에 UI Manager가 활성화 및 해당함수 호출
        _coroutineC =  StartCoroutine(ActivateStoryBUIController());
    }

    
    

    public static event Action GameFinishUIActivateEvent;
    public static event Action SecondStoryUIActivateEvent;
    IEnumerator ActivateStoryBUIController()
    {
        yield return GetWaitForSeconds(storyUIController.waitTimeForSecondActivation);
        storyUIController.gameObject.SetActive(true);
        SecondStoryUIActivateEvent?.Invoke();
        GameManager.isGameStopped = true;
        StopCoroutine(_coroutineC);
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
        // StartCoroutine(InvokeFinishUI()); //AudioUI가 StoryUI컨트롤
        PlayFinishMessage();
    }

    
    // private IEnumerator InvokeFinishUI()
    // {
    //     yield return GetWaitForSeconds(storyUIController.waitTimeForFirstActivation);
    //     
    //     storyUIController.gameObject.SetActive(true);
    //     GameFinishUIActivateEvent?.Invoke();
    //
    // }
    
    
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