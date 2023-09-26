using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class StoryUIController : MonoBehaviour
{
    enum UI
    {
        HowToPlayA,
        HowToPlayB,
        StoryA,
        StoryB,
        Finish
    }
    
    
    [SerializeField]
    private UIAudioController _uiAudioController;

    [SerializeField]
    private TMP_Text _storyUITmp;

    [SerializeField] 
    private Transform playerIcon;
    [SerializeField] 
    private Transform playerIconDefault;
    [SerializeField] 
    private Transform playerIconMovePosition;
    
    [Header("Message Settings")]  [Space(10f)]
    private readonly string _firstUIMessage = "가을을 맞아 동물 친구들이 소풍을 왔어요! 함께 친구들을 만나 보러 가볼까요?"; 
    private readonly string  _secondUIMessage = "이제 밤이되어 모든 동물 친구들이 숲속으로 사라졌어요! 친구들을 찾아볼까요?"; 
    private readonly string  _lastUIMessage = "우와! 동물친구들을 모두 찾았어요!"; 
    
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    // 유니티 루프---------------------------------------------------
    private void Awake()
    {
      
        Deactivate();
        SubscribeGameManagerEvents();
    }

    private void Start()
    {
        _storyUITmp.text = _firstUIMessage;
    }

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }

    //  이벤트 상태별 로직------------------------------------------
    public void OnHowToPlayUIFinished()
    {
        Activate();
        _coroutineA = StartCoroutine(ActivateFirstStoryUICoroutine());
        _coroutineB = StartCoroutine(MovePlayerIcon());
    }
    
    public void OnRoundReady()
    {
     
        Debug.Log("UI내용 변경");
    
    }
    
    public void OnGameStart()
    {
        _storyUITmp.text = _secondUIMessage;
    }

    public void OnFinishUIActiavte()
    {
        Debug.Log("게임종료 메세지 출력");
        _storyUITmp.text = _lastUIMessage;
      //  _coroutineA = StartCoroutine(ActivateLastStoryUICoroutine());
    }

    // 메소드 및 코루틴
    private void Activate() => gameObject.SetActive(true);
    private void Deactivate() => gameObject.SetActive(false);

    public float waitTimeForFirstActivation;
    public float waitTimeForSecondActivation;
    public float waitTimeForLastActivation;

    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;

    // IEnumerator ActivateSecondStoryUICoroutine()
    // {
    //     yield return GetWaitForSeconds(waitTimeForSecondActivation);
    //     
    // }

    IEnumerator ActivateFirstStoryUICoroutine()
    {
        yield return GetWaitForSeconds(waitTimeForFirstActivation);
        _uiAudioController.narrationAudioSource.clip
            = _uiAudioController.uiAudioClip[(int)UI.StoryA];
        _uiAudioController.narrationAudioSource.Play();

    }
    
    IEnumerator ActivateLastStoryUICoroutine()
    {
       
        yield return null;
    }

    private float t;
    public float playerIconMoveSpeed;
    IEnumerator MovePlayerIcon()
    {
        while (true)
        {
            t += Time.deltaTime * playerIconMoveSpeed;
            playerIcon.position = Vector3.Lerp(playerIconDefault.position, playerIconMovePosition.position, t);

            
            if (GameManager.isCameraArrivedToPlay)
            {
                if (_coroutineA != null && _coroutineB != null)
                {
                    StopCoroutine(_coroutineA);
                    StopCoroutine(_coroutineB);
                }
           
            }
            
            yield return null;
            
            
        }
    }

    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
         GameManager.onGameStartEvent += OnGameStart;

        UIManager.HowToPlayUIFinishedEvent -= OnHowToPlayUIFinished;
        UIManager.HowToPlayUIFinishedEvent += OnHowToPlayUIFinished;

        UIManager.SecondStoryUIActivateEvent -= OnRoundReady;
        UIManager.SecondStoryUIActivateEvent += OnRoundReady;
            
        UIManager.GameFinishUIActivateEvent -= OnFinishUIActiavte;
        UIManager.GameFinishUIActivateEvent += OnFinishUIActiavte;
            
        GameManager.onGameStartEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        // GameManager.onCorrectedEvent -= OnCorrect;
        // GameManager.onCorrectedEvent += OnCorrect;
        //
        // GameManager.onRoundFinishedEvent -= OnRoundFinished;
        // GameManager.onRoundFinishedEvent += OnRoundFinished;
        //
        // GameManager.onRoundStartedEvent -= OnRoundStarted;
        // GameManager.onRoundStartedEvent += OnRoundStarted;
        //
        // GameManager.onGameFinishedEvent -= OnGameFinished;
        // GameManager.onGameFinishedEvent += OnGameFinished;
    }

    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        UIManager.HowToPlayUIFinishedEvent -= OnHowToPlayUIFinished;
        //     GameManager.onCorrectedEvent -= OnCorrect;
        //     GameManager.onRoundFinishedEvent -= OnRoundFinished;
        //     GameManager.onRoundStartedEvent -= OnRoundStarted;
        UIManager.GameFinishUIActivateEvent  -= OnFinishUIActiavte;
        // }
    }
}