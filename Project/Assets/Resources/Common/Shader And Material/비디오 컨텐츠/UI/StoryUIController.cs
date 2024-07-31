using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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
    private readonly string  _secondUIMessage = "모든 동물 친구들이 숲속으로 숨었어요!\n친구들을 찾아볼까요?"; 
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
        
        SubscribeGameManagerEvents();
    }

    private void Start()
    {
        _storyUITmp.text = _firstUIMessage;
    }

    private void OnDisable()
    {
        Debug.Log("storyUI *비*활성화");
    }

    private void OnEnable()
    {
        Debug.Log("storyUI 활성화");
    }

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }

    //  이벤트 상태별 로직------------------------------------------
    public void OnHowToPlayUIFinished()
    {
        Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/가을소풍/UI/AnimalTrip_Story_A");
        _coroutineA = StartCoroutine(ActivateFirstStoryUICoroutine());
        _coroutineB = StartCoroutine(MovePlayerIcon());
    }
    
    public void OnRoundReady()
    {
     
    
    
    }

    public GameObject _button;
    public void OnGameStart()
    {
        _button.SetActive(false);
        _storyUITmp.text = _secondUIMessage;
    }

    public void OnFinishUIActiavte()
    {
        _button.SetActive(true);
        Debug.Log("게임종료 메세지 출력");
        _storyUITmp.text = _lastUIMessage;
    }

    // 메소드 및 코루틴
   

    public float waitTimeForFirstActivation;
    public float waitTimeForSecondActivation;
    public float waitTimeForLastActivation;

    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;



    IEnumerator ActivateFirstStoryUICoroutine()
    {
        yield return GetWaitForSeconds(waitTimeForFirstActivation);


        if (_uiAudioController != null)
        {
            _uiAudioController.narrationAudioSource.clip
                = _uiAudioController.uiAudioClip[(int)UI.StoryA];
            _uiAudioController.narrationAudioSource.Play();
        }
  

    }
    
    IEnumerator ActivateLastStoryUICoroutine()
    {
        yield return null;
    }

    private float t;
    public float playerIconMoveSpeed;
    IEnumerator MovePlayerIcon()
    {
#if DEFINE_TEST
        Debug.Log($" MovePlayerIcon()");
#endif
        while (true)
        {
            t += Time.deltaTime * playerIconMoveSpeed;
            playerIcon.position = Vector3.Lerp(playerIconDefault.position, playerIconMovePosition.position, t);

            
            if (AnimalTrip_GameManager.isCameraArrivedToPlay)
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
            
        
        AnimalTrip_GameManager.onGameStartEvent -= OnGameStart;
        AnimalTrip_GameManager.onGameStartEvent += OnGameStart;

        // AnimalTrip_UIManager.HowToPlayUIFinishedEvent -= OnHowToPlayUIFinished;
        // AnimalTrip_UIManager.HowToPlayUIFinishedEvent += OnHowToPlayUIFinished;

        AnimalTrip_UIManager.SecondStoryUIActivateEvent -= OnRoundReady;
        AnimalTrip_UIManager.SecondStoryUIActivateEvent += OnRoundReady;
            
        AnimalTrip_UIManager.GameFinishUIActivateEvent -= OnFinishUIActiavte;
        AnimalTrip_UIManager.GameFinishUIActivateEvent += OnFinishUIActiavte;
            
        AnimalTrip_GameManager.onGameStartEvent -= OnRoundReady;
        AnimalTrip_GameManager.onRoundReadyEvent += OnRoundReady;

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
        AnimalTrip_GameManager.onGameStartEvent -= OnGameStart;
        AnimalTrip_GameManager.onRoundReadyEvent -= OnRoundReady;
        // AnimalTrip_UIManager.HowToPlayUIFinishedEvent -= OnHowToPlayUIFinished;
        //     GameManager.onCorrectedEvent -= OnCorrect;
        //     GameManager.onRoundFinishedEvent -= OnRoundFinished;
        //     GameManager.onRoundStartedEvent -= OnRoundStarted;
        AnimalTrip_UIManager.GameFinishUIActivateEvent  -= OnFinishUIActiavte;
        // }
    }
}