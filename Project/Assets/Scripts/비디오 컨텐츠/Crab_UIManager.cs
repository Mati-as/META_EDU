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

public class Crab_UIManager : UI_PopUp
{

    public bool isStart { get; private set; }


    enum CrabDialogue
    {
        Crab_SpeechBubble,
        Crab_SpeechBubble_Flipped,
        Crab_Dialogue1,
        Crab_Dialogue2,
        Crab_Dialogue3,
        Crab_Dialogue4,
        Crab_Dialogue5,
        Max
    }
  

    private HandFlip2_GameManager _gm; 
    private CanvasGroup _canvasGroup;
    private Vector3 _SpeechBubbleDefaultScale;
    private Vector3 _defaultScale;

    private RectTransform[] _dialogues;
    private GameObject Crab_SpeechBubble;
    private GameObject Crab_SpeechBubbleFlpped;
    private GameObject Crab_Dialogue1;
    private GameObject Crab_Dialogue2;
    private GameObject Crab_Dialogue3;
    private GameObject Crab_Dialogue4;
    private GameObject Crab_Dialogue5;
    
    
    
    private RectTransform _rectSpeechBubble;
    private RectTransform _rectSpeechBubbleFlipped;
    private RectTransform _rectDialogue1;
    private RectTransform _rectDialogue2;
    private RectTransform _rectDialogue3;
    private RectTransform _rectDialogue4;
    private RectTransform _rectDialogue5;
    private Vector3[] defaultScales;

    private float  _intervalBtwStartAndReady =1f;
    
    
    private int _currentUiIndex = (int)CrabDialogue.Crab_Dialogue1;
    

