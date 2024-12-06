using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EasyVer_FishOnWaterUIManager : UI_PopUp
{
    private enum UI_Type
    {
        Timer,
        Ready,
        Start,
        Stop,
        Count,
        ModeSelection,
      //  Tutorial,
        CurrentUserNameInfo,
        CurrentUser,
        RankingParent,
        OnRankingUsers,
        UIHidePosition,
        ScreenDim,
       // Text_Tutorial,
        //Text_CurrentUserRankingText, // 유저등수/ 전체등수
        Slider_Restart,

        //Text_CurrentUserInfoText,
        // Setting_FishManagerOnly,
        // MainVolume,
        // BGMVolume,
        // EffectVolume
    }


    private enum UI_Button
    {
        Btn_StartOnUserInfo,
        Btn_Restart,
        Btn_ShowUserInfo,
        Btn_SinglePlay,
        Btn_MultiPlay,
      //  Btn_Close
        
        // SettingButton,
        // Btn_SettingDisplay,
        // Btn_XmlReset,
        // Btn_SaveCurrentSetting,
        // Btn_ResetSetting
        // Btn_ShowTutorial, // Tutorial -> UserInfo
    }

    private enum RankingUI
    {
        UserIcon, // 유저 아이콘 스프라이트 
        UserNames, //  랭킹 유저 이름
        UserScores, // 랭킹 유저 점수
        CurrentUserName, // 현재 유저이름
        CurrentUserScore // 현재 유저 점수
    }

    private enum RankUserInfo
    {
        Name,
        ScoreFishCaughtCount,
        ScoreRemainTime,
        Sprite,
        Max
    }

    private CanvasGroup _canvasGroup;


    private GameObject[] _uiGameObjects;
    private RectTransform[] _uiRectTransforms;

    private TextMeshProUGUI _fishCountTMP;
    private TextMeshProUGUI _timerTMP;
    private TextMeshProUGUI _timeLimitSliderTMP;
    private TextMeshProUGUI _fishCountSliderTMP;

    private TextMeshProUGUI[] _TMP_usersOnRankScores;
    private TextMeshProUGUI[] _TMP_usersOnRankNames;
    private TextMeshProUGUI[] _TMP_currentUser;


    private RectTransform[] _usersOnRankingRects;

    private Button[] _uiBtns;
    private RectTransform[] _btnRects;

    private Button _restartButton;

    private Button _nextButton;

    //private Button _initialStartButton;
    private Button _startButtonOnUserInfo;

    private Image currentUserChracterImage; // 처음 사용자 정보 인트로 표출 시 
    private Image[] _rankingImage; // 랭킹화면 

    private Image _screenDim;

    private const int USER_ON_RANKING_COUNT = 8;
    private string[][] _TMP_usersOnRankingData;
    private Image[] _usersOnRankingIconSpriteImages;
    private Image _currentUserIconSpriteImage;
    private readonly float _intervalBtwStartAndReady = 2f;
    private bool _isAnimating; // 더블클릭 방지 논리연산자 입니다. 
    private bool _isRestartBtnClickable; // 게임종료 후 바로 다시시작 버튼이 눌리지 않도록 방지합니다.
    private readonly int RESTART_CLIICKABLE_DELAY = 4;
    public static event Action OnStartUIAppear; // 시작 UI가 표출될 때 의 이벤트입니다. 
    public static event Action OnReadyUIAppear; // 준비~! UI 표출 시
    public static event Action OnRestartBtnClicked; // 그만 이후 초기화가 끝난경우
    public static event Action OnResetSettingBtnClicked;
    public static event Action OnModeSelectionUIAppear;

    private EasyVer_FishOnWaterGameManager _gm;
    private bool _isOnFirstRound = true; //초기 버튼 관련 로직
    public Slider _fishSpeedSlider { get; private set; }
    public Slider _timeLimitSlider { get; private set; }
    public Slider _fishGoalCountSlider { get; private set; }

    private float _restartSliderFillAmount;
    private Image _restartSliderImage; // 슬라이더 처럼 동작하지만 실제로는 슬라이더는 아님에 주의합니다.

    public float restartSliderFillAmount
    {
        get => _restartSliderFillAmount;
        private set
        {
            Mathf.Clamp(value, 0, 1);
            _restartSliderFillAmount = value;
            if (value > 0.99f)
            {
                _restartSliderFillAmount = 0;
                OnRestartBtnGaugeFullyFilled();
            }
        }
    }


    private TextMeshProUGUI _TMP_fishSpeed;
    private TextMeshProUGUI _TMP_introUserName;
   // private TextMeshProUGUI _TMP_tutorialUI;
    private TextMeshProUGUI _TMP_currentUserRankingText; // 유저등수/전체등수
    private Vector3[] _defaultanchorPosArray;
    private Vector3[] _defaultSizeArray;

    private WaitForSeconds _wait;
    private WaitForSeconds _waitInterval;
    private WaitForSeconds _waitForReady;
    private float _waitTIme = 4.5f;

    private readonly float ANIM_DURATION_START_AND_READY_STOP = 0.4f;


    // 사운드 세팅, 런쳐와 별도로 구성한 점 주의합니다. ///////////////////////////////////////
    private Slider[] _sliders = new Slider[(int)SoundManager.Sound.Max];

    private Button _soundCloseBtn;


    private void SetSoundUI()
    {

        for (var i = 0; i < (int)SoundManager.Sound.Max - 1; i++) // 나레이션 제외 
        {
            _sliders[i].maxValue = Managers.soundManager.VOLUME_MAX[i];
            _sliders[i].value = Managers.soundManager.volumes[i];
        }


        // default Volume값은 SoundManager에서 관리하며, 초기화 이후, UI Slider가 이를 참조하여 표출하도록 합니다.
        // default Value는 시연 테스트에 결과에 따라 수정가능합니다. 
        _sliders[(int)SoundManager.Sound.Main].onValueChanged.AddListener(_ =>
        {
            Managers.soundManager.volumes[(int)SoundManager.Sound.Main] = _sliders[(int)SoundManager.Sound.Main].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Main].volume =
                Managers.soundManager.volumes[(int)SoundManager.Sound.Main];


            Managers.soundManager.volumes[(int)SoundManager.Sound.Bgm] = _sliders[(int)SoundManager.Sound.Bgm].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _sliders[(int)SoundManager.Sound.Bgm].value);

            Managers.soundManager.volumes[(int)SoundManager.Sound.Effect] =
                _sliders[(int)SoundManager.Sound.Effect].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _sliders[(int)SoundManager.Sound.Effect].value);

            // Managers.Sound.volumes[(int)SoundManager.Sound.Narration] = _sliders[(int)SoundManager.Sound.Narration].value;
            // Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].volume =
            // 	Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Narration],
            // 		Managers.Sound.volumes[(int)SoundManager.Sound.Main] * _sliders[(int)SoundManager.Sound.Narration].value);
        });
        _sliders[(int)SoundManager.Sound.Bgm].onValueChanged.AddListener(_ =>
        {
            Managers.soundManager.volumes[(int)SoundManager.Sound.Bgm] = _sliders[(int)SoundManager.Sound.Bgm].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _sliders[(int)SoundManager.Sound.Bgm].value);
        });

        _sliders[(int)SoundManager.Sound.Effect].onValueChanged.AddListener(_ =>
        {
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/TestSound/Test_Effect");

            Managers.soundManager.volumes[(int)SoundManager.Sound.Effect] =
                _sliders[(int)SoundManager.Sound.Effect].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _sliders[(int)SoundManager.Sound.Effect].value);
        });
    }

    public override bool Init()
    {
        
        
        UI_Scene_StartBtn.onBtnShut -= OnReadyAndStart;
        UI_Scene_StartBtn.onBtnShut += OnReadyAndStart;
        
        EasyVer_FishOnWaterGameManager.OnRoundFinished -= ShowStopUI;
        EasyVer_FishOnWaterGameManager.OnRoundFinished += ShowStopUI;

        
        // UFishOnWaterBaseGameManager.OnReady -= OnReadyAndStart;
        // UFishOnWaterBaseGameManager.OnReady += OnReadyAndStart;

        // UI_Scene_StartBtn.onBtnShut -= OnStartBtnClicked;
        // UI_Scene_StartBtn.onBtnShut += OnStartBtnClicked;
        
  
        
        
        
        _gm = GameObject.FindWithTag("GameManager").GetComponent<EasyVer_FishOnWaterGameManager>();
        
        BindObject(typeof(UI_Type));
        BindButton(typeof(UI_Button));

        InitializeUIElements();
        InitializeRankingElements();

        ShowTutorial();

        return true;
    }

    private void InitializeUIElements()
    {
        var uiElementCount = Enum.GetValues(typeof(UI_Type)).Length;
        _uiGameObjects = new GameObject[uiElementCount];
        _uiRectTransforms = new RectTransform[uiElementCount];
        _defaultanchorPosArray = new Vector3[uiElementCount];
        _defaultSizeArray = new Vector3[uiElementCount];

        for (var i = 0; i < uiElementCount; i++)
        {
            _uiGameObjects[i] = GetObject(i);
            _uiRectTransforms[i] = _uiGameObjects[i].GetComponent<RectTransform>();
            _defaultSizeArray[i] = _uiRectTransforms[i].localScale;
            _defaultanchorPosArray[i] = _uiRectTransforms[i].anchoredPosition;
            _uiGameObjects[i].SetActive(false);
        }

       // _TMP_tutorialUI = _uiGameObjects[(int)UI_Type.Text_Tutorial].GetComponent<TextMeshProUGUI>();

        var btnElementCount = Enum.GetValues(typeof(UI_Button)).Length;
        _uiBtns = new Button[btnElementCount];
        _btnRects = new RectTransform[btnElementCount];

        //GetButton((int)UI_Button.Btn_InitialStart).gameObject.BindEvent(() => ));
        //GetButton((int)UI_Button.Btn_ShowTutorial).gameObject.BindEvent(() =>ShowTuorial());

        for (var i = 0; i < btnElementCount; i++)
        {
            _uiBtns[i] = GetButton(i);
            _btnRects[i] = _uiBtns[i].transform.GetComponent<RectTransform>();
        }

        //_uiBtns[(int)UI_Button.Btn_InitialStart].gameObject.BindEvent(() => OnStartBtnOnUserInfoClicked());

        _uiBtns[(int)UI_Button.Btn_StartOnUserInfo].gameObject.BindEvent(OnStartBtnOnUserInfoClicked);
        _uiBtns[(int)UI_Button.Btn_ShowUserInfo].gameObject.BindEvent(ShowUserInfo);
        _uiBtns[(int)UI_Button.Btn_Restart].gameObject.BindEvent(OnRestartBtnPerCLicked);
        _uiBtns[(int)UI_Button.Btn_SinglePlay].gameObject.BindEvent(OnSinglePlayBtnClicked);
        _uiBtns[(int)UI_Button.Btn_MultiPlay].gameObject.BindEvent(OnMultiPlayBtnClicked);
        
        //_uiBtns[(int)UI_Button.Btn_Close].gameObject.BindEvent(OnSoundSettingCloseBtnClicked);
        // _uiBtns[(int)UI_Button.SettingButton].gameObject.BindEvent(OnSoundSettingBtnClicked);
        // _uiBtns[(int)UI_Button.Btn_XmlReset].gameObject.BindEvent(OnXmlResetBtnClicked);
        // _uiBtns[(int)UI_Button.Btn_ResetSetting].gameObject.BindEvent(OnResetSetting);
        // _uiBtns[(int)UI_Button.Btn_SaveCurrentSetting].gameObject.BindEvent(SaveCurrentSetting);


        
        _uiGameObjects[(int)UI_Type.ScreenDim].SetActive(true);
        _uiGameObjects[(int)UI_Type.CurrentUser].transform.Find("CurrentUserIcon");
        _uiGameObjects[(int)UI_Type.Timer].SetActive(false);
        _uiRectTransforms[(int)UI_Type.Timer].localScale = Vector3.zero;

        _uiGameObjects[(int)UI_Type.Count].SetActive(false);
        _uiRectTransforms[(int)UI_Type.Count].localScale = _defaultSizeArray[(int)UI_Type.Count];

        _timerTMP = _uiGameObjects[(int)UI_Type.Timer].GetComponentInChildren<TextMeshProUGUI>();
        _fishCountTMP = _uiGameObjects[(int)UI_Type.Count].GetComponent<TextMeshProUGUI>();

        var userNameUIParentTransform = _uiGameObjects[(int)UI_Type.CurrentUserNameInfo].transform;

        currentUserChracterImage = userNameUIParentTransform.Find("CharacterImage").GetComponent<Image>();
        _TMP_introUserName = userNameUIParentTransform.Find("Text_Name")
            .GetComponent<TextMeshProUGUI>();

        var currentUser = _uiGameObjects[(int)UI_Type.CurrentUser].transform;
        _currentUserIconSpriteImage = currentUser.Find("CharacterFrame").GetChild(0).GetComponent<Image>();

        _uiGameObjects[(int)UI_Type.ModeSelection].SetActive(true);
        _uiGameObjects[(int)UI_Type.ModeSelection].transform.localScale = _defaultSizeArray[(int)UI_Type.ModeSelection];
        _uiGameObjects[(int)UI_Type.ModeSelection].transform.localScale = Vector3.zero;
        //
        _uiGameObjects[(int)UI_Type.Slider_Restart].SetActive(true);
        _restartSliderImage = _uiGameObjects[(int)UI_Type.Slider_Restart].GetComponent<Image>();
        _restartSliderImage.fillAmount = 0;

       // _TMP_currentUserRankingText =
         //   _uiGameObjects[(int)UI_Type.Text_CurrentUserRankingText].GetComponent<TextMeshProUGUI>();
    }



    private void OnStartBtnClicked()
    {
       // ShowModeSelectionMode();
    }

    private void ShowUserInfo()
    {
        if (!_isBtnClickable || _isAnimating) return;
        //중복클릭방지
        _isAnimating = true;
        _isBtnClickable = false;
        DOVirtual.Float(0, 0, 3f, _ => { _isBtnClickable = false; })
            .OnComplete(() => { _isBtnClickable = true; });

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);
        StartCoroutine(ShowUserInfoCo());
    }

    private IEnumerator ShowUserInfoCo()
    {
       // if (!_uiGameObjects[(int)UI_Type.Tutorial].activeSelf && _isAnimating) yield break;


        yield return _waitInterval;
        currentUserChracterImage.sprite = _gm._characterImageMap[_gm.currentImageChar];

        // yield return _uiRectTransforms[(int)UI_Type.Tutorial]
        //     .DOAnchorPos(_uiRectTransforms[(int)UI_Type.UIHidePosition].anchoredPosition, 0.8f)
        //     .SetEase(Ease.InOutSine)
        //     .WaitForCompletion();

        //
        // _uiGameObjects[(int)UI_Type.Tutorial].SetActive(false);
        // _btnRects[(int)UI_Button.Btn_ShowUserInfo].gameObject.SetActive(false);


        _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo].anchoredPosition =
            _defaultanchorPosArray[(int)UI_Type.CurrentUserNameInfo];
        _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo].localScale = Vector3.zero;
        _uiGameObjects[(int)UI_Type.CurrentUserNameInfo].SetActive(true);

        DOVirtual.Float(0, 1, 0.5f,
            value => { _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo].localScale = Vector3.one * value; });

        _TMP_introUserName.text = _gm.currentUserName;
        _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo].anchoredPosition =
            _uiRectTransforms[(int)UI_Type.UIHidePosition].position;

        _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo]
            .DOAnchorPos(_defaultanchorPosArray[(int)UI_Type.CurrentUserNameInfo], 0.8f).SetEase(Ease.InOutSine);

        _uiBtns[(int)UI_Button.Btn_StartOnUserInfo].gameObject.SetActive(true);
        _btnRects[(int)UI_Button.Btn_StartOnUserInfo].localScale = Vector3.zero;
        DOVirtual.Float(0, 1, 0.45f,
                scale => { _btnRects[(int)UI_Button.Btn_StartOnUserInfo].localScale = Vector3.one * scale; })
            .SetDelay(1.35f);

        _isAnimating = false;
    }


    private void OnStartButtonClicked()
    {
        if (!_isBtnClickable || _isAnimating) return;


        _screenDim = _uiGameObjects[(int)UI_Type.ScreenDim].GetComponent<Image>();

        if (_screenDim == null)
            Debug.LogError("screenDim is Null");
        else
            _screenDim.DOFade(0, 0.55f);
        _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo]
            .DOAnchorPos(_uiRectTransforms[(int)UI_Type.UIHidePosition].anchoredPosition, 0.5f)
            .SetEase(Ease.InOutSine);
    }

    private void InitializeRankingElements()
    {
        _TMP_usersOnRankingData = new string[USER_ON_RANKING_COUNT][];
        for (var i = 0; i < _TMP_usersOnRankingData.Length; i++)
            _TMP_usersOnRankingData[i] = new string[(int)RankUserInfo.Max];

        _TMP_usersOnRankNames = new TextMeshProUGUI[USER_ON_RANKING_COUNT];
        _TMP_usersOnRankScores = new TextMeshProUGUI[USER_ON_RANKING_COUNT];
        _usersOnRankingIconSpriteImages = new Image[USER_ON_RANKING_COUNT];

        var usersOnRankingObj = _uiGameObjects[(int)UI_Type.OnRankingUsers];
        _usersOnRankingRects = new RectTransform[USER_ON_RANKING_COUNT];
        for (var i = 0; i < USER_ON_RANKING_COUNT; i++)
        {
            var userTransform = usersOnRankingObj.transform.GetChild(i);
            _usersOnRankingRects[i] = userTransform.GetComponent<RectTransform>();
            _usersOnRankingIconSpriteImages[i] = userTransform.Find("CharacterFrame").GetChild(0).GetComponent<Image>();
            _TMP_usersOnRankNames[i] = userTransform.Find("Text_UserName")?.GetComponent<TextMeshProUGUI>();
            _TMP_usersOnRankScores[i] = userTransform.Find("Text_Score_Value")?.GetComponent<TextMeshProUGUI>();
        }

        var currentUser = _uiGameObjects[(int)UI_Type.CurrentUser];
        var currentUserTransform = currentUser.transform;
        _TMP_currentUser = new TextMeshProUGUI[(int)RankUserInfo.Max];
        _TMP_currentUser[(int)RankUserInfo.Name] = currentUserTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _TMP_currentUser[(int)RankUserInfo.ScoreFishCaughtCount] =
            currentUserTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
        _TMP_currentUser[(int)RankUserInfo.ScoreRemainTime] =
            currentUserTransform.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        //UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
       // UFishOnWaterBaseGameManager.OnReady -= OnReadyAndStart;
        OriginalHardVer_FishOnWaterBaseGameManager.OnRoundFinished -= ShowStopUI;
        UI_Scene_StartBtn.onBtnShut -= OnReadyAndStart;
    }

    private void Update()
    {
        if (!_gm.isOnReInit && _uiGameObjects[(int)UI_Type.Timer].activeSelf)
        {
            _timerTMP.text = _gm.remainTime.ToString("00") + "초";
            _fishCountTMP.text =
                _gm.FishCaughtCount == _gm.FISH_POOL_COUNT ? "모든 물고기를 다 잡았어요!" : _gm.FishCaughtCount + " 마리";
        }


        _timeSinceLastTouch = Mathf.Clamp(_timeSinceLastTouch, 0, 2) + Time.deltaTime;

        if (_timeSinceLastTouch > 1f) _restartSliderImage.fillAmount = 0;
    }

    //-------------------------------------------------------------

    public void OnReadyAndStart()
    {
        ShowStartUI();
        OnStartButtonClicked();
    }


    private void ParseXML()
    {
        XmlNode root = _gm.xmlDoc_Temp.DocumentElement;
        var nodes = root.SelectNodes("StringData");

        var userScores = new List<(string mode, string username, string score, string iconNumber)>();

        foreach (XmlNode node in nodes)
        {
            var username = node.Attributes["username"].Value;
            var score = node.Attributes["score"].Value;
            var icon = node.Attributes["iconnumber"].Value;

            var mode = node.Attributes["mode"].Value;

            if (mode == _gm.currentMode.ToString()) userScores.Add((mode, username, score, icon));
        }


        if (_gm.currentMode == (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.MultiPlay)
        {
            // 첫 번째 숫자(A)를 기준으로 내림차순 정렬, A 값이 같으면 두 번째 숫자(B)를 기준으로 정렬
            userScores = userScores.OrderByDescending(x => int.Parse(x.score)).ToList();


            for (var i = 0; i < USER_ON_RANKING_COUNT && i < userScores.Count; i++)
            {
                _TMP_usersOnRankingData[i][(int)RankUserInfo.Name] = userScores[i].Item2;
                _TMP_usersOnRankingData[i][(int)RankUserInfo.ScoreFishCaughtCount] = userScores[i].Item3;


                _TMP_usersOnRankNames[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.Name];
                _TMP_usersOnRankScores[i].text =
                    _TMP_usersOnRankingData[i][(int)RankUserInfo.ScoreFishCaughtCount] + " 마리";
                _usersOnRankingIconSpriteImages[i].sprite = _gm._characterImageMap[userScores[i].Item4[0]];

                // 현재 유저가 랭킹에 오르는 것에 성공한 경우 애니메이션 --------------------
                if (_TMP_usersOnRankNames[i].text == _gm.currentUserName &&
                    _TMP_usersOnRankScores[i].text == _gm.fishCaughtCount + " 마리")
                {
#if UNITY_EDITOR
                    Debug.Log($"On Ranking! {i} ");
# endif
                    var seq = DOTween.Sequence();
                    seq.Append(_usersOnRankingRects[i].DOScale(1.06f, 0.1f).SetEase(Ease.InOutSine));
                    seq.Append(_usersOnRankingRects[i].DOScale(0.94f, 0.1f).SetEase(Ease.InOutSine));
                    seq.SetDelay(0.2f);
                    seq.SetLoops(20);

                    seq.Append(_usersOnRankingRects[i].DOScale(1f, 0.5f));
                }
            }
        }
        else if (_gm.currentMode == (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.SinglePlay)
        {
            // 첫 번째 숫자(A)를 기준으로 내림차순 정렬, A 값이 같으면 두 번째 숫자(B)를 기준으로 정렬
            userScores = userScores.OrderByDescending(x =>
                {
                    var splitScore = x.score.Split('/');
                    return int.Parse(splitScore[0]); // A 값을 기준으로 정렬
                })
                .ThenByDescending(x =>
                {
                    var splitScore = x.score.Split('/');
                    return float.Parse(splitScore[1]); // B 값을 기준으로 정렬
                })
                .ToList();


            for (var i = 0; i < USER_ON_RANKING_COUNT && i < userScores.Count; i++)
            {
                _TMP_usersOnRankingData[i][(int)RankUserInfo.Name] = userScores[i].Item2;
                _TMP_usersOnRankingData[i][(int)RankUserInfo.ScoreFishCaughtCount] = userScores[i].Item3;


                _TMP_usersOnRankNames[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.Name];
                _TMP_usersOnRankScores[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.ScoreFishCaughtCount];
                _usersOnRankingIconSpriteImages[i].sprite = _gm._characterImageMap[userScores[i].Item4[0]];

                // 현재 유저가 랭킹에 오르는 것에 성공한 경우 애니메이션 --------------------
                if (_TMP_usersOnRankNames[i].text == _gm.currentUserName &&
                    _TMP_usersOnRankScores[i].text == _gm.currentUserScore)
                {
#if UNITY_EDITOR
                    Debug.Log($"On Ranking! {i} ");
# endif
                    var seq = DOTween.Sequence();
                    seq.Append(_usersOnRankingRects[i].DOScale(1.07f, 0.1f).SetEase(Ease.InOutSine));
                    seq.Append(_usersOnRankingRects[i].DOScale(0.93f, 0.1f).SetEase(Ease.InOutSine));
                    seq.SetDelay(0.2f);
                    seq.SetLoops(20);

                    seq.Append(_usersOnRankingRects[i].DOScale(1f, 0.5f));
                }
            }
        }


        var currentUserRank = -1;
        if (_gm.currentMode == (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.MultiPlay)
        {
            for (var i = 0; i < userScores.Count; i++)
                if (userScores[i].username == _gm.currentUserName &&
                    userScores[i].score == _gm.fishCaughtCount.ToString())
                {
                    currentUserRank = i + 1; // 등수는 1부터 시작하므로 1을 더함
                    break;
                }
        }
        else //U_FishOnWater_GameManager.PlayMode.SinglePlay
        {
            for (var i = 0; i < userScores.Count; i++)
                if (userScores[i].username == _gm.currentUserName && userScores[i].score == _gm.currentUserScore)
                {
                    currentUserRank = i + 1; // 등수는 1부터 시작하므로 1을 더함
                    break;
                }
        }


        if (currentUserRank == -1)
        {
#if UNITY_EDITOR
            Debug.Log("There's no matching score with Current User's. ");
# endif
            currentUserRank = userScores.Count;
        }


        _TMP_currentUserRankingText.gameObject.SetActive(true);
        _TMP_currentUserRankingText.text = $"{currentUserRank}등 / {userScores.Count}명";
    }


    private bool _isBtnClickable = true; // 짧은시간 내 중복클릭방지 


    private void ShowModeSelectionMode()
    {
        StartCoroutine(ShowModeSelectionModeCo());
    }

    private IEnumerator ShowModeSelectionModeCo()
    {
        _isBtnClickable = false;
        OnModeSelectionUIAppear?.Invoke();
        
        DOVirtual.Float(0, 0, 2.5f, _ => { }).OnComplete(() => { _isBtnClickable = true; });

        _uiRectTransforms[(int)UI_Type.ModeSelection].anchoredPosition =
            _defaultanchorPosArray[(int)UI_Type.ModeSelection];
        _uiGameObjects[(int)UI_Type.ModeSelection].SetActive(true);
        _uiRectTransforms[(int)UI_Type.ModeSelection].localScale = Vector3.zero;

        yield return DOVirtual.Float(0, 1, ANIM_DURATION_START_AND_READY_STOP,
                scale => { _uiRectTransforms[(int)UI_Type.ModeSelection].localScale = Vector3.one * scale; })
            .SetDelay(0.5f);
    }

    private void OnSinglePlayBtnClicked()
    {
        if (!_isBtnClickable) return;
        _isBtnClickable = false;

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);

        DOVirtual.Float(1, 0, ANIM_DURATION_START_AND_READY_STOP,
            scale => { _uiRectTransforms[(int)UI_Type.ModeSelection].localScale = Vector3.one * scale; });

        _gm.currentMode = (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.SinglePlay;
        ShowTutorial();
    }

    private void OnMultiPlayBtnClicked()
    {
        if (!_isBtnClickable) return;
        _isBtnClickable = false;
        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);


        _gm.currentMode = (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.MultiPlay;

        DOVirtual.Float(1, 0, ANIM_DURATION_START_AND_READY_STOP,
            scale => { _uiRectTransforms[(int)UI_Type.ModeSelection].localScale = Vector3.one * scale; });
        ShowTutorial();
    }

    private void ShowStartUI()
    {
        StartCoroutine(ShowReadyAndStartUICoroutine());
    }

    private void ShowStopUI()
    {
        StartCoroutine(ShowStopUICo());
    }


    private IEnumerator ShowReadyAndStartUICoroutine()
    {
        //ready와 start사이 시간간격
        yield return DOVirtual.Float(0, 0, 0.5f, _ => { }).WaitForCompletion();

        _uiRectTransforms[(int)UI_Type.Count].localScale = Vector3.zero;


        _uiGameObjects[(int)UI_Type.Ready].gameObject.SetActive(true);
        _uiGameObjects[(int)UI_Type.Timer].SetActive(true);
        _uiGameObjects[(int)UI_Type.Count].SetActive(true);
        yield return DOVirtual.Float(0, 1, ANIM_DURATION_START_AND_READY_STOP,
            scale =>
            {
                _uiRectTransforms[(int)UI_Type.Count].localScale = Vector3.one * scale;
                _uiRectTransforms[(int)UI_Type.Timer].localScale = Vector3.one * scale;
                _uiRectTransforms[(int)UI_Type.Ready].localScale = Vector3.one * scale;
            }).OnStart(
            () =>
            {
                OnReadyUIAppear?.Invoke();
                _fishCountTMP.enabled = true;
                _timerTMP.enabled = true;
                _fishCountTMP.text = _gm.FishCaughtCount + " 마리";
                _timerTMP.text = _gm.remainTime.ToString("F1") + "초";
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Ready", 0.8f);
            }).WaitForCompletion();

        yield return _waitInterval;
        yield return DOVirtual.Float(0, 0, _intervalBtwStartAndReady, _ => { }).WaitForCompletion();


        yield return DOVirtual.Float(1, 0, ANIM_DURATION_START_AND_READY_STOP,
                scale => { _uiRectTransforms[(int)UI_Type.Ready].localScale = Vector3.one * scale; })
            .WaitForCompletion();


        yield return DOVirtual.Float(0, 0, 0.25f, _ => { }).WaitForCompletion();
        _uiRectTransforms[(int)UI_Type.Start].gameObject.SetActive(true);

        yield return DOVirtual.Float(0, 1, ANIM_DURATION_START_AND_READY_STOP,
            scale => { _uiRectTransforms[(int)UI_Type.Start].localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Start", 0.85f);
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle", 0.7f);
                OnStartUIAppear?.Invoke();
#if UNITY_EDITOR
                Debug.Log("UI Invoke");
#endif
            }).WaitForCompletion();

        yield return DOVirtual.Float(1, 0, ANIM_DURATION_START_AND_READY_STOP + 0.1F,
                scale => { _uiRectTransforms[(int)UI_Type.Start].localScale = Vector3.one * scale; })
            .WaitForCompletion();
    }

    private WaitForSeconds _waitIntervalBeforeRestart =  new WaitForSeconds(5f);
    private IEnumerator ShowStopUICo()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.3f);

        _fishCountTMP.text = _gm.FishCaughtCount >= _gm.fishCountGoal ? "물고기를 전부 잡았어요!" : "시간이 다 지났어요!";


        yield return _waitInterval;
        _uiRectTransforms[(int)UI_Type.Stop].gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 0.45f,
            scale => { _uiRectTransforms[(int)UI_Type.Stop].localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Stop", 0.8f);
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle", 0.4f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1,
                scale => { _uiRectTransforms[(int)UI_Type.Stop].localScale = Vector3.one * scale; })
            .WaitForCompletion();
        //OnUIFinished?.Invoke();

        _fishCountTMP.text = _gm.FishCaughtCount >= _gm.fishCountGoal
            ? "물고기를 전부 잡았어요!"
            : $"물고기를 {_gm.FishCaughtCount}마리 잡았어요";


        _gm.FishCaughtCount = 0;
        yield return _waitIntervalBeforeRestart;
        ShowStartUI();

        //ShowRankingPanel();
        // ShowRankingPanel();
    }

    private void LoadRankingInfo()
    {
        _TMP_currentUser[(int)RankUserInfo.Name].text = _gm.currentUserName;

        // _TMP_currentUser[(int)RankUserInfo.ScoreSub].text =
        //     $"{_gm.FishCaughtCount} 마리 / {_gm.remainTime:F2} 초 ";
        var splitScore = _gm.currentUserScore.Split('/');


        if (_gm.currentMode == (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.SinglePlay)
        {
            _TMP_currentUser[(int)RankUserInfo.ScoreFishCaughtCount].text = $"{splitScore[0]}마리 / ";
            _TMP_currentUser[(int)RankUserInfo.ScoreRemainTime].gameObject.SetActive(true);
            _TMP_currentUser[(int)RankUserInfo.ScoreRemainTime].text = $"{splitScore[1]}초";
        }
        else
        {
            _TMP_currentUser[(int)RankUserInfo.ScoreFishCaughtCount].text = $"{splitScore[0]}마리";
            _TMP_currentUser[(int)RankUserInfo.ScoreRemainTime].gameObject.SetActive(false);
            _TMP_currentUser[(int)RankUserInfo.ScoreRemainTime].text = string.Empty;
        }


        _currentUserIconSpriteImage.sprite = _gm._characterImageMap[_gm.currentImageChar];
        ParseXML();

        StartCoroutine(PlayReplayTextAnimCo());
    }

    private IEnumerator PlayReplayTextAnimCo()
    {
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();

        var seq = DOTween.Sequence();
        var rect = _btnRects[(int)UI_Button.Btn_Restart].transform.GetChild(1);
        seq.Append(rect.DOScale(1.02f, 0.35f).SetEase(Ease.InOutSine));
        seq.Append(rect.DOScale(0.98f, 0.35f).SetEase(Ease.InOutSine));
        seq.SetLoops(30);
        seq.SetDelay(0.3f);
    }

    private IEnumerator ShowRankingPanelCo()
    {
        //btn
        _isAnimating = true;
        _isBtnClickable = false;
        DOVirtual.Float(0, 0, 2.5f, _ => { }).OnComplete(() => { _isBtnClickable = true; });
        //delay
        yield return DOVirtual.Float(0, 0, 1.5f, _ => { }).WaitForCompletion();

        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.3f);

        _fishCountTMP.enabled = false;
        _timerTMP.enabled = false;

        LoadRankingInfo();
        yield return _waitInterval;

        _uiGameObjects[(int)UI_Type.RankingParent].SetActive(true);
        _uiGameObjects[(int)UI_Type.CurrentUser].SetActive(true);
        _uiGameObjects[(int)UI_Type.OnRankingUsers].SetActive(true);

        _uiBtns[(int)UI_Button.Btn_Restart].transform.gameObject.SetActive(true);
        yield return
            DOVirtual.Float(0, 1, 0.45f, scale
                =>
            {
                _uiRectTransforms[(int)UI_Type.RankingParent].localScale = Vector3.one * scale;
            });

        yield return DOVirtual.Float(0, 0, RESTART_CLIICKABLE_DELAY, _ => { }).WaitForCompletion();
        _isRestartBtnClickable = true;

        DOVirtual.Float(0, 0, 2.5f, _ => { _isBtnClickable = true; });

        _isAnimating = false;
    }

    private void ShowRankingPanel()
    {
        StartCoroutine(ShowRankingPanelCo());
    }


    private void OnRestartButtonClicked()
    {
        _isRestartBtnBeingClicked = true;
    }

    private readonly float _guageIncreaseSensitiviy = 0.021f;
    private bool _isRestartBtnBeingClicked;

    private IEnumerator OnRestartBtnClickedCo()
    {
        if (_isRestartBtnBeingClicked) yield break;
        _isRestartBtnBeingClicked = true;

        if (_timeSinceLastTouch > 0.25f)
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);

        _timeSinceLastTouch = 0;

        restartSliderFillAmount = _restartSliderImage.fillAmount;
        yield return DOVirtual.Float(0, 0, _guageIncreaseSensitiviy, _ => { _restartSliderImage.fillAmount += 0.028f; })
            .WaitForCompletion();
        _isRestartBtnBeingClicked = false;
    }


    private float _timeSinceLastTouch;
    private Coroutine _fillCoroutine;


    private void OnRestartBtnPerCLicked()
    {
        StartCoroutine(OnRestartBtnClickedCo());
    }

    public void OnRestartBtnGaugeFullyFilled()
    {
        if (_isAnimating)
        {
#if UNITY_EDITOR
            Debug.Log("This UI has already activated");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Restarting------------------------------------------------------------");
#endif
        _isAnimating = true;
        _isRestartBtnClickable = false;
        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);
        
        ShowTutorial();
        //StartCoroutine(OnRestartAndShowSelectionMode());
    }

    private void OnStartBtnOnUserInfoClicked()
    {
        if (!_isBtnClickable || _isAnimating) return;
        _isAnimating = true;

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);
        OnRestartBtnClicked?.Invoke();

        _screenDim = _uiGameObjects[(int)UI_Type.ScreenDim].GetComponent<Image>();
        _screenDim?.DOFade(0, 0.55f);
        _uiRectTransforms[(int)UI_Type.CurrentUserNameInfo]
            .DOAnchorPos(_uiRectTransforms[(int)UI_Type.UIHidePosition].anchoredPosition, 0.5f)
            .SetEase(Ease.InOutSine);
        DOVirtual.Float(1, 0, 0.35f, scale
            =>
        {
            _btnRects[(int)UI_Button.Btn_StartOnUserInfo].localScale = Vector3.one * scale;
        }).OnComplete(() => { _isAnimating = false; });
    }


    private string _currentModeInKorean;

    private void ShowTutorial()
    {
        
      //  GetObject(((int)UI_Type.Tutorial)).gameObject.SetActive(true);
#if UNITY_EDITOR
        Debug.Log($"CurrentMode: {(OriginalHardVer_FishOnWaterBaseGameManager.PlayMode)_gm.currentMode}");
#endif
        _currentModeInKorean = _gm.currentMode == (int)OriginalHardVer_FishOnWaterBaseGameManager.PlayMode.SinglePlay
            ? _gm.SINGLE_PLAY_IN_KOREAN
            : _gm.MULTI_PLAY_IN_KOREAN;
     //   _TMP_tutorialUI.text = $"{_currentModeInKorean} - 놀이방법";
       // _TMP_tutorialUI.gameObject.SetActive(true);

        _isBtnClickable = false;
        DOVirtual.Float(0, 0, 2.5f, _ => { _isBtnClickable = false; }).OnComplete(() => { _isBtnClickable = true; });

        _timerTMP.text = _gm.timeLimit.ToString("00") + "초";

        // _uiRectTransforms[(int)UI_Type.Tutorial].anchoredPosition = _defaultanchorPosArray[(int)UI_Type.Tutorial];
        // _uiGameObjects[(int)UI_Type.Tutorial].SetActive(true);
        // _uiRectTransforms[(int)UI_Type.Tutorial].localScale = Vector3.zero;
        // DOVirtual.Float(0, 1, 0.5f,
        //     value => { _uiRectTransforms[(int)UI_Type.Tutorial].localScale = Vector3.one * value; });

        _btnRects[(int)UI_Button.Btn_ShowUserInfo].gameObject.SetActive(true);
        _btnRects[(int)UI_Button.Btn_ShowUserInfo].localScale = Vector3.zero;
        DOVirtual.Float(0, 1, 0.5f,
            value => { _btnRects[(int)UI_Button.Btn_ShowUserInfo].localScale = Vector3.one * value; }).SetDelay(1.5f);
    }


    private IEnumerator OnRestartAndShowSelectionMode()
    {
        yield return _waitInterval;

#if UNITY_EDITOR
        Debug.Log("Restart!");
#endif

        _gm.SetUserInfo();
        _TMP_introUserName.text = _gm.currentUserName;


        yield return DOVirtual.Float(1, 0, 0.45f,
                scale => { _uiRectTransforms[(int)UI_Type.RankingParent].localScale = Vector3.one * scale; })
            .WaitForCompletion();

        if (_screenDim == null)
            Debug.LogError("screenDim is Null");
        else
            _screenDim.DOFade(0.55f, 0.5f);
        _uiGameObjects[(int)UI_Type.RankingParent].SetActive(false);
        ShowModeSelectionMode();
        _isAnimating = false;
    }


    public static event Action OnResetXML;
    private bool _isConfirmed;
    private bool _isXmlResetting;
    private void OnXmlResetBtnClicked()
    {
       
            if (!_isConfirmed)
            {
      

                // set a delay to prevent double-clicks and allow confirmation
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    _isConfirmed = true;
                });
                return;
            }

            if (_isXmlResetting) 
            {
                return;
            }

            _isXmlResetting = true;

            // reset the XML and update the UI after a delay
            DOVirtual.DelayedCall(2f, () =>
            {
                _isXmlResetting = false;
                _isConfirmed = false;
            });

