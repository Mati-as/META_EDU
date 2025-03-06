using System.Collections;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using System;
using DG.Tweening;


public class HandFlip2_UIManager : UI_PopUp
{


    public bool isStart { get; private set; }
    public static event Action onStartUIFinished;
    enum Buttons
    {
        StartButton
    }

    enum HandFlip_UI_Type
    {
        Ready,
        Start,
        Stop,
        Blue_Win,
        Red_Win
    }

    private HandFlip2BaseGameManager _gm; 
    private CanvasGroup _canvasGroup;

    private GameObject _ready;
    private GameObject _start;
    private GameObject _stop;
    private GameObject _blueWin;
    private GameObject _redWin;
    
    private RectTransform _rectReady;
    private RectTransform _rectStart;
    private RectTransform _rectStop;
    private RectTransform _rectBlueWin;
    private RectTransform _rectRedWin;

    private float  _intervalBtwStartAndReady =1f;

    public override bool Init()
    {

        _gm = GameObject.Find("GameManager").GetComponent<HandFlip2BaseGameManager>();
        
        BindObject(typeof(HandFlip_UI_Type));

        _ready = GetObject((int)HandFlip_UI_Type.Ready);
        _rectReady = _ready.GetComponent<RectTransform>();
        _rectReady.localScale = Vector3.zero;
        _ready.SetActive(false);
        
        _start = GetObject((int)HandFlip_UI_Type.Start);
        _rectStart =_start.GetComponent<RectTransform>();
        _start.SetActive(false);
        
        
        _blueWin = GetObject((int)HandFlip_UI_Type.Blue_Win);
        _rectBlueWin = _blueWin.GetComponent<RectTransform>();
        _rectBlueWin.localScale = Vector3.zero;
        _blueWin.SetActive(false);

        _stop = GetObject((int)HandFlip_UI_Type.Stop);
        _rectStop = _stop.GetComponent<RectTransform>();
        _rectStop.localScale = Vector3.zero;
        _stop.SetActive(false); 
        
        _redWin = GetObject((int)HandFlip_UI_Type.Red_Win);
        _rectRedWin = _redWin.GetComponent<RectTransform>();
        _rectRedWin.localScale = Vector3.zero;
        _redWin.SetActive(false);

        UI_Scene_StartBtn.onGameStartBtnShut -= OnGameStartStart;
        UI_Scene_StartBtn.onGameStartBtnShut += OnGameStartStart;
        
        HandFlip2BaseGameManager.onRoundFinishedForUI -= OnRoundFinish;
        HandFlip2BaseGameManager.onRoundFinishedForUI += OnRoundFinish;
        
        
           
        HandFlip2BaseGameManager.onRoundFinished -= PopStopUI;
        HandFlip2BaseGameManager.onRoundFinished += PopStopUI;
        
        HandFlip2BaseGameManager.restart -= OnGameStartStart;
        HandFlip2BaseGameManager.restart += OnGameStartStart;
        return true;
        
    }

    private void OnDestroy()
    {
        UI_Scene_StartBtn.onGameStartBtnShut -= OnGameStartStart;
        HandFlip2BaseGameManager.onRoundFinishedForUI -= OnRoundFinish;
        HandFlip2BaseGameManager.onRoundFinished -= PopStopUI;
        HandFlip2BaseGameManager.restart -= OnGameStartStart;
    
    }
    public void OnGameStartStart()
    {
#if UNITY_EDITOR
        Debug.Log("Button Click: UI event binding successful and event execution");
#endif
        StartCoroutine(PlayStartAnimations());
    }

    private IEnumerator PlayStartAnimations()
    {
        
       
        yield return DOVirtual.Float(0, 0, 1, _ => { }).WaitForCompletion();
        _ready.gameObject.SetActive(true);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Ready",1.0f);
        yield return DOVirtual.Float(0, 1, 0.2f, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();
        yield return DOVirtual.Float(0, 1f, 1f, _ => { }).WaitForCompletion();
        yield return DOVirtual.Float(1, 0, 0.2f, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();

        _start.gameObject.SetActive(true);
        _rectStart.localScale = Vector3.zero;
        yield return DOVirtual.Float(0, 1, 0.2f, scale => { _rectStart.localScale = Vector3.one * scale; }).SetDelay(_intervalBtwStartAndReady).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle",0.5f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Start",1.0f);
            }).WaitForCompletion();
        isStart = true;
       
        yield return DOVirtual.Float(1, 0, 0.2f, scale => { _rectStart.localScale = Vector3.one * scale; }).WaitForCompletion();
        
   
        _ready.gameObject.SetActive(false);
        _start.gameObject.SetActive(false);
        
        onStartUIFinished?.Invoke();
    }

    private void OnRoundFinish()
    {
        PopUI();
    }
    private void PopUI()
    {
        StartCoroutine(PlayWinnerUICoroutine());
    }
    
    private void PopStopUI()
    {
        StartCoroutine(PopUpStopUICoroutine());
    }

    private WaitForSeconds _wait;
    private WaitForSeconds _waitForStop;
    private float _waitTIme= 4.5f;
    
    private IEnumerator PlayWinnerUICoroutine()
    {
        isStart = false;
        
        if (_wait == null)
        {
            _wait = new WaitForSeconds(4.5f);
        }
        if (_gm.isATeamWin)
        {
            _redWin.gameObject.SetActive(true);
            yield return DOVirtual.Float(0, 1, 1, scale => { _rectRedWin.localScale = Vector3.one * scale; })
                .OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration,"Audio/BB004/Red_Win");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/나레이션/Narrations/RedWin",0.8f);
                })
                .WaitForCompletion();
          
            yield return _wait;
            yield return DOVirtual.Float(1, 0, 1, scale => { _rectRedWin.localScale = Vector3.one * scale; })
                .WaitForCompletion();
        }
        else
        {
            _blueWin.gameObject.SetActive(true);
            yield return DOVirtual.Float(0, 1, 1, scale => { _rectBlueWin.localScale = Vector3.one * scale; })
                .OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration,"Audio/BB004/Blue_Win");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/나레이션/Narrations/BlueWin",0.8f);
                }).WaitForCompletion();
          
            yield return _wait;
            yield return DOVirtual.Float(1, 0, 1, scale => { _rectBlueWin.localScale = Vector3.one * scale; })
                .WaitForCompletion();
        }
        

        _redWin.gameObject.SetActive(false);
        _blueWin.gameObject.SetActive(false);
        
    }
    private IEnumerator PopUpStopUICoroutine()
    {
       
        if (_waitForStop == null)
        {
            _waitForStop = new WaitForSeconds(1f);
        }
        
        _stop.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStop.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Stop",0.8f);
            }).WaitForCompletion();
        yield return _waitForStop;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStop.localScale = Vector3.one * scale; }).WaitForCompletion();
        
    }

}




