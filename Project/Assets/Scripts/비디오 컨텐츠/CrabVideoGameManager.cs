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

public class CrabVideoGameManager : InteractableVideoGameManager
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
    
    public static readonly float VIDEO_STOP_DELAY = 23.5f;

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
        
        SubscribeEvent();

        rewindParticleAudioPath = "Audio/비디오 컨텐츠/Crab/Bubbles";
     
       
        Camera uiCamera = _UIManager.GetComponentInChildren<Camera>();
        
        if (Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var mainCameraData))
        {
            mainCameraData.cameraStack.Add(uiCamera);
        }
        else
        {
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData component.");
        }

    
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

    
        Debug.Log("UI Init");
        var UIinstance = Resources.Load<GameObject>("게임별분류/비디오컨텐츠/Crab/Prefab/Crab_UI_Scene");
        var root = GameObject.Find("@Root");
        _UIManager = Instantiate(UIinstance, root.transform);
      
        isInitialized = true;
    }
    
    public void SubscribeEvent()
    {
        
        onCrabAppear -= OnCrabAppear;
        onCrabAppear += OnCrabAppear;
        
        // onCrabSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
        // onCrabSpeechBubbleFinished += OnOwlSpeechBubbleFinished;
        
        onReplay -= OnReplay;
        onReplay += OnReplay;
        Crab_UIManager.onCrabDialogueFinished -= PlayAgain;
        Crab_UIManager.onCrabDialogueFinished += PlayAgain;
    }
    
    private void OnDestroy()
    {
      
        onCrabAppear -= OnCrabAppear;
        // onCrabSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
        onReplay -= OnReplay;
    }


    private void CrabOnRaySynced()
    {
        base.OnRaySynced();
        if (!_initiailized) return;

        if (!_isShaked) transform.DOShakePosition(1.2f, 0.5f, randomness: 90, vibrato: 6);

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
                          //  PlayNextMessageAnim(_currentLineIndex);
                            
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
    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!isStartButtonClicked) return;
        CrabOnRaySynced();
        
        // if (_isNextUIAppearable)
        // {
        //     if (_currentLineIndex >= 2) return;
        //     _isNextUIAppearable = false;
        //     _currentLineIndex++;
        //     PlayNextMessageAnim(_currentLineIndex);
        // }
        
        // 부엉이 대사 관련 UI 제어 파트 -------------------------------------------------
         if(isCrabPlayingUI)onRaySyncForCrabUI?.Invoke();
    }  
    

    

    
    // private void PlayNextMessageAnim(int currentIndex)
    // {
    //
    //  onCrabSpeechBubbleFinished?.Invoke();
    //     
    // }
    // private void OnOwlSpeechBubbleFinished()
    // {
    //     
    //    
    //     DOVirtual.Float(0, 1, 0.5f, _ => { })
    //         .OnComplete(() =>
    //         {
    //             _crabSpeechBubbleUI.transform
    //                 .DOScale(Vector3.zero, 2f)
    //                 .SetEase(Ease.OutBounce)
    //                 .OnStart(() =>
    //                 {
    //                     
    //                     StopCoroutine(_typingCoroutine);
    //                     Play();
    //                 })
    //                 .OnComplete(() =>
    //                 {
    //                 }).SetDelay(1f);
    //         });
    // }

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
                               // PlayNextMessageAnim(_currentLineIndex);
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