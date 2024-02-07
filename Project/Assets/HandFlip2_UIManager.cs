using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class HandFlip2_UIManager : UI_PopUp
{

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

    public float fadeOutDelay;
    private CanvasGroup _canvasGroup;

    private GameObject _ready;
    private GameObject _start;
    private GameObject _blueWin;
    private GameObject _redWin;
    
    private RectTransform _rectReady;
    private RectTransform _rectStart;
    private RectTransform _rectBlueWin;
    private RectTransform _rectRedWin;

    public override bool Init()
    {
        Init();
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
        
        _rectRedWin = _redWin.GetComponent<RectTransform>();
        _redWin = GetObject((int)HandFlip_UI_Type.Red_Win);
        _redWin.SetActive(false);

        GetButton((int)Buttons.StartButton).gameObject.BindEvent(OnStart);
        return true;
        
    }

    public void OnStart()
    {
        DOVirtual.Float(-0, -0, 1, _ => { }).OnComplete(() =>
        {
            DOVirtual.Float(0, 1, 1, scale =>
            {
                _rectReady.localScale = Vector3.one * scale;
            });
        });
    }

}
