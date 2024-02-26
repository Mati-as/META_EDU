using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class HandPainting_UIManager : UI_PopUp
{
    
    enum HandFlip_UI_Type
    {
        Start,
    }

    //  private IGameManager _gm; 
    private CanvasGroup _canvasGroup;
    
    private GameObject _start;

    

    private RectTransform _rectStart;

    private float  _intervalBtwStartAndReady =1f;

    public override bool Init()
    {

      //  _gm = GameObject.FindWithTag("GameManager").GetComponent<IGameManager>();
        
        BindObject(typeof(HandFlip_UI_Type));
        
        _start = GetObject((int)HandFlip_UI_Type.Start);
        _rectStart =_start.GetComponent<RectTransform>();
        _rectStart.localScale = Vector3.zero;
        _start.SetActive(false);
        

        UI_Scene_Button.onBtnShut -= OnStart;
        UI_Scene_Button.onBtnShut += OnStart;
        
        HandFootPainting_GameManager.onRoundRestart -= OnStart;
        HandFootPainting_GameManager.onRoundRestart += OnStart;
    
        return true;
        
    }

    public void OnStart()
    {
#if UNITY_EDITOR
        Debug.Log("Button Click: UI event binding successful and event execution");
#endif
        PopUpStartUI();
    }
    
    
    private void PopUpStartUI()
    {
        StartCoroutine(PopUpStopUICoroutine());
    }

    private WaitForSeconds _wait;
    private WaitForSeconds _waitForStart;
    private float _waitTIme= 4.5f;
    

    private IEnumerator PopUpStopUICoroutine()
    {
       
        if (_waitForStart == null)
        {
            _waitForStart = new WaitForSeconds(1f);
        }
        yield return _waitForStart;
        _start.gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Start",0.8f);
            }).WaitForCompletion();
        yield return _waitForStart;
        yield return DOVirtual.Float(1, 0, 1, scale => { _rectStart.localScale = Vector3.one * scale; }).WaitForCompletion();
        
    }
}
