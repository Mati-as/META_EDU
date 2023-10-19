using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

public class UndergroundUIManager : MonoBehaviour
{
    private enum UI
    {
        HowToPlayA,
        HowToPlayB,
        StoryA,
        StoryB,
        Finish
    }


    [Header("References")] //-------------------------------
    [SerializeField]
    private GroundGameManager gameManager;

    [SerializeField] private FootstepManager footstepManager;

    [Header("Intervals")] //-------------------------------
    public float stagesInterval;

    public float UIInterval;


    [Space(30f)] [Header("Tutorial UI Parts")] //-------------------------------
    public GameObject tutorialUIGameObject;

    public CanvasGroup tutorialUICvsGroup;
    public TMP_Text tutorialTMPText;
    public RectTransform tutorialUIRectTransform;

    [FormerlySerializedAs("tutorialAwayPosition")]
    public RectTransform tutorialAwayTransfrom;

    [Space(10f)] [Header("Tutorial Message Settings")]
    public string tutorialMessage;

    [Space(30f)]
    [Header("Story UI")] //-------------------------------
    [Space(10f)]
    public GameObject storyUIGameObject;

    public CanvasGroup storyUICvsGroup;
    public RectTransform storyUIRectTransform;
    [SerializeField] private RectTransform _storyUIInitialRectPos;
    [SerializeField] private TMP_Text _storyUITmp;
    [SerializeField] private UIAudioController _uiAudioController;

    [Space(10f)] [Header("Story Message Settings")] [Space(10f)]
    public string _firstUIMessage = " 땅속에는 다양한 동물친구가 살고 있어요! 저를 따라오면서 구경해볼까요?";

    public string _lastUIMessage = "우와! 동물친구들을 모두 찾았어요!";

    [Space(30f)] [Header("Audio")] [Space(10f)] //-------------------------------
    public AudioSource uiAudioSource; // 두개 AudioSource중 하단 AudioSource 입니니다. 위에 것은 Button AudioSource

    public AudioClip[] uiAudioClips = new AudioClip[5];

    public AudioClip[] etcAudioClips = new AudioClip[5];
    private enum EtcAudioClips
    {
        WhoIsNext,//다음동물친구는 누굴까
        LetsMeetNext,// 다음동물친구를 찾아보자
        FollowFootsteps, // 발판을 따라가서 개미를 찾아봐!
        FoundAllAnimals // 동물을 모두 찾았어!
        
        
    }
    private enum AudioClips
    {
        Tutorial1, // 놀이 방법을 알아볼까요
        Tutorial2, // 발자국을 따라가며 ~ ..  
        Story, // 땅속에는 ~ ..
        PageOver, // 다음친구가 나와서 찾아볼까요
        GameOver // 우와
    }

    public AudioClip[] animalAudioClips = new AudioClip[12];

    private enum AnimalAudioClips
    {
        Ant,
        Worm,
        Mole,
        Spider,
        Slate,
        Snail,
        Snake,
        Beetle,
        Frog,
        Squirrel,
        Rabbit,
        Fox
    }

    [Space(30f)]
    [Header("etc")]
    [Space(10f)] //-------------------------------
    [SerializeField]
    private Transform playerIcon;

    [SerializeField] private Transform playerIconDefault;
    [SerializeField] private Transform playerIconMovePosition;

    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    private void PlayAudio(AudioClip audioClip)
    {
        uiAudioSource.clip = audioClip;
        uiAudioSource.Play();
    }

    private Coroutine _uiPlayCoroutine;
    private Coroutine _onAnimalFindAudioCoroutine;
    private void Awake()
    {
        tutorialUIGameObject.SetActive(true);


        _uiPlayCoroutine = StartCoroutine(PlayTutorialAudio());


        _storyUIInitialRectPos = storyUIRectTransform;
        DOTween.Init();
        Init();
        tutorialUICvsGroup.DOFade(1, 1);
    }

