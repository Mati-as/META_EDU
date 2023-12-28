using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIAudioController : MonoBehaviour
{
    private enum UI
    {
        HowToPlayA,
        HowToPlayB,
        StoryA,
        StoryB,
        Finish
    }
    
    [Header("References")] [Space(10f)]
    [SerializeField] private StoryUIController storyUIController;
    [SerializeField] private AudioAndUIData audioAndUIData;
    [FormerlySerializedAs("gameManager")] [SerializeField] private AnimalTrip_GameManager animalTripGameManager;

    [Space(15f)] [Header("Audio Clips")] [Space(10f)]
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

    [Space(15f)] [Header("Audio Source")] [Space(10f)]
   public AudioSource narrationAudioSource;
   public AudioSource animalSoundAudioSource;
   public AudioSource correctSoundAudioSource;
   public AudioSource windowOpenSoundAudioSource;

   [Space(15f)] [Header("Effects Clip")] [Space(10f)]

   public AudioClip windowOpenSoundClip;

   public AudioClip correctedSoundClip;
    
    
    [Space(15f)] [Header("Interval/Duration Settings")] [Space(10f)]
    public float HTPAAudioWFS;
    public float HTPAAudioInterval;
    public float onRoundStartWaitTime;
    public float onCorrectWaitTime;
    public float onGameFinishedWaitTime;
    
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
        SubscribeProps();
    }

    private void Start()
    {
        foreach (var animalData in animalTripGameManager.allAnimals)
        {
            UIAudioA.Add(animalData.englishName, animalData.UIAnimAudioA);
            UIAudioB.Add(animalData.englishName, animalData.UIAnimAudioB);
            AnimalSoundAudio.Add(animalData.englishName, animalData.aninalSound);
        }

        _howToPlayACoroutine = StartCoroutine(PlayHowToPlayAudio());
    }

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
        UnSubscribeProps();
    }

