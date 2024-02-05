using System;
using System.Collections;
using System.Xml;
using DG.Tweening;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CrabVideoGameManager : Video_GameManager
{
#if UNITY_EDITOR
    [Header("Debug Only")] public bool ManuallyReplay;
    
    [Space(15f)]
#endif

    [Header("Particle and Audio Setting")]
    [SerializeField]
    private ParticleSystem[] particleSystems;

    [SerializeField] private AudioSource particleSystemAudioSource;
    public static bool _isShaked;

    private bool _isReplayEventTriggered;

    public bool isCrabAppearable { get; private set; }

    public static event Action OnReplay;
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
    private void Start()
    {
        isCrabAppearable = true;

        _crabSpeechBubbleUI = GameObject.Find("Crab_SpeechBubble").GetComponent<RectTransform>();
        _speechBubbleDefaultSize = _crabSpeechBubbleUI.localScale;
        _crabSpeechBubbleUI.localScale = Vector3.zero;


        _xmlAsset = Resources.Load<TextAsset>("게임별분류/비디오컨텐츠/Owl/Owl_UI_Data");
        _xmlDoc = new XmlDocument();
        _xmlDoc.LoadXml(_xmlAsset.text);

        onCrabAppear -= OnCrabAppear;
        onCrabAppear += OnCrabAppear;
    }

    private void OnDestroy()
    {
        onCrabAppear -= OnCrabAppear;
    }
    

    public float replayOffset;

    private void Update()
    {
        if (videoPlayer.isPlaying && !_isReplayEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            var totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.99
#if UNITY_EDITOR
                || ManuallyReplay
#endif
               )
            {
                DOVirtual.Float(0, 0, replayOffset, nullParam => { })
                    .OnComplete(() =>
                    {
                        _isReplayEventTriggered = true;
                        ReplayTriggerEvent();
                    });


                DOVirtual.Float(1, 0, 2f, speed => { videoPlayer.playbackSpeed = speed; }).OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("RelayTriggerReady");
#endif
                    videoPlayer.time = 0;

                    DOVirtual.Float(0, 0, 1f, duration => { })
                        .OnComplete(() =>
                        {
#if UNITY_EDITOR
                            ManuallyReplay = false;
#endif
                            _isReplayEventTriggered = false;
                            _isShaked = false;
                        });
                });
            }
        }
    }


    protected override void Init()
    {
        base.Init();

        DOVirtual.Float(1, 0, 1.1f, speed =>
        {
            videoPlayer.playbackSpeed = speed;
            // 점프메세지 출력 이후 bool값 수정되도록 로직변경 필요할듯 12/26
        });
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
        PlaySpeechBubble(6.5f);
    }

    private void PlaySpeechBubble(float delay)
    {
        DOVirtual.Float(0, 1, delay, _ => { })
            .OnComplete(() =>
            {
                _crabSpeechBubbleUI.DOScale(_speechBubbleDefaultSize, 0.8f)
                    .OnComplete(() =>
                    {
                        videoPlayer.playbackSpeed = 0;
                    });
            });
    }

    private void TurnOffSpeechBubble(float delay)
    {
        DOVirtual.Float(0, 1, delay, _ => { })
            .OnComplete(() => { _crabSpeechBubbleUI.DOScale(Vector3.zero, 0.8f); });
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!isStartButtonClicked) return;
        CrabOnRaySynced();
    }

    private void ReplayTriggerEvent()
    {
        foreach (var ps in particleSystems) ps.Play();

        _currentClickCount = 0;
        isCrabAppearable = true;
        _isOnCrabAppearEventInvoked = false;
        SoundManager.FadeInAndOutSound(particleSystemAudioSource);
        OnReplay?.Invoke();
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


        var node = _xmlDoc.SelectSingleNode($"//StringData[@ID='{"Owl_UI_" + _currentLineIndex}']");
        if (node != null)
        {
            var message = node.Attributes["string"].Value;
            _tmp.text = message;
        }

        _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0.3f));

        DOVirtual.Float(0, 1, 10f, _ => { }).OnComplete(() => { _isNextUIAppearable = true; });


        //duration -> 10
        DOVirtual.Float(0, 1, 1f, _ => { })
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

}