    private IEnumerator PlayTutorialAudio()
    {
        PlayAudio(uiAudioClips[(int)AudioClips.Tutorial1]);

        while (uiAudioSource.isPlaying) yield return null;


        yield return GetWaitForSeconds(1f);

        PlayAudio(uiAudioClips[(int)AudioClips.Tutorial2]);

        yield return GetWaitForSeconds(3f);
    }

    private void Init()
    {
        storyUICvsGroup.alpha = 0;
        tutorialUICvsGroup.alpha = 0;
    }

    private Coroutine _stageStartCoroutine;
    private void Start()
    {
        gameManager.isStartButtonClicked
            .Where(_=>_ == true)
            .Delay(TimeSpan.FromSeconds(INTRO_UI_DELAY))
            .Subscribe(_ => SetUIIntroUsingUniRx())
            .AddTo(this);

        gameManager.currentStateRP
            .Do(currentState => Debug.Log($"Current state is: {currentState.GameState}"))
            .Where(_currentState => _currentState.GameState == IState.GameStateList.GameStart)
            .Subscribe(_ => OnGameStart())
            .AddTo(this);

        gameManager.currentStateRP
            .Where(_currentState => _currentState.GameState == IState.GameStateList.StageStart)
            // .Delay(TimeSpan.FromSeconds(3f))
            .Subscribe(_ =>
            {
                _stageStartCoroutine = StartCoroutine(OnStageStartCoroutine());
            })
            .AddTo(this);

        gameManager.isGameFinishedRP
            .Where(value => value)
            .Delay(TimeSpan.FromSeconds(8f))
            .Subscribe(_ => OnGameOver())
            .AddTo(this);

        footstepManager.finishPageTriggerProperty
            .Where(value => value)
            .Delay(TimeSpan.FromSeconds(5f))
            .Subscribe(_ => OnPageChange())
            .AddTo(this);

        footstepManager.lastElementClickedProperty
            .Where(_ => _)
            .Delay(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ => OnAnimalFind())
            .AddTo(this);
    }


    
    private void OnAnimalFind()
    {
       
            _onAnimalFindAudioCoroutine = StartCoroutine(PlayOnFindAudios());
          
    }

    private IEnumerator PlayOnFindAudios()
    {
        if (FootstepManager.currentFootstepGroupOrder != 0)
        {
            PlayAudio(animalAudioClips[FootstepManager.currentFootstepGroupOrder - 1]);
        }
        
        yield return GetWaitForSeconds(2.1f);

        // 마지막 동물인 여우가 아닐 때만...
        if (FootstepManager.currentFootstepGroupOrder != 12)
        {
            PlayAudio(etcAudioClips[(int)EtcAudioClips.WhoIsNext]);
        }
        else
        {
            PlayAudio(etcAudioClips[(int)EtcAudioClips.FoundAllAnimals]);
        }
      

       while (uiAudioSource.isPlaying)
       {
           yield return null;
       }
       
       StopCoroutine(_onAnimalFindAudioCoroutine);
        
    }

    private void OnPageChange()
    {
        OnpageChangeCoroutine =StartCoroutine(OnpageChangeWithAudio());
    }

    private Coroutine OnpageChangeCoroutine;
    private readonly float INTERVAL_BETWEEN_PAGECHANGE_UI=2f;
    private IEnumerator OnpageChangeWithAudio()
    {
        if (FootstepManager.currentFootstepGroupOrder < 11)
        {
            buttonToDeactivate.SetActive(false);
        
            LeanTween.move(storyUIRectTransform, Vector2.zero, 3f)
                .setEase(LeanTweenType.easeInOutBack)
                .setOnComplete(() => LeanTween.delayedCall(3.5f, MoveAwayUI));
        
            UpdateUI(storyUICvsGroup, _storyUITmp, "다음 친구가 나와서\n땅속 친구들을 찾아볼까요?");
            
            yield return GetWaitForSeconds(INTERVAL_BETWEEN_PAGECHANGE_UI);
        
            PlayAudio(uiAudioClips[(int)AudioClips.PageOver]);

            yield return GetWaitForSeconds(1f);
            StopCoroutine(OnpageChangeCoroutine);
        }

  
    }