//----------------------------------------------------

    private void SubscribeProps()
    {
        WindowController.WindowOpenEvent -= OnWindowOpen;
        WindowController.WindowOpenEvent += OnWindowOpen;
      
    }

    private void UnSubscribeProps()
    {
        WindowController.WindowOpenEvent -= OnWindowOpen;
    }
    private void OnWindowOpen()
    {
        SetAndPlayAudio(windowOpenSoundAudioSource, windowOpenSoundClip);
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
        if (!AnimalTrip_GameManager.isGameFinished)
        {
            Debug.Log("찾아보세요 클립재생");
            uiAudioAReadyCoroutine = StartCoroutine(PlayUI_A_Audio());
        }
    }

    private void OnCorrect()
    {
      
        
        if (!AnimalTrip_GameManager.isGameFinished)
        {
    
            SetAndPlayAudio(correctSoundAudioSource, correctedSoundClip);
            
            Debug.Log($"앵무새를 찾았어요 , _isCorrectClipPlayed: {_isCorrectClipPlayed}");
            uiAudioBCorrectCoroutine = StartCoroutine(PlayOnCorrectAudio());
        }
    }

    private void OnRoundFinished()
    {
        animalSoundAudioSource.Stop();
    }

    private void OnGameFinished()
    {
        PlayOnGameFinishedEvent();
    }


    private void SubscribeGameManagerEvents()
    {
        UIManager.SecondStoryUIActivateEvent -= PlayStoryBAudio;
        UIManager.SecondStoryUIActivateEvent += PlayStoryBAudio;

        // UIManager.GameFinishUIActivateEvent -= PlayOnGameFinishedEvent;
        // UIManager.GameFinishUIActivateEvent += PlayOnGameFinishedEvent;

        AnimalTrip_GameManager.onGameStartEvent -= OnGameStart;
        AnimalTrip_GameManager.onGameStartEvent += OnGameStart;

        AnimalTrip_GameManager.onRoundReadyEvent -= OnRoundReady;
        AnimalTrip_GameManager.onRoundReadyEvent += OnRoundReady;

        AnimalTrip_GameManager.onCorrectedEvent -= OnCorrect;
        AnimalTrip_GameManager.onCorrectedEvent += OnCorrect;

        AnimalTrip_GameManager.onRoundFinishedEvent -= OnRoundFinished;
        AnimalTrip_GameManager.onRoundFinishedEvent += OnRoundFinished;

        AnimalTrip_GameManager.onRoundStartedEvent -= OnRoundStarted;
        AnimalTrip_GameManager.onRoundStartedEvent += OnRoundStarted;

        AnimalTrip_GameManager.onGameFinishedEvent -= OnGameFinished;
        AnimalTrip_GameManager.onGameFinishedEvent += OnGameFinished;
    }

    private void UnsubscribeGamaManagerEvents()
    {
        UIManager.SecondStoryUIActivateEvent -= PlayStoryBAudio;
        UIManager.GameFinishUIActivateEvent -= PlayOnGameFinishedEvent;

        AnimalTrip_GameManager.onGameStartEvent -= OnGameStart;
        AnimalTrip_GameManager.onRoundReadyEvent -= OnRoundReady;
        AnimalTrip_GameManager.onCorrectedEvent -= OnCorrect;
        AnimalTrip_GameManager.onRoundFinishedEvent -= OnRoundFinished;
        AnimalTrip_GameManager.onRoundStartedEvent -= OnRoundStarted;
        AnimalTrip_GameManager.onGameFinishedEvent -= OnGameFinished;
    }

    private Coroutine _playFirstHtpCoroutine;
  

    private IEnumerator PlayHowToPlayAudio()
    {
        yield return GetWaitForSeconds(HTPAAudioWFS);
        narrationAudioSource.clip = uiAudioClip[(int)UI.HowToPlayA];
        narrationAudioSource.Play();

        yield return GetWaitForSeconds(HTPAAudioInterval);

        narrationAudioSource.clip = uiAudioClip[(int)UI.HowToPlayB];
        narrationAudioSource.Play();

        yield return GetWaitForSeconds(HTPAAudioInterval);
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
        narrationAudioSource.clip = uiAudioClip[(int)UI.StoryB];
        narrationAudioSource.Play();
    }

    public float storyBWFS;

    private IEnumerator PlayStoryAudioA()
    {
        yield return GetWaitForSeconds(HTPAAudioWFS);
        narrationAudioSource.clip = uiAudioClip[(int)UI.StoryA];
        narrationAudioSource.Play();
    }

  
    private bool _isAnimalSoundPlaying;
    public float animalSoundDuration;
    private float _animalSoundElapsed;

    private IEnumerator PlayOnCorrectAudio()
    {
        _isCorrectClipPlayed = false;
        _isAnimalSoundPlaying = false;
        elapsedForAnimalSound = 0f;
        _animalSoundElapsed = 0f;

        if (uiAudioAReadyCoroutine != null) StopCoroutine(uiAudioAReadyCoroutine);


        while (true)
        {
            if (!_isCorrectClipPlayed)
            {
                yield return GetWaitForSeconds(onCorrectWaitTime);
                
                if (UIAudioB[AnimalTrip_GameManager.answer] != null)
                    SetAndPlayAudio(narrationAudioSource, UIAudioB[AnimalTrip_GameManager.answer]);

                _isCorrectClipPlayed = true;
            }

            if (!_isCorrectClipPlayed)   yield return null;
            
            elapsedForAnimalSound += Time.deltaTime;

            
            
            
            if (elapsedForAnimalSound < intervalBtwAnimalSoundAndNarration
                || AnimalSoundAudio[AnimalTrip_GameManager.answer] == null
                || _isAnimalSoundPlaying)
            {
                yield return null;
            }
            else
            {
                SetAndPlayAudio(animalSoundAudioSource, AnimalSoundAudio[AnimalTrip_GameManager.answer]);
                _isAnimalSoundPlaying = true;
            }

            
            
            if (_isAnimalSoundPlaying)
            {
                _animalSoundElapsed += Time.deltaTime;
                
                if (_animalSoundElapsed > animalSoundDuration)
                {
                    animalSoundAudioSource.Stop();
                }
            }
          
            
            if (AnimalTrip_GameManager.isGameFinished) StopCoroutine(uiAudioBCorrectCoroutine);
            yield return null;
        }
    }

    private void SetAndPlayAudio(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    private float elapsedForAnimalSound;

    [FormerlySerializedAs("intervalBtwAnimalsoundAndNarration")] [FormerlySerializedAs("intervalForAnimalSound")]
    public float intervalBtwAnimalSoundAndNarration;

    private IEnumerator PlayUI_A_Audio()
    {
        if (uiAudioBCorrectCoroutine != null) StopCoroutine(uiAudioBCorrectCoroutine);

        _isReadyClipPlayed = false;

        while (true)
        {
            if (!_isReadyClipPlayed)
            {
                Debug.Log("StoryB 재생중...");
                yield return GetWaitForSeconds(onRoundStartWaitTime);
                if (UIAudioA[AnimalTrip_GameManager.answer] != null) narrationAudioSource.clip = UIAudioA[AnimalTrip_GameManager.answer];
                narrationAudioSource.Play();
                _isReadyClipPlayed = true;
            }


            yield return null;
        }
    }

    private void PlayOnGameFinishedEvent()
    {
        _FinishCoroutine = StartCoroutine(PlayOnGameFinishedAudio());
    }

    private IEnumerator PlayOnGameFinishedAudio()
    {
        yield return GetWaitForSeconds(onGameFinishedWaitTime);
       
        storyUIController.gameObject.SetActive(true);
        storyUIController.OnFinishUIActiavte();
        
        narrationAudioSource.clip = uiAudioClip[(int)UI.Finish];
        narrationAudioSource.Play();
      
    }
}