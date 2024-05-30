using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;

public class U_FishOnWater_UIManager : UI_PopUp
{
      
    enum HandFlip_UI_Type
    {
        Ready,
        Start,
        Stop,
        Timer,
        Count,
        Max
    }

    //  private IGameManager _gm; 
    private CanvasGroup _canvasGroup;
    
    private GameObject _start;
    private GameObject _ready;
    private GameObject _stop;
    private GameObject _fishCount;
    private GameObject _timer;

    private TextMeshProUGUI _fishCountTMP;
    private TextMeshProUGUI _timerTMP;

    private RectTransform _rectStart;
    private RectTransform _rectReady;
    private RectTransform _rectStop;
    private RectTransform _fishCountRect;
    private RectTransform _timerRect;

    private float  _intervalBtwStartAndReady =1f;
    public static event Action OnStartUIAppear;// 시작 UI 동작! 
    public static event Action OnUIFinished; // 그만 이후 초기화가 끝난경우
    private U_FishOnWater_GameManager _gm; 

    public override bool Init()
    {

    _gm = GameObject.FindWithTag("GameManager").GetComponent<U_FishOnWater_GameManager>();
        
    BindObject(typeof(HandFlip_UI_Type));
    
    _start = GetObject((int)HandFlip_UI_Type.Start);
    _rectStart =_start.GetComponent<RectTransform>();
    _rectStart.localScale = Vector3.zero;
    _start.SetActive(false);
    
    _ready = GetObject((int)HandFlip_UI_Type.Ready);
    _rectReady =_ready.GetComponent<RectTransform>();
    _rectReady.localScale = Vector3.zero;
    _ready.SetActive(false);
    
    _stop = GetObject((int)HandFlip_UI_Type.Stop);
    _rectStop =_stop.GetComponent<RectTransform>();
    _rectStop.localScale = Vector3.zero;
    _stop.SetActive(false);
    
    
    // 타이머, 잡은 물고기 갯수 --------------------------------------------------------
    _timer = GetObject((int)HandFlip_UI_Type.Timer);
    _timerRect =_stop.GetComponent<RectTransform>();
    _timerTMP = _timer.GetComponent<TextMeshProUGUI>();
    
    _fishCount = GetObject((int)HandFlip_UI_Type.Count);
    _fishCountRect =_fishCount.GetComponent<RectTransform>();
    _fishCountTMP = _fishCount.GetComponent<TextMeshProUGUI>();

        
    U_FishOnWater_GameManager.OnReady -= OnReadyAndStart;
    U_FishOnWater_GameManager.OnReady += OnReadyAndStart;

    U_FishOnWater_GameManager.OnFinished -= PopUpStopUI;
    U_FishOnWater_GameManager.OnFinished += PopUpStopUI;

    return true;
    
    }

    private void OnDestroy()
    {
        U_FishOnWater_GameManager.OnReady -= OnReadyAndStart;
        U_FishOnWater_GameManager.OnFinished -= PopUpStopUI;
    }

    private void Update()
    {
        if (!_gm.isOnReInit)
        {
        _timerTMP.text = _gm.remainTime.ToString("F1")+"초";
        _fishCountTMP.text = _gm.FishCaughtCount == _gm.FISH_COUNT? "물고기를 모두 다 잡았어요!" :
        _gm.FishCaughtCount.ToString() + " 마리";
        }
    
    }

    public void OnReadyAndStart()
    {

        PopUpStartUI();
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
    private float _waitTIme= 4.5f;
    

    private IEnumerator PopUpStartUICoroutine()
    {
       
        if (_waitInterval == null)
        {
            _waitInterval = new WaitForSeconds(1f);
        }
        
        if (_waitForReady == null)
        {
            _waitForReady = new WaitForSeconds(1f);
        }
        
        yield return _waitForReady;
        _ready.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                _fishCountTMP.text =_gm.FishCaughtCount.ToString() + " 마리";
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Ready",0.8f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();
        
        
        yield return _waitInterval;
        _start.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Start",0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle",0.4f);
                OnStartUIAppear?.Invoke();
#if UNITY_EDITOR
                Debug.Log("UI Invoke");
#endif
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).WaitForCompletion();
        
    }
    
    private IEnumerator PopUpStopUICoroutine()
    {
       
        if (_waitInterval == null)
        {
            _waitInterval = new WaitForSeconds(0.3f);
        }

        _fishCountTMP.text = _gm.FishCaughtCount >= 20 ? "물고기를 전부 잡았어요!" : "시간이 다 지났어요!";
    
        
        yield return _waitInterval;
        _stop.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 0.45f, scale => { _rectStop.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Stop",0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle",0.4f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStop.localScale = Vector3.one * scale; }).WaitForCompletion();
        OnUIFinished?.Invoke();
        
        _fishCountTMP.text = _gm.FishCaughtCount >= 20? "물고기를 전부 잡았어요!" : $"물고기를 {_gm.FishCaughtCount}마리 잡았어요";

    }
}