    [SerializeField] private GameObject buttonToDeactivate;

    private Coroutine _gameOVerCoroutine;

    private IEnumerator OnGameOverWithAudio()
    {
        Debug.Log("종료UI표출");
        buttonToDeactivate.SetActive(false);

        LeanTween.move(storyUIRectTransform, Vector2.zero, 3f)
            .setEase(LeanTweenType.easeInOutBack)
            .setOnComplete(() => LeanTween.delayedCall(1.0f, MoveAwayUI));


        UpdateUI(storyUICvsGroup, _storyUITmp, _lastUIMessage);

        yield return GetWaitForSeconds(3f);
        
        PlayAudio(uiAudioClips[(int)AudioClips.GameOver]);

        yield return GetWaitForSeconds(3f);
        StopCoroutine(_gameOVerCoroutine);

    }

    private void OnGameOver()
    {
        _gameOVerCoroutine = StartCoroutine(OnGameOverWithAudio());
    }

    private Coroutine _gameStartCoroutine;
    private void OnGameStart()
    {

        _gameStartCoroutine = StartCoroutine(OnGameStartCoroutine());


    }

    private IEnumerator OnGameStartCoroutine()
    {
    
        
        LeanTween.move(tutorialUIRectTransform,
                new Vector2(0, tutorialAwayTransfrom.position.y),
                2f)
            .setEase(LeanTweenType.easeInOutBack);
            
        yield return GetWaitForSeconds(3.5f);
    
        buttonToDeactivate.SetActive(false);
        UpdateUI(storyUICvsGroup, _storyUITmp, _firstUIMessage);
        
        yield return GetWaitForSeconds(6.6f);
        buttonToDeactivate.SetActive(true);
        
        yield return GetWaitForSeconds(1f);
        StopCoroutine(_gameStartCoroutine);
    }

    public RectTransform uiAwayPosition;

    private void PlayFollowFootstepAudio()
    {
        uiAudioSource.clip = etcAudioClips[(int)EtcAudioClips.FollowFootsteps];
        uiAudioSource.Play();
    }

    private IEnumerator OnStageStartCoroutine()
    {
        OnStageStart();
        yield return GetWaitForSeconds(1.8f);
        PlayFollowFootstepAudio();
        
        yield return GetWaitForSeconds(3f);
        StopCoroutine(_stageStartCoroutine);

    }
    private void MoveAwayUI()
    {
        LeanTween.move(storyUIRectTransform, new Vector2(storyUIRectTransform.position.x, 
            storyUIRectTransform.position.y + 1000), 2.3f).setEase(LeanTweenType.easeInOutBack);
    }

    private void OnStageStart()
    {
        LeanTween.move(storyUIRectTransform,
            new Vector2(0, tutorialAwayTransfrom.position.y),
            3f).setEase(LeanTweenType.easeInOutBack);

        Debug.Log("UI OnstageStart");
        storyUICvsGroup.DOFade(0, 0.5f);
    }

    private void UpdateUI(CanvasGroup cvsGroup, TMP_Text tmptext, string message)
    {
        ActivateWithFadeInUI(cvsGroup);
        ChangeUIText(tmptext, message);
    }

    private void ActivateWithFadeInUI(CanvasGroup cvsGroup)
    {
        cvsGroup.alpha = 0;
        cvsGroup.DOFade(1, 1);
    }

    private void ChangeUIText(TMP_Text tmptext, string message)
    {
        tmptext.text = message;
    }


    [SerializeField] private float cameraMoveTime;
    private float cameraMoveElapsed;

    public static float INTRO_UI_DELAY = 4.5f;
    public GameObject howToPlayUI;

    private void SetUIIntroUsingUniRx()
    {
        howToPlayUI.SetActive(true);

        if (_uiPlayCoroutine != null) StopCoroutine(_uiPlayCoroutine);

        PlayAudio(uiAudioClips[(int)AudioClips.Story]);
        Debug.Log("Second introduction message.");
    }
}