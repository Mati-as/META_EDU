using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using DG.Tweening;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class CrabVideoBaseGameManager : InteractableVideoBaseGameManager
{
#if UNITY_EDITOR
    [Header("Debug Only")] public bool ManuallyReplay;
    
    [Space(15f)]
#endif
    
    public static bool _isShaked;
    
    private bool _isReplayEventTriggered;

    public bool isCrabAppearable { get; private set; }

    
    private RectTransform _crabSpeechBubbleUI;
    private Vector3 _speechBubbleDefaultSize;
    private GameObject _UIManager;
    
    private Coroutine _typingCoroutine;
    private int _currentLineIndex;
    private bool _isNextUIAppearable;
        
    public float replayOffset;
    
    //Camera DoShakePosition 강도 설정위한 bool값 입니다.
    private bool isCrabPlayingUI;
    
    public static readonly float VIDEO_STOP_DELAY = 21.5f;

    // Start is called before the first frame update

    public int crabAppearClickCount;
    private int _currentClickCount;
    private bool _isOnCrabAppearEventInvoked;

    private static event Action onCrabSpeechBubbleFinished; 
    public static event Action onCrabAppear;
    public static event Action onRaySyncForCrabUI;




    protected override void Start()
    {
        base.Start();
        DOTween.Init().SetCapacity(1000,1000);
        isCrabAppearable = true;
        DefaultSensitivity = 0.55f;
        
        SubscribeEvent();

        rewindParticleAudioPath = "Audio/비디오 컨텐츠/Crab/Bubbles";
    }


   
    protected override void Init()
    {
        if (isInitialized) return;
        base.Init();
        
        DOVirtual.Float(1, 0, 1.1f, speed =>
        {
            videoPlayer.playbackSpeed = speed;
            // 점프메세지 출력 이후 bool값 수정되도록 로직변경 필요할듯 12/26
        });
        
        
    }
    
    public void SubscribeEvent()
    {
        
        onCrabAppear -= OnCrabAppear;
        onCrabAppear += OnCrabAppear;
        
        onReplay -= OnReplay;
        onReplay += OnReplay;
        
        Crab_UIManager.onCrabDialogueFinished -= PlayAgain;
        Crab_UIManager.onCrabDialogueFinished += PlayAgain;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
      
        onCrabAppear -= OnCrabAppear;
        // onCrabSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
        onReplay -= OnReplay;
    }

    private float _clickInterval = 0.35f;
    private WaitForSeconds _clickWait;
    private bool _isClickable =true;
    private void SetClickable()
    {
        StartCoroutine(SetClickableCo());
    }

    private IEnumerator SetClickableCo()
    {
        _isClickable = false;
        
        if (_clickWait == null)
        {
            _clickWait = new WaitForSeconds(_clickInterval);
        }

        yield return _clickWait;

        _isClickable = true;

    }

    private void CrabOnRaySynced()
    {
        base.OnRaySynced();
        if (!_initiailized) return;
        if (!_isClickable)
        {
#if UNITY_EDITOR
            Debug.Log("it's not clickable temporary ----------------------");
#endif
            return;
        } 
        SetClickable();
        _currentClickCount++;

        if (_currentClickCount > crabAppearClickCount && !_isOnCrabAppearEventInvoked)
        {
            onCrabAppear?.Invoke();
            //effectManager에서 새로 crab 생성 못하게하는 bool값입니다. 
            isCrabAppearable = false;
            //event를 한번만 실행하도록 하는 boo값 입니다.  
            _isOnCrabAppearEventInvoked = true;


            DOVirtual.Float(0, 1, 1f, speed => { videoPlayer.playbackSpeed = speed; })
                .OnComplete(() => { _isShaked = true; });
        }
        

    }

    private void OnCrabAppear()
    {
        isCrabPlayingUI = true;
        DOVirtual.Float(0,0,VIDEO_STOP_DELAY, _ => { }).OnComplete(()=>
        {
#if UNITY_EDITOR
            Debug.Log("Stop Video ------------------");
#endif
            videoPlayer.playbackSpeed = 0;
        });
        
    }

    private void PlaySpeechBubble(float delay)
    {
        DOVirtual.Float(0, 1, delay, _ => { })
            .OnComplete(() =>
            {
                _crabSpeechBubbleUI.DOScale(_speechBubbleDefaultSize, 0.8f)
                    .OnComplete(() =>
                    {
#if UNITY_EDITOR
                            Debug.Log("꽃게 대사 재생시작");
#endif
                            
                        videoPlayer.playbackSpeed = 0;
                    });
            });
    }

    private void PlayAgain()
    {
        isCrabPlayingUI = false;
        DOVirtual.Float(0, 0, 3, _ => { }).OnComplete(() =>
        {
            videoPlayer.playbackSpeed = 1;
           
        });
    }

    private void TurnOffSpeechBubble(float delay)
    {
        DOVirtual.Float(0, 1, delay, _ => { })
            .OnComplete(() => { _crabSpeechBubbleUI.DOScale(Vector3.zero, 0.8f); });
    }
    public int nextUIApperableWaitTime; //10
    public override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!isStartButtonClicked) return;
        CrabOnRaySynced();
        
        // 부엉이 대사 관련 UI 제어 파트 -------------------------------------------------
         if(isCrabPlayingUI)onRaySyncForCrabUI?.Invoke();
    }  
    

    
    protected override void OnReplay()
    {
        //start Delay
        DOVirtual.Float(0, 1, 6f, _ => { })
            .OnComplete(() =>
            {
                _crabSpeechBubbleUI.transform
                    .DOScale(_speechBubbleDefaultSize, 2f)
                    .SetEase(Ease.OutBounce)
                    .OnStart(() =>
                    {
                        DOVirtual.Float(1, 0, 1f,
                                speed => { videoPlayer.playbackSpeed = speed; }).SetDelay(0.5f)
                            .OnComplete(() =>
                            {
#if UNITY_EDITOR
                                Debug.Log("꽃게 대사 재생시작");
#endif
                            });
                    })
                    .SetDelay(5f);
            });
    }

    protected override void OnRewind()
    {
        base.OnRewind();
        _currentClickCount = 0;
        isCrabAppearable = true;
        _isOnCrabAppearEventInvoked = false;
    }
}