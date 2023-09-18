using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioController : MonoBehaviour
{
    public AudioClip roundStartClip;
    public AudioClip correctClip;
    public AudioClip finishedClip;

    // 코루틴 WaitForSeconds 캐싱 자료사전
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    private AudioSource _audioSource;

    private void Awake()
    {
        SubscribeGameManagerEvents();
        _audioSource = GetComponent<AudioSource>();
    }


    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }


    private void OnGameStart()
    {
    }

    private void OnRoundReady()
    {
        _isReadyClipPlayed = false;
        _isCorrectClipPlayed = false;
        _isFinishedClipPlayed = false;
       
    }

    private bool _isReadyClipPlayed;
    private bool _isCorrectClipPlayed;
    private bool _isFinishedClipPlayed;

    private void OnRoundStarted()
    {
        if (!_isReadyClipPlayed && !GameManager.isGameFinished)
        {
            _isReadyClipPlayed = true;
            StartCoroutine(PlayRoundStartAudio());
        }
    }

    private void OnCorrect()
    {
        if (!_isCorrectClipPlayed && !GameManager.isGameFinished)
        {
            Debug.Log($"앵무새를 찾았어요 , _isCorrectClipPlayed: {_isCorrectClipPlayed}");
            _isCorrectClipPlayed = true;
            StartCoroutine(PlayOnCorrectAudio());
        }
    }

    private void OnRoundFinished()
    {
     
    }

    private void OnGameFinished()
    {
        if (!_isFinishedClipPlayed)
        {
            _isFinishedClipPlayed = true;
            StartCoroutine(PlayOnGameFinishedAudio());
        }
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

    public float onRoundStartWaitTime;
    public float onCorrectWaitTime;
    public float onGameFinishedWaitTime;

    private IEnumerator PlayRoundStartAudio()
    {
        yield return GetWaitForSeconds(onRoundStartWaitTime);
        _audioSource.clip = roundStartClip;
        _audioSource.Play();
      
       
    }

    private IEnumerator PlayOnCorrectAudio()
    {
        yield return GetWaitForSeconds(onCorrectWaitTime);
        _audioSource.clip = correctClip;
        _audioSource.Play();
        
    }

    private IEnumerator PlayOnGameFinishedAudio()
    {
        yield return GetWaitForSeconds(onGameFinishedWaitTime);
        _audioSource.clip = finishedClip;
        _audioSource.Play();
        
    }
}