#if UNITY_EDITOR
            Debug.Log("Reset XML.");
#endif
            OnResetXML?.Invoke();
           
        
        

    }
    
    /// <summary>
    ///  사운드는 현재 저장되지않음
    /// </summary>
//     private void InitializeGameSettingByXML()
//     {
//         XmlNode root = _gm.xmlDoc_Setting.DocumentElement;
//         var nodes = root.SelectNodes("UI_Fish_SettingData");
//        
//         foreach (XmlNode node in nodes)
//         {
//             var mainvolume = node.Attributes["mainvolume"].Value;
//             var bgmvol = node.Attributes["bgmvol"].Value;
//             var effectvol = node.Attributes["effectvol"].Value;
//             
//             var timelimit = node.Attributes["timelimit"].Value;
//             var fishspeed = node.Attributes["fishspeed"].Value;
//             var fishgoalcount = node.Attributes["fishgoalcount"].Value;
//
//             if (float.TryParse(mainvolume, out var mainVolResult))
//             {
//                 _sliders[(int)SoundManager.Sound.Main].value = mainVolResult;
//             }
//             
//             if (float.TryParse(bgmvol, out var bgmvolToFloat))
//             {
//                 _sliders[(int)SoundManager.Sound.Bgm].value = bgmvolToFloat;
//             }
//             if (float.TryParse(effectvol, out var effectvolvolToFloat))
//             {
//                 _sliders[(int)SoundManager.Sound.Effect].value = effectvolvolToFloat;
//             }
//             
//             
//             if (float.TryParse(timelimit, out var playtimeToFloat))
//             {
//                 _timeLimitSlider.value = playtimeToFloat;
//             }
//             if (float.TryParse(fishspeed, out var fishspeedToFloat))
//             {
//                _fishSpeedSlider.value = fishspeedToFloat;
//             }
//             if (float.TryParse(fishgoalcount, out var fishgoalcountToFloat))
//             {
//                 _fishGoalCountSlider.value = (int)fishgoalcountToFloat;
//             }
//             
// #if UNITY_EDITOR
// //            Debug.Log($"setting by XML Completed main {mainVolResult}, effect {bgmvolToFloat}, timelimt {playtimeToFloat}");
// #endif
//             
//         }
//     }

    
    public static event Action<float, float, float, int, float, int> OnSaveCurrentSettingClicked;
    private void SaveCurrentSetting()
    {
        var tempRootSetting = _gm.xmlDoc_Setting.DocumentElement;
        tempRootSetting.RemoveAll();
        
        OnSaveCurrentSettingClicked?.Invoke(_sliders[(int)SoundManager.Sound.Main].value,
            _sliders[(int)SoundManager.Sound.Bgm].value,
            _sliders[(int)SoundManager.Sound.Effect].value
            ,(int)_timeLimitSlider.value
            ,_fishSpeedSlider.value
            ,(int)_fishGoalCountSlider.value);

       

    }

   
    private void OnResetSetting()
    {
        var tempRootSetting = _gm.xmlDoc_Setting.DocumentElement;
        tempRootSetting.RemoveAll();
        
        _sliders[(int)SoundManager.Sound.Main].value = SoundManager.VOLUME_INITVALUE_MAIN;
        _sliders[(int)SoundManager.Sound.Bgm].value = SoundManager.VOLUME_INITVALUE_BGM;
        _sliders[(int)SoundManager.Sound.Effect].value = SoundManager.VOLUME_INITVALUE_EFFECT;
        
        _timeLimitSlider.value = 30;
        _fishSpeedSlider.value = 1;
        _fishGoalCountSlider.value = 30;
        
        OnResetSettingBtnClicked?.Invoke();
    }
    
    
    

}