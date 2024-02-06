using System;
using System.Collections;
using System.Xml;
using DG.Tweening;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

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


    //Speech Bubble UI관련 세팅
    private TextAsset _xmlAsset;
    private XmlNode _soundNode;
    private XmlDocument _xmlDoc;
    private TMP_Text _tmp;

    private Coroutine _typingCoroutine;
    private readonly float _textPrintingSpeed = 0.03f;
    private int _currentLineIndex;
    private bool _isNextUIAppearable;

    private static event Action onCrabSpeechBubbleFinished; 


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        isCrabAppearable = true;

        _crabSpeechBubbleUI = GameObject.Find("Crab_SpeechBubble").GetComponent<RectTransform>();
        _speechBubbleDefaultSize = _crabSpeechBubbleUI.localScale;
        _crabSpeechBubbleUI.localScale = Vector3.zero;
        _tmp = _crabSpeechBubbleUI.GetComponentInChildren<TMP_Text>();
        _tmp.text = string.Empty;


        _xmlAsset = Resources.Load<TextAsset>("게임별분류/비디오컨텐츠/Owl/Video_UI_text_Data");
        _xmlDoc = new XmlDocument();
        _xmlDoc.LoadXml(_xmlAsset.text);

        SubscribeEvent();

        rewindParticleAudioPath = "Audio/비디오 컨텐츠/Crab/Bubbles";

    }


    
    public float replayOffset;
    
    protected override void Init()
    {
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
        
        onCrabSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
        onCrabSpeechBubbleFinished += OnOwlSpeechBubbleFinished;
        
        onReplay -= OnReplay;
        onReplay += OnReplay;
        
    }
    
    private void OnDestroy()
    {
      
        onCrabAppear -= OnCrabAppear;
        onCrabSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
        onReplay -= OnReplay;
    }


    public int crabAppearClickCount;
    private int _currentClickCount;
    private bool _isOnCrabAppearEventInvoked;

    public static event Action onCrabAppear;

    private void CrabOnRaySynced()
    {
        base.OnRaySynced();
        if (!_initiailized) return;

        if (!_isShaked) transform.DOShakePosition(2.25f, 1f + 0.1f * _currentClickCount, randomness: 90, vibrato: 5);

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
        PlaySpeechBubble(12.5f);
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
                            PlayNextMessageAnim(_currentLineIndex);
                            
                        videoPlayer.playbackSpeed = 0;
                    });
            });
    }

    private void Play() => videoPlayer.playbackSpeed = 1;
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
        
        if (_isNextUIAppearable)
        {
            if (_currentLineIndex >= 2) return;
            _isNextUIAppearable = false;
            _currentLineIndex++;
            PlayNextMessageAnim(_currentLineIndex);
        }
    }  
    

    
    public IEnumerator TypeIn(string str, float offset)
    {
        _tmp.text = ""; // 초기화
        yield return new WaitForSeconds(offset);

        var strTypingLength = str.GetTypingLength();
        for (var i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            _tmp.text = str.Typing(i);
            yield return new WaitForSeconds(_textPrintingSpeed);
        }


        yield return new WaitForNextFrameUnit();
    }
    
    private void PlayNextMessageAnim(int currentIndex)
    {
     
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);


        var node = _xmlDoc.SelectSingleNode($"//StringData[@ID='{"Crab_UI_" + _currentLineIndex}']");
        if (node != null)
        {
            var message = node.Attributes["string"].Value;
            _tmp.text = message;
        }
        else
        {
            
#if UNITY_EDITOR
Debug.Log($"the current XML node is null");
#endif
        }

        _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0.3f));

        //duration -> 10
        DOVirtual.Float(0, 1, nextUIApperableWaitTime, _ => { })
            .OnComplete(() => { _isNextUIAppearable = true; });


        if (_currentLineIndex >= 2)
            DOVirtual.Float(0, 1, 0.5f, _ => { })
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log($"꽃게 대사 끝. 대사 번호: {currentIndex}");
#endif
                  
                    onCrabSpeechBubbleFinished?.Invoke();
                   
                });
    }
    private void OnOwlSpeechBubbleFinished()
    {
        
       
        DOVirtual.Float(0, 1, 0.5f, _ => { })
            .OnComplete(() =>
            {
                _crabSpeechBubbleUI.transform
                    .DOScale(Vector3.zero, 2f)
                    .SetEase(Ease.OutBounce)
                    .OnStart(() =>
                    {
                        
                        StopCoroutine(_typingCoroutine);
                        Play();
                    })
                    .OnComplete(() =>
                    {
                     
                        //_isRewind가 false라는 것은, 두번째 나뭇잎이 다시 밝아지기위해 화면이 멈추는 경우를 말합니다. 
                    }).SetDelay(1f);
            });
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
                                PlayNextMessageAnim(_currentLineIndex);
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