using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using Unity.VisualScripting;
using UnityEngine;

public class ScratchPainting_UIManager : UI_PopUp
{
 enum HandFlip_UI_Type
    {
        Ready,
        Start,
        Stop,
        LetsPaint
    }

    //  private IGameManager _gm; 
    private CanvasGroup _canvasGroup;
    
    private GameObject _ready;
    private GameObject _start;
    private GameObject _stop;
    private GameObject _letsPaint;
    
    private RectTransform _rectReady;
    private RectTransform _rectStart;
    private RectTransform _rectStop;
    private RectTransform _rectLetsPaint;

    private float  _intervalBtwStartAndReady =1f;
    public static event Action onStartUI;

    private WaitForSeconds _wait;
    private WaitForSeconds _waitInterval;
    private WaitForSeconds _waitForReady;
    private float _waitTIme= 4.5f;

    public override bool InitOnLoad()
    {

      //  _gm = GameObject.FindWithTag("GameManager").GetComponent<IGameManager>();
        
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
        
        _letsPaint = GetObject((int)HandFlip_UI_Type.LetsPaint);
        _rectLetsPaint =_letsPaint.GetComponent<RectTransform>();
        _rectLetsPaint.localScale = Vector3.zero;
        _letsPaint.SetActive(false);
        
        UI_InScene_StartBtn.onGameStartBtnShut -= OnGameStartStart;
        UI_InScene_StartBtn.onGameStartBtnShut += OnGameStartStart;
        
        ScratchPaintingBaseGameManager.onRoundRestart -= OnGameStartStart;
        ScratchPaintingBaseGameManager.onRoundRestart += OnGameStartStart;

        ScratchPaintingBaseGameManager.OnStampingFinished -= PopUpStopUI;
        ScratchPaintingBaseGameManager.OnStampingFinished += PopUpStopUI;
    
        return true;
        
    }

    private void OnDestroy()
    {
        ScratchPaintingBaseGameManager.onRoundRestart -= OnGameStartStart;
        ScratchPaintingBaseGameManager.OnStampingFinished -= PopUpStopUI;
    }

    public void OnGameStartStart()
    {
#if UNITY_EDITOR
        Debug.Log("Button Click: UI event binding successful and event execution");
#endif
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
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Ready",0.8f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();
        
        
        yield return _waitInterval;
        _start.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Start",0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Whistle",0.4f);
                onStartUI?.Invoke();
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).WaitForCompletion();
        
    }
    
    private IEnumerator PopUpStopUICoroutine()
    {
       
        if (_waitInterval == null)
        {
            _waitInterval = new WaitForSeconds(1f);
        }
        
    
        //1.그만!
        yield return _waitInterval;
        _stop.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStop.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Stop",0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Whistle",0.4f);
            }).WaitForCompletion();
        yield return _waitInterval; 
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStop.localScale = Vector3.one * scale; }).WaitForCompletion();
        
        
        //2.그림을 그려보세요
      
        yield return _waitInterval;
        _rectLetsPaint.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectLetsPaint.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
               //Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Stop",0.8f);
               //Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Whistle",0.4f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 3f, _ => { }).WaitForCompletion();
    
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectLetsPaint.localScale = Vector3.one * scale; }).WaitForCompletion();

    }
}
