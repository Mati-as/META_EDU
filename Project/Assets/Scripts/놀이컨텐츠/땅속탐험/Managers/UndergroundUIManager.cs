using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Xml;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

   
    


    [FormerlySerializedAs("gameManager")]
    [Header("References")] //-------------------------------
    [SerializeField]
    private GroundGameController gameController;

     private FootstepManager _footstepManager;
    
    public RectTransform popUpUIRect;
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
    public string _firstUIMessage = " 땅속에는 다양한 동물친구가 살고 있어요!     저를 따라오면서 구경해볼까요?";
    public string _lastUIMessage = "와~ 동물친구들을 모두 찾았어요!";

    [Space(30f)] [Header("Audio")] [Space(10f)] //-------------------------------
    public AudioSource uiAudioSource; // 두개 AudioSource중 하단 AudioSource 입니니다. 위에 것은 Button AudioSource

    public AudioClip[] uiAudioClips = new AudioClip[5];

    public AudioClip[] etcAudioClips = new AudioClip[5];

    
    //Pop창 안의 실제 사진 표출 부분의 Image Component..
    [SerializeField]
    private Image _image; 
    
    public void ChangeImageSource(string resourcePath)
    {
        Sprite newSprite = Resources.Load<Sprite>(resourcePath);
        if (_image != null && newSprite != null)
        {
            _image.sprite = newSprite; // 이미지 소스 변경
        }
        else
        {
            Debug.LogError("Image or Sprite not found!");
        }
    }
    private enum EtcAudioClips
    {
        WhoIsNext, //다음동물친구는 누굴까
        LetsMeetNext, // 다음동물친구를 찾아보자
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
       
        Init();

        
        
        popUpUIRect.localScale = Vector3.zero;
      
        FootstepController.onLastFootstepClicked -= EveryLastFootstepClicked;
        FootstepController.onLastFootstepClicked += EveryLastFootstepClicked;
        popUpUIRectTmp = popUpUIRect.GetComponentInChildren<TMP_Text>();
  

        _uiPlayCoroutine = StartCoroutine(PlayTutorialAudio());

        _storyUIInitialRectPos = storyUIRectTransform;


        Underground_PopUpUI_Button.onPopUpButtonEvent -= OnPopUpUIClicked;
        Underground_PopUpUI_Button.onPopUpButtonEvent += OnPopUpUIClicked;
    }
    
    int soundID;
    private TextAsset xmlAsset;
    private XmlNode soundNode;
    private XmlDocument soundPathXml;
    private void Start()
    {
        _footstepManager=  GameObject.FindWithTag("GameManager").GetComponent<FootstepManager>();

        xmlAsset = Resources.Load<TextAsset>("Common/Data/Path/SoundPathData");
        soundPathXml = new XmlDocument();
        soundPathXml.LoadXml(xmlAsset.text);
        
        soundNode = soundPathXml.SelectSingleNode($"//SoundData[@ID='{soundID}']");
        
        
        gameController.isStartButtonClicked
            .Where(_ => _)
            .Delay(TimeSpan.FromSeconds(INTRO_UI_DELAY))
            .Subscribe(_ => SetUIIntroUsingUniRx())
            .AddTo(this);

        gameController.currentStateRP
            .Where(_currentState => _currentState.GameState == IState.GameStateList.GameStart)
            .Subscribe(_ => OnGameStart())
            .AddTo(this);

        gameController.currentStateRP
            .Where(_currentState => _currentState.GameState == IState.GameStateList.StageStart)
            // .Delay(TimeSpan.FromSeconds(3f))
            .Subscribe(_ => { _stageStartCoroutine = StartCoroutine(OnStageStartCoroutine()); })
            .AddTo(this);

        gameController.isGameFinishedRP
            .Where(value => value)
            .Delay(TimeSpan.FromSeconds(8f))
            .Subscribe(_ => OnGameOver())
            .AddTo(this);

        _footstepManager.finishPageTriggerProperty
            .Where(value => value)
            .Delay(TimeSpan.FromSeconds(3.5f))
            .Subscribe(_ => OnPageChange())
            .AddTo(this);

        _footstepManager.lastElementClickedProperty
            .Where(_ => _)
            .Delay(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ => OnAnimalFind())
            .AddTo(this);
    }
    

    private void OnDestroy()
    {
        Underground_PopUpUI_Button.onPopUpButtonEvent -= OnPopUpUIClicked;
        FootstepController.onLastFootstepClicked -= EveryLastFootstepClicked;
    }
    



    public TMP_Text popUpUIRectTmp;

    public RectTransform popUpButton;
    [SerializeField]
    private Underground_PopUpUI_Button _underground_PopUpUI_Button;

    [SerializeField] private GameObject popUpUIGameObj;
    private Tween _popUpUIScaleTween;

    private void EveryLastFootstepClicked()
    {
        var defaultSize = popUpButton.localScale;
        popUpButton.localScale = Vector2.zero;
#if UNITY_EDITOR
        Debug.Log("동물설명 UI 표출");
#endif
        //FootstepMager에서 클릭시 GetComponent하여 Tmp수정
       //popUpUIRectTmp.text = footstepController.animalByLastFootstep.name;
       popUpUIGameObj.SetActive(true);
       _popUpUIScaleTween = popUpUIRect.DOScale(1, 1.5f)
           .SetEase(Ease.InOutExpo)
           .SetDelay(3f) // 마지막 발판 클릭 후 팝업 UI가 나타나기 까지 Delay하는 시간
           .OnComplete(() =>
           {
               _popUpUIScaleTween = popUpButton.DOScale(_underground_PopUpUI_Button._defaultSize
                                                        *_underground_PopUpUI_Button.scaleUpSize, 1f)
                   .SetEase(Ease.InOutExpo)
                   .SetDelay(3f)
                   .OnComplete(() =>
                   {
                      
                       //콜백 딜레이용 DoVirtual입니다. Duration제외 의미없음
                       DOVirtual.Float(0, 1, 2, value => value++).OnComplete(() =>
                       {
                           _popUpUIScaleTween.Kill();
                           _underground_PopUpUI_Button.DownScale();
                       });

                   });


           });
    }

    /// <summary>
    /// 팝업 클릭시 -> 스케일감소 및 소리재생 및 카메라전환까지 이루어져야합니다. 
    /// </summary>
    private void OnPopUpUIClicked()
    {
      
        
        popUpUIRect.DOScale(0, 1.5f)
            .SetEase(Ease.InOutExpo)
            .SetDelay(0f);
        
        // 단순 delay 설정용 DoVirtual..
        DOVirtual.Float(0, 1, 1.5f, val => val++)
            .OnComplete(() =>
            {
                if (FootstepManager.currentFootstepGroupOrder <= 13)
                {
                    soundNode = soundPathXml.SelectSingleNode(
                        //calculate index..
                        $"//SoundData[@ID='{FootstepManager.currentFootstepGroupOrder * 2}']");
                    string soundPath = soundNode.Attributes["path"].Value;
                   Managers.Sound.Play(SoundManager.Sound.Effect, soundPath);

                    //PlayAudio(etcAudioClips[(int)EtcAudioClips.WhoIsNext]);
                }
            });
      

    }
    
    private void OnAnimalFind()
    {
        //팝업UI에는 버튼밖에 이벤트가 없으므로, 여기서 PopUpUI image 업데이트를 진행합니다. 
        ChangeImageSource("SortedbyGame/땅속탐험/image/" + _footstepManager.currentlyClickedObjectName);
        
        _onAnimalFindAudioCoroutine = StartCoroutine(PlayOnFindAudios());
    }

    private float _waitTimeUntilPopUpAppear = 3.25f;
    private IEnumerator PlayOnFindAudios()
    {
        if (FootstepManager.currentFootstepGroupOrder != 0)
        {
          //  PlayAudio(animalAudioClips[FootstepManager.currentFootstepGroupOrder - 1]);

            soundNode = soundPathXml
                .SelectSingleNode($"//SoundData[@ID='{FootstepManager.currentFootstepGroupOrder * 2 - 1}']");
            string soundPath = soundNode.Attributes["path"].Value;
            Managers.Sound.Play(SoundManager.Sound.Effect, soundPath);
            
        }
          
        
        
        yield return GetWaitForSeconds(_waitTimeUntilPopUpAppear);
        
        //popup_UI 음성 재생파트 
        if (FootstepManager.currentFootstepGroupOrder != 0)
        {
            //  PlayAudio(animalAudioClips[FootstepManager.currentFootstepGroupOrder - 1]);

            soundNode = soundPathXml
                .SelectSingleNode($"//SoundData[@ID='{FootstepManager.currentFootstepGroupOrder + 24}']");
            string soundPath = soundNode.Attributes["path"].Value;
            Managers.Sound.Play(SoundManager.Sound.Effect, soundPath);
        }

        // 마지막 동물인 여우가 아닐 때만...
       


        while (uiAudioSource.isPlaying) yield return null;

        StopCoroutine(_onAnimalFindAudioCoroutine);
    }
    
    

    private IEnumerator PlayTutorialAudio()
    {
        //gameManager.s_soundManager.Play();
        PlayAudio(uiAudioClips[(int)AudioClips.Tutorial1]);

        while (uiAudioSource.isPlaying) yield return null;


        yield return GetWaitForSeconds(1f);

        PlayAudio(uiAudioClips[(int)AudioClips.Tutorial2]);

        yield return GetWaitForSeconds(3f);
    }

    private void Init()
    {
        storyUICvsGroup.alpha = 0;
     
    }

    private Coroutine _stageStartCoroutine;

   




    private void OnPageChange()
    {
        OnpageChangeCoroutine = StartCoroutine(OnpageChangeWithAudio());
    }

    private Coroutine OnpageChangeCoroutine;
    private readonly float INTERVAL_BETWEEN_PAGECHANGE_UI = 2f;

    private IEnumerator OnpageChangeWithAudio()
    {
        if (FootstepManager.currentFootstepGroupOrder < 11)
        {
            buttonToDeactivate.SetActive(false);

            storyUIRectTransform.DOAnchorPos(Vector2.zero,3f)
                .SetEase(Ease.InOutBack)
                .SetDelay(0)
                .OnComplete(() =>  MoveAwayUI());

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
        
        
        yield return GetWaitForSeconds(0.0f);
        
#if UNITY_EDITOR
        Debug.Log("종료UI표출");
#endif
        buttonToDeactivate.SetActive(false);
        storyUIRectTransform.DOAnchorPos(Vector2.zero, 3f)
            .SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                MoveAwayUI(4f);
            });



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
    
        
        yield return GetWaitForSeconds(3.5f);

        buttonToDeactivate.SetActive(false);
        UpdateUI(storyUICvsGroup, _storyUITmp, _firstUIMessage);

        yield return GetWaitForSeconds(6.6f);
        
        buttonToDeactivate.SetActive(true);
        var defaultScale = buttonToDeactivate.transform.localScale;
        buttonToDeactivate.transform.localScale = Vector3.zero; 
        buttonToDeactivate.transform.DOScale(defaultScale, 1f);
        
      

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

    private void MoveAwayUI(float delay =3f)
    {
        storyUIRectTransform
            .DOAnchorPos
            (new Vector2(storyUIRectTransform.anchoredPosition.x, storyUIRectTransform.anchoredPosition.y + 1000),
                2.3f)
            .SetEase(Ease.InOutBack)
            .SetDelay(delay);

    }

    private void OnStageStart()
    {
   
#if UNITY_EDITOR
        Debug.Log("UI OnstageStart");
#endif
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
    

    private void SetUIIntroUsingUniRx()
    {
        // howToPlayUI.SetActive(true);
        
         if (_uiPlayCoroutine != null) StopCoroutine(_uiPlayCoroutine);

        PlayAudio(uiAudioClips[(int)AudioClips.Story]);
        
#if UNITY_EDITOR
        Debug.Log("Second introduction message.");
#endif
    }
}