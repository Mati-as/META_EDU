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

public class U_FishOnWater_UIManager : UI_PopUp
{
    private enum UI_Type
    {
        Ready,
        Start,
        Stop,
        Timer,
        Count,
        RankingParent,
        Btn_Restart,
        CurrentUser,
        OnRankingUsers
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
        Score,
        Max
    }

    //  private IGameManager _gm; 
    private CanvasGroup _canvasGroup;

    private GameObject _start;
    private GameObject _ready;
    private GameObject _stop;
    private GameObject _fishCount;
    private GameObject _timer;
    private GameObject _button;
    private GameObject _usersOnRankingObj;
    private GameObject _rankingParent; //랭킹 프리팹 부모관리
    private GameObject _onRankingUsers;
    private GameObject _currentUser;
    private GameObject _restart;

    private TextMeshProUGUI _fishCountTMP;
    private TextMeshProUGUI _timerTMP;

    private TextMeshProUGUI[] _TMP_usersOnRankScores;
    private TextMeshProUGUI[] _TMP_usersOnRankNames;
    private TextMeshProUGUI[] _TMP_currentUser;

    private RectTransform _rectStart;
    private RectTransform _rectReady;
    private RectTransform _rectStop;
    private RectTransform _rankingParentRect;
    private RectTransform _fishCountRect;
    private RectTransform _timerRect;
    private RectTransform _restartRect;
    private RectTransform _rankingSystemRect;

    private Button _restartButton;
    

    private readonly int USER_ON_RANKINNG_COUNT = 4;
    private string[][] _TMP_usersOnRankingData;

    private float _intervalBtwStartAndReady = 1f;
    private bool _isAnimating; // 더블클릭 방지 논리연산자 입니다. 
    public static event Action OnStartUIAppear; // 시작 UI가 표출될 때 의 이벤트입니다. 
    public static event Action OnRestartBtnClicked; // 그만 이후 초기화가 끝난경우

    private U_FishOnWater_GameManager _gm;