    public override bool Init()
    {

      
        _gm = GameObject.Find("GameManager").GetComponent<HandFlip2_GameManager>();
        _dialogues = new RectTransform[(int)CrabDialogue.Max];
        defaultScales = new Vector3[(int)CrabDialogue.Max];
        BindObject(typeof(CrabDialogue));

        
        Crab_SpeechBubble = GetObject((int)CrabDialogue.Crab_SpeechBubble);
        _rectSpeechBubble = Crab_SpeechBubble.GetComponent<RectTransform>();
        _SpeechBubbleDefaultScale = _rectSpeechBubble.localScale;
        defaultScales[(int)CrabDialogue.Crab_SpeechBubble] = _rectSpeechBubble.localScale;
        _rectSpeechBubble.localScale = Vector3.zero;
        Crab_SpeechBubble.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_SpeechBubble] = _rectSpeechBubble;
      
        
        Crab_Dialogue1 = GetObject((int)CrabDialogue.Crab_Dialogue1);
        _rectDialogue1 = Crab_Dialogue1.GetComponent<RectTransform>();
        defaultScales[(int)CrabDialogue.Crab_Dialogue1] = _rectDialogue1.localScale;
        _rectDialogue1.localScale = Vector3.zero;
        Crab_Dialogue1.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_Dialogue1] = _rectDialogue1;
        
        Crab_Dialogue2 = GetObject((int)CrabDialogue.Crab_Dialogue2);
        _rectDialogue2 =Crab_Dialogue2.GetComponent<RectTransform>();
        defaultScales[(int)CrabDialogue.Crab_Dialogue2] = _rectDialogue2.localScale;
        _rectDialogue2.localScale = Vector3.zero;
        Crab_Dialogue2.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_Dialogue2] = _rectDialogue2;
        
        Crab_Dialogue3 = GetObject((int)CrabDialogue.Crab_Dialogue3);
        _rectDialogue3 = Crab_Dialogue3.GetComponent<RectTransform>();
        defaultScales[(int)CrabDialogue.Crab_Dialogue3] = _rectDialogue3.localScale;
        _rectDialogue3.localScale = Vector3.zero;
        Crab_Dialogue3.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_Dialogue3] = _rectDialogue3;
        
        Crab_Dialogue4 = GetObject((int)CrabDialogue.Crab_Dialogue4);
        _rectDialogue4 = Crab_Dialogue4.GetComponent<RectTransform>();
        defaultScales[(int)CrabDialogue.Crab_Dialogue4] = _rectDialogue4.localScale;
        _rectDialogue4.localScale = Vector3.zero;
        Crab_Dialogue4.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_Dialogue4] = _rectDialogue4;
        
        Crab_Dialogue5 = GetObject((int)CrabDialogue.Crab_Dialogue5);
        _rectDialogue5 = Crab_Dialogue5.GetComponent<RectTransform>();
        defaultScales[(int)CrabDialogue.Crab_Dialogue5] = _rectDialogue5.localScale;
        _rectDialogue5.localScale = Vector3.zero;
        Crab_Dialogue5.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_Dialogue5] = _rectDialogue5;
        
        Crab_SpeechBubbleFlpped = GetObject((int)CrabDialogue.Crab_SpeechBubble_Flipped);
        _rectSpeechBubbleFlipped = Crab_SpeechBubbleFlpped.GetComponent<RectTransform>();
        _rectSpeechBubbleFlipped.localScale = Vector3.zero;
        Crab_SpeechBubbleFlpped.SetActive(false);
        _dialogues[(int)CrabDialogue.Crab_SpeechBubble_Flipped] = _rectSpeechBubbleFlipped;


        CrabVideoGameManager.onRewind -= OnCrabReWind;
        CrabVideoGameManager.onRewind += OnCrabReWind;
        
        CrabVideoGameManager.onRewind -= OnCrabReWind;
        CrabVideoGameManager.onRewind += OnCrabReWind;

        CrabVideoGameManager.onCrabAppear -= OnCrabUIStart;
        CrabVideoGameManager.onCrabAppear += OnCrabUIStart;

        CrabVideoGameManager.onRaySyncForCrabUI -= OnRaySyncForUI;
        CrabVideoGameManager.onRaySyncForCrabUI += OnRaySyncForUI;


        return true;
        
    }


    private void OnCrabUIStart()
    {
        DOVirtual.Float(0,1,CrabVideoGameManager.VIDEO_STOP_DELAY - 10, _ => { })
            .OnComplete(() =>
            {
                Crab_SpeechBubble.SetActive(true);
                Crab_SpeechBubbleFlpped.SetActive(true);
                _rectSpeechBubble.DOScale(_SpeechBubbleDefaultScale, 0.75f).SetEase(Ease.OutBounce);
            
                _currentUiIndex = (int)CrabDialogue.Crab_Dialogue1;
                
                _dialogues[_currentUiIndex].gameObject.SetActive(true);
                _dialogues[_currentUiIndex].DOScale(defaultScales[(int)CrabDialogue.Crab_Dialogue1], 1.2f).SetEase(Ease.OutBounce);

                Managers.soundManager.Play(SoundManager.Sound.Effect, 
                    "Audio/비디오 컨텐츠/Crab/Crab_UI_" + 
                    0,0.5f);

                DOVirtual.Float(0, 0, 5f, _ => { })
                .OnComplete(() =>
                {
                    _currentUiIndex++; 
                    _isNextUIPlayable = true;
                              
                });
               
              
            });
    }

    /// <summary>
    /// UI 재생조건
    /// 1. 이전 UI 재생 후 10초이상 지났을때..
    /// 2. 사용자가 화면을 클릭할때
    /// </summary>
    ///
    private bool _isNextUIPlayable;

    public static event Action onCrabDialogueFinished;
    private void OnRaySyncForUI()
    {

        if (!_isNextUIPlayable) return;
        _isNextUIPlayable = false;
        
        if (_currentUiIndex < (int)CrabDialogue.Max)
        {
            
            _dialogues[_currentUiIndex - 1].DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                
                Managers.soundManager.Play(SoundManager.Sound.Effect, 
                    "Audio/비디오 컨텐츠/Crab/Crab_UI_" + 
                    (_currentUiIndex-2).ToString(),0.5f);
#if UNITY_EDITOR
                Debug.Log($"현재 대사 순서 {(CrabDialogue)_currentUiIndex}");
#endif 
                _dialogues[_currentUiIndex - 1].gameObject.SetActive(false);
                
                DOVirtual.Float(0, 0, 0.05f, _ => { })
                    .OnComplete(() =>
                    {
                       
                        _dialogues[_currentUiIndex].gameObject.SetActive(true);
                        if (_currentUiIndex % 2 == (int)CrabDialogue.Crab_SpeechBubble)
                        {
                            _dialogues[(int)CrabDialogue.Crab_SpeechBubble].DOScale(_SpeechBubbleDefaultScale, 0.35f).SetEase(Ease.OutBounce);
                            _dialogues[(int)CrabDialogue.Crab_SpeechBubble_Flipped].DOScale(Vector3.zero, 0.35f).SetEase(Ease.OutBounce);
                            
                        }

                        if (_currentUiIndex % 2 == (int)CrabDialogue.Crab_SpeechBubble_Flipped)
                        {
                            _dialogues[(int)CrabDialogue.Crab_SpeechBubble_Flipped].DOScale(_SpeechBubbleDefaultScale, 0.35f).SetEase(Ease.OutBounce);
                            _dialogues[(int)CrabDialogue.Crab_SpeechBubble].DOScale(Vector3.zero, 0.35f).SetEase(Ease.OutBounce);
                        }
                        
                      
                        _dialogues[_currentUiIndex].DOScale(defaultScales[_currentUiIndex], 0.35f).SetEase(Ease.OutBounce).OnComplete(() =>
                        {
                            DOVirtual.Float(0, 0, 5f, _ => { })
                                .OnComplete(() =>
                                {
                                    _currentUiIndex++; 
                                    _isNextUIPlayable = true;
                                  
                                });

                        });
                    });
               

            });
            
            
        }
        else
        {
#if UNITY_EDITOR
Debug.Log("UI Finished Invoke And Play Video Again..");
#endif
            onCrabDialogueFinished?.Invoke();
            OnDialougeFinished();
            _isNextUIPlayable = false;
            _currentUiIndex = (int)CrabDialogue.Crab_Dialogue1;
        }
        
     
        
    }

    private void OnDialougeFinished()
    {
        _dialogues[(int)CrabDialogue.Crab_SpeechBubble].DOScale(Vector3.zero, 1.5f).SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                _dialogues[(int)CrabDialogue.Crab_SpeechBubble].gameObject.SetActive(false);
            });
        
        
        _dialogues[(int)CrabDialogue.Crab_Dialogue5].DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                _dialogues[(int)CrabDialogue.Crab_Dialogue5].gameObject.SetActive(false);
            });
    }

    private void OnCrabReWind()
    {
        _currentUiIndex = 0;
    }

}
