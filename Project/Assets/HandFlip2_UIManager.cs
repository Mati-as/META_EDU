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
        
        
        BindObject(typeof(HandFlip_UI_Type));

        _ready = GetObject((int)HandFlip_UI_Type.Ready);
        _rectReady = _ready.GetComponent<RectTransform>();
        _ready.SetActive(false);
        
        _start = GetObject((int)HandFlip_UI_Type.Start);
        _rectStart =_start.GetComponent<RectTransform>();
        _start.SetActive(false);
        
        
        _blueWin = GetObject((int)HandFlip_UI_Type.Blue_Win);
        _rectBlueWin = _blueWin.GetComponent<RectTransform>();
        _blueWin.SetActive(false);
        
    
        _redWin = GetObject((int)HandFlip_UI_Type.Red_Win);
        _rectRedWin = _redWin.GetComponent<RectTransform>();
        _redWin.SetActive(false);

        UI_Scene_Button.onBtnShut += OnStart;
        return true;
        
    }

    public void OnStart()
    {
#if UNITY_EDITOR
        Debug.Log("Button Click: UI event binding successful and event execution");
#endif
        StartCoroutine(SequenceAnimations());
    }

    private IEnumerator SequenceAnimations()
    {
        _ready.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectReady.localScale = Vector3.one * scale; }).WaitForCompletion();

        _start.gameObject.SetActive(true);
        _rectStart.localScale = Vector3.zero;
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).SetDelay(_intervalBtwStartAndReady).WaitForCompletion();
        yield return DOVirtual.Float(1, 0, 0.6f, scale => { _rectStart.localScale = Vector3.one * scale; }).WaitForCompletion();
        
        isStart = true;

        _ready.gameObject.SetActive(false);
        _start.gameObject.SetActive(false);
        
        onStartUIFinished?.Invoke();
    }
}




