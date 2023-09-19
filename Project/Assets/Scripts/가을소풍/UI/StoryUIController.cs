using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class StoryUIController : MonoBehaviour
{
    
  

    [SerializeField]
    private TMP_Text _storyUITmp;
    
    [Header("Message Settings")]  [Space(10f)]
    public string firstUIMessage = "가을을 맞아 동물 친구들이 소풍을 왔어요! 함께 친구들을 만나 보러 가볼까요?"; 
    public string secondUIMessage = "이제 밤이되어 모든 동물 친구들이 숲속으로 사라졌어요!\n친구들을 찾아볼까요?"; 
    
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    // 유니티 루프---------------------------------------------------
    private void Awake()
    {
        _storyUITmp.text = firstUIMessage;
        Deactivate();
        SubscribeGameManagerEvents();
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
    }


  
    public void OnRoundReady()
    {
        if (_coroutineA != null)
        {
            StopCoroutine(_coroutineA);
        }

        _storyUITmp.text = secondUIMessage;
    }


    // 메소드 및 코루틴
    private void Activate() => gameObject.SetActive(true);
    private void Deactivate() => gameObject.SetActive(false);

    public float waitTimeForFirstActivation;
    public float waitTimeForSecondActivation;

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
        Activate();

    }

    private void SubscribeGameManagerEvents()
    {
        // GameManager.onGameStartEvent -= OnGameStart;
        // GameManager.onGameStartEvent += OnGameStart;

        UIManager.HowToPlayUIFinishedEvent -= OnHowToPlayUIFinished;
        UIManager.HowToPlayUIFinishedEvent += OnHowToPlayUIFinished;

        GameManager.onRoundReadyEvent -= OnRoundReady;
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
        //     GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        UIManager.HowToPlayUIFinishedEvent -= OnHowToPlayUIFinished;
        //     GameManager.onCorrectedEvent -= OnCorrect;
        //     GameManager.onRoundFinishedEvent -= OnRoundFinished;
        //     GameManager.onRoundStartedEvent -= OnRoundStarted;
        //     GameManager.onGameFinishedEvent -= OnGameFinished;
        // }
    }
}