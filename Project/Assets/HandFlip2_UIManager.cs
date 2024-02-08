using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

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
        Blue_Win,
        Red_Win
    }

    private HandFlip2_GameManager _gm; 
    private CanvasGroup _canvasGroup;

    private GameObject _ready;
    private GameObject _start;
    private GameObject _blueWin;
    private GameObject _redWin;
    
    private RectTransform _rectReady;
    private RectTransform _rectStart;
    private RectTransform _rectBlueWin;
    private RectTransform _rectRedWin;

    private float  _intervalBtwStartAndReady =1f;

    public override bool Init()
    {

        _gm = GameObject.Find("GameManager").GetComponent<HandFlip2_GameManager>();
        
        BindObject(typeof(HandFlip_UI_Type));

        _ready = GetObject((int)HandFlip_UI_Type.Ready);
        _rectReady = _ready.GetComponent<RectTransform>();
        _ready.SetActive(false);
        
        _start = GetObject((int)HandFlip_UI_Type.Start);
        _rectStart =_start.GetComponent<RectTransform>();
        _start.SetActive(false);
        
        
        _blueWin = GetObject((int)HandFlip_UI_Type.Blue_Win);
        _rectBlueWin = _blueWin.GetComponent<RectTransform>();
        _rectBlueWin.localScale = Vector3.zero;
        _blueWin.SetActive(false);
        
    
        _redWin = GetObject((int)HandFlip_UI_Type.Red_Win);
        _rectRedWin = _redWin.GetComponent<RectTransform>();
        _rectRedWin.localScale = Vector3.zero;
        _redWin.SetActive(false);

        UI_Scene_Button.onBtnShut -= OnStart;
        UI_Scene_Button.onBtnShut += OnStart;
        
        HandFlip2_GameManager.onRoundFinishedForUI -= PopUI;
        HandFlip2_GameManager.onRoundFinishedForUI += PopUI;

        HandFlip2_GameManager.restart -= OnStart;
        HandFlip2_GameManager.restart += OnStart;
        return true;
        
    }

    public void OnStart()
    {
#if UNITY_EDITOR
        Debug.Log("Button Click: UI event binding successful and event execution");
#endif
        StartCoroutine(PlayStartAnimations());
    }

    private IEnumerator PlayStartAnimations()
    {
        
       

        _ready.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();

        _start.gameObject.SetActive(true);
        _rectStart.localScale = Vector3.zero;
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).SetDelay(_intervalBtwStartAndReady).WaitForCompletion();
        isStart = true;
        yield return DOVirtual.Float(1, 0, 0.6f, scale => { _rectStart.localScale = Vector3.one * scale; }).WaitForCompletion();
        
   
        _ready.gameObject.SetActive(false);
        _start.gameObject.SetActive(false);
        
        onStartUIFinished?.Invoke();
    }

    private void PopUI()
    {
        StartCoroutine(PlayWinnerUI());
    }

    private WaitForSeconds _wait;
    private float _waitTIme= 4.5f;
    private IEnumerator PlayWinnerUI()
    {
        isStart = false;
        
        if (_wait == null)
        {
            _wait = new WaitForSeconds(4.5f);
        }
        if (_gm.isATeamWin)
        {
            _redWin.gameObject.SetActive(true);
            yield return DOVirtual.Float(0, 1, 1, scale => { _rectRedWin.localScale = Vector3.one * scale; }).WaitForCompletion();
            yield return _wait;
            yield return DOVirtual.Float(1, 0, 1, scale => { _rectRedWin.localScale = Vector3.one * scale; }).WaitForCompletion();
        }
        else
        {
            _blueWin.gameObject.SetActive(true);
            yield return DOVirtual.Float(0, 1, 1, scale => { _rectBlueWin.localScale = Vector3.one * scale; }).WaitForCompletion();
            yield return _wait;
            yield return DOVirtual.Float(1, 0, 1, scale => { _rectBlueWin.localScale = Vector3.one * scale; }).WaitForCompletion();
        }
        

        _redWin.gameObject.SetActive(false);
        _blueWin.gameObject.SetActive(false);
        
    }
    

}




