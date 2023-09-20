using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class UIAudioController : MonoBehaviour
{
    [SerializeField]
    private StoryUIController storyUIController;
    enum UI
    {
        HowToPlayA,
        HowToPlayB,
        StoryA,
        StoryB,
        Finish
    }
    [SerializeField]
    private AudioAndUIData audioAndUIData;
    [SerializeField]
    private GameManager gameManager;

    
    public AudioClip[] uiAudioClip = new AudioClip[6];
    
    // UI Audio 컨트롤
    public Dictionary<string, AudioClip> UIAudioA = new();
    public Dictionary<string, AudioClip> UIAudioB = new();
    public Dictionary<string, AudioClip> AnimalSoundAudio = new();
    // 코루틴 WaitForSeconds 캐싱 자료사전
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    public AudioSource _audioSource;

    private Coroutine _howToPlayACoroutine;
    private Coroutine _howToPlayBCoroutine;
    private Coroutine _storyACoroutine;
    private Coroutine _storyBCoroutine;
    private Coroutine _FinishCoroutine;

  
    private Coroutine uiAudioAReadyCoroutine;
    private Coroutine uiAudioBCorrectCoroutine;
    private Coroutine animalSoundCoroutine;
   
    private void OnEnable()
    {
      
    }

    private void Awake()
    {
        SubscribeGameManagerEvents();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        foreach (AnimalData animalData in gameManager.allAnimals)
        {
            UIAudioA.Add(animalData.englishName,animalData.UIAnimAudioA);
            UIAudioB.Add(animalData.englishName,animalData.UIAnimAudioB);
            AnimalSoundAudio.Add(animalData.englishName,animalData.aninalSound);
        }

        _howToPlayACoroutine = StartCoroutine(PlayHowToPlayAudio());
    }

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }

//----------------------------------------------------
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
        if (!GameManager.isGameFinished)
        {
            Debug.Log("찾아보세요 클립재생");
            uiAudioAReadyCoroutine = StartCoroutine(PlayUI_A_Audio());
        }
    }

    private void OnCorrect()
    {
        if (!GameManager.isGameFinished)
        {
            Debug.Log($"앵무새를 찾았어요 , _isCorrectClipPlayed: {_isCorrectClipPlayed}");
            uiAudioBCorrectCoroutine = StartCoroutine(PlayOnCorrectAudio());
        }
    }

    private void OnRoundFinished()
    {
     
    }

    private void OnGameFinished()
    {  
        
    }


    private void SubscribeGameManagerEvents()
    {
        UIManager.SecondStoryUIActivateEvent -= PlayStoryBAudio;
        UIManager.SecondStoryUIActivateEvent += PlayStoryBAudio;
        
        UIManager.GameFinishUIActivateEvent -= PlayOnGameFinishedEvent;
        UIManager.GameFinishUIActivateEvent += PlayOnGameFinishedEvent;
        
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
        UIManager.SecondStoryUIActivateEvent -= PlayStoryBAudio;
        UIManager.GameFinishUIActivateEvent -= PlayOnGameFinishedEvent;
        
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }

    private Coroutine _playFirstHtpCoroutine;
    public float HTPAAudioWFS;
    public float HTPAAudioInterval;
    private IEnumerator PlayHowToPlayAudio()
    {
        yield return GetWaitForSeconds(HTPAAudioWFS);
        _audioSource.clip = uiAudioClip[(int)UI.HowToPlayA];
        _audioSource.Play();
        
        yield return GetWaitForSeconds(HTPAAudioInterval);
        
        _audioSource.clip = uiAudioClip[(int)UI.HowToPlayB];
        _audioSource.Play();

        yield return GetWaitForSeconds(HTPAAudioInterval);
        Debug.Log("첫번째HTP 오디오 종료");
        StopCoroutine(_howToPlayACoroutine);
    }


    private Coroutine _secondStoryCoroutine;
    private void OnHowToPlayUIFinished()
    {
        _secondStoryCoroutine = StartCoroutine(PlayStoryAudioA());
    }

    public void PlayStoryBAudio()
    {
        _storyBCoroutine = StartCoroutine(PlayStoryAudioB());
    }
    private IEnumerator PlayStoryAudioB()
    {
        yield return null;
        _audioSource.clip = uiAudioClip[(int)UI.StoryB];
        _audioSource.Play();
    }

    public float storyBWFS;
    private IEnumerator PlayStoryAudioA()
    {
        yield return GetWaitForSeconds(HTPAAudioWFS);
        _audioSource.clip = uiAudioClip[(int)UI.StoryA];
        _audioSource.Play();
    }
    
    public float onRoundStartWaitTime;
    public float onCorrectWaitTime;
    public float onGameFinishedWaitTime;

    private IEnumerator PlayOnCorrectAudio()
    {
        _isCorrectClipPlayed = false;
        elapsedForAnimalSound = 0f;
        
        if (uiAudioAReadyCoroutine != null)
        {
            StopCoroutine(uiAudioAReadyCoroutine);
        }
        
        
        while (true)
        {
            if(!_isCorrectClipPlayed)
            {
                yield return GetWaitForSeconds(onCorrectWaitTime);
                if (UIAudioB[GameManager.answer] != null) 
                    _audioSource.clip = UIAudioB[GameManager.answer];
                _audioSource.Play();

                _isCorrectClipPlayed = true;
            }

            if (_isCorrectClipPlayed)
            {
                elapsedForAnimalSound += Time.deltaTime;
                
                if (elapsedForAnimalSound > intervalForAnimalSound)
                {
                    if (AnimalSoundAudio[GameManager.answer] != null)
                    {
                        _audioSource.clip = AnimalSoundAudio[GameManager.answer];
                        _audioSource.Play();
                    }
                }
            }
            
            
            if (GameManager.isGameFinished)
            {
                StopCoroutine(uiAudioBCorrectCoroutine);
            }
            yield return null;
        }

    }

    private float elapsedForAnimalSound;
    public float intervalForAnimalSound;
    private IEnumerator PlayUI_A_Audio()
    {
        if (uiAudioBCorrectCoroutine != null)
        {
            StopCoroutine(uiAudioBCorrectCoroutine);
        }
    
        _isReadyClipPlayed = false;
        
        while (true)
        {
            if(!_isReadyClipPlayed)
            {
                Debug.Log("StoryB 재생중...");
                yield return GetWaitForSeconds(onRoundStartWaitTime);
                if (UIAudioA[GameManager.answer] != null)
                {
                    _audioSource.clip = UIAudioA[GameManager.answer];
                }
                _audioSource.Play();
                _isReadyClipPlayed = true;
            }

           

            yield return null;
        }
       
        
        
    }

    private void PlayOnGameFinishedEvent()
    {
        _audioSource.clip = uiAudioClip[(int)UI.Finish];
        _audioSource.Play();
        _FinishCoroutine =StartCoroutine(PlayOnGameFinishedAudio());
    }

    private IEnumerator PlayOnGameFinishedAudio()
    {
        yield return null;

    }
}