    public override bool Init()
    {
      
        
        
        _gm = GameObject.FindWithTag("GameManager").GetComponent<U_FishOnWater_GameManager>();

        BindObject(typeof(UI_Type));

        _start = GetObject((int)UI_Type.Start);
        _rectStart = _start.GetComponent<RectTransform>();
        _rectStart.localScale = Vector3.zero;
        _start.SetActive(false);

        _ready = GetObject((int)UI_Type.Ready);
        _rectReady = _ready.GetComponent<RectTransform>();
        _rectReady.localScale = Vector3.zero;
        _ready.SetActive(false);

        _stop = GetObject((int)UI_Type.Stop);
        _rectStop = _stop.GetComponent<RectTransform>();
        _rectStop.localScale = Vector3.zero;
        _stop.SetActive(false);

        // 타이머, 잡은 물고기 갯수 ---------------------------------------------------------------
        _timer = GetObject((int)UI_Type.Timer);
        _timerRect = _stop.GetComponent<RectTransform>();
        _timerTMP = _timer.GetComponent<TextMeshProUGUI>();

        _fishCount = GetObject((int)UI_Type.Count);
        _fishCountRect = _fishCount.GetComponent<RectTransform>();
        _fishCountTMP = _fishCount.GetComponent<TextMeshProUGUI>();

        // 랭킹시스템 다시시작, 잡은 물고기 갯수 -----------------------------------------------------
        _usersOnRankingObj = GetObject((int)UI_Type.OnRankingUsers);
        _rankingSystemRect = _usersOnRankingObj.GetComponent<RectTransform>();
        //_usersOnRankingObj.SetActive(false);


        _rankingParent = GetObject((int)UI_Type.RankingParent);
        _rankingParentRect = _rankingParent.GetComponent<RectTransform>();
        _rankingParentRect.localScale = Vector3.zero;
        _rankingParent.SetActive(false);
        
        //----------------------------------------------------------------------------------------
        _TMP_usersOnRankingData = new string[USER_ON_RANKINNG_COUNT][];

        for (int i = 0; i < _TMP_usersOnRankingData.Length; i++)
        {
            _TMP_usersOnRankingData[i] = new string[(int)RankUserInfo.Max];
        }
        

        _TMP_usersOnRankNames = new TextMeshProUGUI[USER_ON_RANKINNG_COUNT];
        _TMP_usersOnRankScores = new TextMeshProUGUI[USER_ON_RANKINNG_COUNT];
        
        
        for (int i = 0; i < USER_ON_RANKINNG_COUNT; i++)
        {
            
            Transform userTransform = _usersOnRankingObj.transform.GetChild(i);

            // Check if the user transform is found correctly
            if (userTransform == null)
            {
                Debug.LogError($"User transform not found at index {i}");
                continue;
            }

            _TMP_usersOnRankNames[i] = userTransform.Find("Text_UserName")?.GetComponent<TextMeshProUGUI>();
            _TMP_usersOnRankScores[i] = userTransform.Find("Text_Score_Value")?.GetComponent<TextMeshProUGUI>();

            
           // _TMP_usersOnRankNames[i]= _usersOnRankingObj.transform.GetChild(i).Find("Text_UserName").GetComponent<TextMeshProUGUI>();
           //
           // #if UNITY_EDITOR
           //  Debug.Log($" 랭킹 객체 이름 {_usersOnRankingObj.transform.Find("Text_UserName")}");
           //  #endif
           // _TMP_usersOnRankScores[i]= _usersOnRankingObj.transform.GetChild(i).Find("Text_Score_Value").GetComponent<TextMeshProUGUI>();
        }
        
        _currentUser = GetObject((int)UI_Type.CurrentUser);
        var _currentUserTransform = _currentUser.transform;
        _TMP_currentUser = new TextMeshProUGUI[(int)RankUserInfo.Max];
        _TMP_currentUser[(int)RankUserInfo.Name] = _currentUserTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _TMP_currentUser[(int)RankUserInfo.Score] = _currentUserTransform.GetChild(2).GetComponent<TextMeshProUGUI>();

        _restart = GetObject((int)UI_Type.Btn_Restart);
        _restartRect = _restart.GetComponent<RectTransform>();
        _restartButton = _restart.GetComponent<Button>();

        _restart.BindEvent(OnRestartBtnClicked);
        U_FishOnWater_GameManager.OnReady -= OnReadyAndStart;
        U_FishOnWater_GameManager.OnReady += OnReadyAndStart;

        U_FishOnWater_GameManager.OnRoundFinished -= PopUpStopUI;
        U_FishOnWater_GameManager.OnRoundFinished += PopUpStopUI;

        OnRestartBtnClicked -= OnRestartBtnClick;
        OnRestartBtnClicked += OnRestartBtnClick;
        return true;
    }

    private void OnDestroy()
    {
        OnRestartBtnClicked -= OnRestartBtnClick;
        U_FishOnWater_GameManager.OnReady -= OnReadyAndStart;
        U_FishOnWater_GameManager.OnRoundFinished -= PopUpStopUI;
    }

    private void Update()
    {
        if (!_gm.isOnReInit)
        {
            _timerTMP.text = _gm.remainTime.ToString("F1") + "초";
            _fishCountTMP.text =
                _gm.FishCaughtCount == _gm.FISH_COUNT ? "물고기를 모두 다 잡았어요!" : _gm.FishCaughtCount + " 마리";
        }
    }

