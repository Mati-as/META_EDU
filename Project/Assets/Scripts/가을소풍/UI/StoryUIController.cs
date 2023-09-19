using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class StoryUIController : MonoBehaviour
{

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

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }

    //  이벤트 상태별 로직------------------------------------------
    private void OnHowToPlayUIFinished()
    {
        Activate();
        _coroutineA = StartCoroutine(ActivateFirstStoryUICoroutine());
    }


    private void OnRoundReady()
    {
        if (_coroutineA != null)
        {
            StopCoroutine(_coroutineA);
        }

        _coroutineA = StartCoroutine(ActivateSecondStoryUICoroutine());
    }


    // 메소드 및 코루틴


    private void Activate() => gameObject.SetActive(true);
    private void Deactivate() => gameObject.SetActive(false);

    public float waitTimeForFirstActivation;
    public float waitTimeForSecondActivation;

    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;

    IEnumerator ActivateSecondStoryUICoroutine()
    {
        yield return GetWaitForSeconds(waitTimeForSecondActivation);
        Activate();

    }

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