    public void OnReadyAndStart()
    {
        PopUpStartUI();
    }
    private void ParseXML()
    {
        XmlNode root = _gm.xmlDoc.DocumentElement;
        XmlNodeList nodes = root.SelectNodes("StringData");

        List<Tuple<string, string>> userScores = new List<Tuple<string, string>>();

        foreach (XmlNode node in nodes)
        {
            string username = node.Attributes["username"].Value;
            float score = float.Parse(node.Attributes["score"].Value);

            userScores.Add(new Tuple<string, string>(username, score.ToString()));
        }
        
        userScores = userScores.OrderByDescending(x => float.Parse(x.Item2)).ToList();
      


#if UNITY_EDITOR
        Debug.Log($"saved user Counts in XML{userScores.Count} ");
#endif

        for (int i = 0; i < 4 && i < userScores.Count; i++)
        {
            _TMP_usersOnRankingData[i][(int)RankUserInfo.Name] = userScores[i].Item1;
            _TMP_usersOnRankingData[i][(int)RankUserInfo.Score] = userScores[i].Item2;
#if UNITY_EDITOR
            Debug.Log($"ranking data {i} : { userScores[i].Item1} ");
            Debug.Log($"ranking data {i} : { userScores[i].Item2} ");
#endif

            _TMP_usersOnRankNames[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.Name];
            _TMP_usersOnRankScores[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.Score];
        }
        
        
        
        
    }

    private void PopUpStartUI()
    {
        StartCoroutine(PopUpStartUICoroutine());
    }

    private void PopUpStopUI()
    {
        StartCoroutine(PopUpStopUICoroutine());
    }


    private WaitForSeconds _wait;
    private WaitForSeconds _waitInterval;
    private WaitForSeconds _waitForReady;
    private float _waitTIme = 4.5f;


    private IEnumerator PopUpStartUICoroutine()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(1f);

        if (_waitForReady == null) _waitForReady = new WaitForSeconds(1f);

        yield return _waitForReady;
        _ready.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                _fishCountTMP.text = _gm.FishCaughtCount + " 마리";
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Ready", 0.8f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectReady.localScale = Vector3.one * scale; })
            .WaitForCompletion();


        yield return _waitInterval;
        _start.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Start", 0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle", 0.4f);
                OnStartUIAppear?.Invoke();
#if UNITY_EDITOR
                Debug.Log("UI Invoke");
#endif
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStart.localScale = Vector3.one * scale; })
            .WaitForCompletion();
    }

    private IEnumerator PopUpStopUICoroutine()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.3f);

        _fishCountTMP.text = _gm.FishCaughtCount >= 20 ? "물고기를 전부 잡았어요!" : "시간이 다 지났어요!";


        yield return _waitInterval;
        _stop.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 0.45f, scale => { _rectStop.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Stop", 0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle", 0.4f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStop.localScale = Vector3.one * scale; })
            .WaitForCompletion();
        //OnUIFinished?.Invoke();

        _fishCountTMP.text = _gm.FishCaughtCount >= 20 ? "물고기를 전부 잡았어요!" : $"물고기를 {_gm.FishCaughtCount}마리 잡았어요";

        PopUpRankingSystem();
    }

    private void LoadRankingInfo()
    {
        
        _TMP_currentUser[(int)RankUserInfo.Name].text = _gm.currentUserName;
        _TMP_currentUser[(int)RankUserInfo.Score].text = _gm.currentUserScore;
        ParseXML();
    }
    private IEnumerator PopUpRankSystemCo()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.3f);

        LoadRankingInfo();
        yield return _waitInterval;
        _rankingParent.SetActive(true);
        yield return
            DOVirtual.Float(0, 1, 0.45f, scale => { _rankingParentRect.localScale = Vector3.one * scale; });
    }

    private void PopUpRankingSystem()
    {
        StartCoroutine(PopUpRankSystemCo());
    }

    private void OnRestartBtnClick()
    {
        StartCoroutine(OnRestartBtnClickCo());
    }


    private IEnumerator OnRestartBtnClickCo()
    {
        if (_isAnimating) yield break;

        
        
        yield return _waitInterval;
        _isAnimating = true;
       
        
        yield return DOVirtual.Float(1, 0, 0.45f, scale => { _rankingParentRect.localScale = Vector3.one * scale; })
            .WaitForCompletion();
        _rankingParent.SetActive(false);
        
        _isAnimating = false;
    }
}