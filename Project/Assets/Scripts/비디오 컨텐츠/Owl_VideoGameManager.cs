using System;
using System.Collections;
using System.Xml;
using DG.Tweening;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Owl_VideoGameManager : InteractableVideoGameManager
{
    private GameObject _owlSpeechBubble;
    private Vector3 _defaultScale;


    private ParticleSystem _psOnReplayAfterPaused;

    private TextAsset _xmlAsset;
    private XmlNode _soundNode;
    private XmlDocument _xmlDoc;
    private TMP_Text _tmp;

    public static bool isOwlUIFinished { get; private set; }
    //Rewind로 처음부터 다시 초기화되어 재생되는지 판단
  
    public static event Action onOwlSpeechBubbleFinished;
    
    private void Awake()
    {
        //UI 관련 로직
        _owlSpeechBubble = GameObject.Find(nameof(_owlSpeechBubble));
        Debug.Assert(_owlSpeechBubble != null);
        _tmp = _owlSpeechBubble.GetComponentInChildren<TMP_Text>();
        _tmp.text = string.Empty;

        isOwlUIFinished = true;
        _defaultScale = _owlSpeechBubble.GetComponent<RectTransform>().localScale;
#if UNITY_EDITOR

        Debug.Log($"부엉이 스케일할당: default Scale: {_defaultScale}");
#endif
    }

    protected override void Init()
    {
        Managers.Sound.Play(SoundManager.Sound.Bgm,
            "Audio/Gamemaster Audio - Fun Casual Sounds/Ω_Bonus_Music/music_candyland", 0.125f);

        isJustRewind = true;
        base.Init();
        UI_Init();
        LoadPrefabs();
    }


    private void UI_Init()
    {
        // XML 파일 로드 (Resources 폴더 안에 있어야 함)
        _xmlAsset = Resources.Load<TextAsset>("게임별분류/비디오컨텐츠/Owl/Video_UI_text_Data");
        _xmlDoc = new XmlDocument();
        _xmlDoc.LoadXml(_xmlAsset.text);

        _owlSpeechBubble.transform.localScale = Vector3.zero;
        BindEvent();
    }

    private Coroutine _typingCoroutine;
    private readonly float _textPrintingSpeed = 0.03f;
    private int currentLineIndex;

    public int nextUIApperableWaitTime; //10

    private void PlayNextMessageAnim(int currentIndex)
    {
     
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);


        var node = _xmlDoc.SelectSingleNode($"//StringData[@ID='{"Owl_UI_" + currentLineIndex}']");
        if (node != null)
        {
            var message = node.Attributes["string"].Value;
            _tmp.text = message;
        }

        _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0.3f));

      
        
        //duration -> 10
        DOVirtual.Float(0, 1, nextUIApperableWaitTime, _ => { })
            .OnComplete(() => { _isNextUIAppearable = true; });

        if (currentLineIndex >= 2)
            DOVirtual.Float(0, 1, 0.5f, _ => { })
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log($"부엉이 대사 끝. 대사 번호: {currentIndex}");
#endif
                  
                    
                    onOwlSpeechBubbleFinished?.Invoke();
                   
                });
    }



    private bool _isReplayable;
    private bool _isRewindable;

    public void BindEvent()
    {
        onReplay -= UIOnReplay;
        onReplay += UIOnReplay;

        Owl_LeavesMaterialController.OnAllLeavesDarkend -= OnAllLeaveDarkend;
        Owl_LeavesMaterialController.OnAllLeavesDarkend += OnAllLeaveDarkend;

        onOwlSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
        onOwlSpeechBubbleFinished += OnOwlSpeechBubbleFinished;
    }
    private void OnDestroy()
    {
        Owl_LeavesMaterialController.OnAllLeavesDarkend -= OnAllLeaveDarkend;
        onReplay -= UIOnReplay;
        onOwlSpeechBubbleFinished -= OnOwlSpeechBubbleFinished;
    }


    private void OnAllLeaveDarkend()
    {
        if (!isStartButtonClicked) return;
        _isReplayable = true;
        OnRaySyncFromGameManager();
        _isReplayable = false;
    }

    protected override bool CheckReplayCondition()
    {
        return _isReplayable;
    }


    protected override void OnRaySynced()
    { 
        base.OnRaySynced();
     
        if (_isNextUIAppearable)
        {
            if (currentLineIndex >= 2) return;
            _isNextUIAppearable = false;
            currentLineIndex++;
            PlayNextMessageAnim(currentLineIndex);
        }
        
    }

    private void OnOwlSpeechBubbleFinished()
    {
        if (!isJustRewind) return;

       
        DOVirtual.Float(0, 1, 0.5f, _ => { })
            .OnComplete(() =>
            {
                _owlSpeechBubble.transform
                    .DOScale(Vector3.zero, 2f)
                    .SetEase(Ease.OutBounce)
                    .OnStart(() =>
                    {
                        isJustRewind = false;
                        StopCoroutine(_typingCoroutine);
                        //영상 다시 중지
                        DOVirtual.Float(1, 0, 3f, _ => { }).OnComplete(() =>
                        {
                            DOVirtual.Float(1, 0, 3f, speed => { videoPlayer.playbackSpeed = speed; });
                        });
                    })
                    .OnComplete(() =>
                    {
                        isOwlUIFinished = true;
                        //_isRewind가 false라는 것은, 두번째 나뭇잎이 다시 밝아지기위해 화면이 멈추는 경우를 말합니다. 
                    }).SetDelay(1f);
            });
    }


    private bool _isNextUIAppearable;

    private void UIOnReplay()
    {
        if (!isJustRewind) return;

        //start Delay
        DOVirtual.Float(0, 1, 6f, _ => { })
            .OnComplete(() =>
            {
                _owlSpeechBubble.transform
                    .DOScale(_defaultScale, 2f)
                    .SetEase(Ease.OutBounce)
                    .OnStart(() =>
                    {
                        isOwlUIFinished = false;
                        //영상 다시 중지
                        DOVirtual.Float(1, 0, 1f,
                                speed =>
                                {

                                    videoPlayer.playbackSpeed = speed;
                                }).SetDelay(0.5f)
                            .OnComplete(() => { PlayNextMessageAnim(currentLineIndex); });
                    })
                    .SetDelay(5f);
            });
    }

    protected override void Update()
    {
        if (videoPlayer.isPlaying && !_isRewindEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            var totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.92f
#if UNITY_EDITOR
                || DEBUG_manuallyTrigger
#endif
               )
            {
                DOVirtual.Float(0, 0, rewindDuration, nullParam => { })
                    .OnComplete(() =>
                    {
                        if (!_isRewindEventTriggered)
                        {
                            _isRewindEventTriggered = true;
                            RewindAndReplayTriggerEvent();
                        }
                    });


                DOVirtual.Float(1, 0, 2f, speed => { videoPlayer.playbackSpeed = speed; }).OnComplete(() =>
                {
                    videoPlayer.time = 0;

                    DOVirtual.Float(0, 0, 1f, duration => { })
                        .OnComplete(() =>
                        {
#if UNITY_EDITOR
                            DEBUG_manuallyTrigger = false;
#endif
                            _isRewindEventTriggered = false;
                            _isShaked = false;
                        });
                });
                _isRewindEventTriggered = false;
            }
        }
    }



    protected override void OnReplay()
    {
        //처음 영상시작, 혹은 처음부터 영상다시재생되는경우 
        if (isJustRewind)
        {
            base.OnReplay();

            //start delay
            DOVirtual.Float(0, 1, 1.25f, _ =>{})
                .OnComplete(() =>
            {
                _psOnReplayAfterPaused.transform.gameObject.SetActive(true);
                _psOnReplayAfterPaused.Play();

#if UNITY_EDITOR       
                Debug.Log("파티클 재생");
#endif
                //Particle Duration
                DOVirtual.Float(0, 1, 3.5f, _ =>{})
                    .OnComplete(() => { _psOnReplayAfterPaused.Stop(); });
            });
        }
        // 부엉이 UI가 끝나고 다시 풀이 밝아지며 재생되는 상황의 경우
        else
        {
            DOVirtual
                .Float(0, 1, 1f, speed => { videoPlayer.playbackSpeed = speed; })
                .OnComplete(() => { _isShaked = true; });
                
        }
    }
    public static bool isJustRewind { get;  private set; }

   
    protected override void OnRewind()
    {
        base.OnRewind();
        _isShaked = false;
        transform.DOShakePosition(3.0f, 2f + 0.1f, randomness: 90, vibrato: 6).OnComplete(() =>
        {
            transform.DOMove(_defaultPosition, 1f).SetEase(Ease.Linear);
        });
    }
    
    protected override void  RewindAndReplayTriggerEvent()
    {
        isJustRewind = true;
        _isNextUIAppearable = false;
        _tmp.text = string.Empty;
        currentLineIndex = 0;
        base.RewindAndReplayTriggerEvent();
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

    private void LoadPrefabs()
    {
        rewindPsPrefabPath = "게임별분류/비디오컨텐츠/Owl/CFX/CFX_OnRewind";
        rewindParticleAudioPath = "Audio/비디오 컨텐츠/Owl/Leaves";


        var prefabRewind = Resources.Load<GameObject>(rewindPsPrefabPath);

        if (prefabRewind == null)
        {
            Debug.LogError($"Particle is null. Resource Path : {rewindPsPrefabPath}");
        }
        else
        {
            var rewindPsPosition = GameObject.Find("Position_Rewind_Particle");
            _particleOnRewind = Instantiate(prefabRewind, rewindPsPosition.transform.position,
                rewindPsPosition.transform.rotation).GetComponent<ParticleSystem>();
        }

        var replayPsPrefabPath = "게임별분류/비디오컨텐츠/Owl/CFX/CFX_OnReplayAfterPaused";
        var prefabReplayPs = Resources.Load<GameObject>(replayPsPrefabPath);

        if (prefabReplayPs == null)
        {
            Debug.LogError($"Particle is null. Resource Path : {replayPsPrefabPath}");
        }
        else
        {
            var replayPosition = GameObject.Find("Position_Replay_Particle");
            _psOnReplayAfterPaused = Instantiate(prefabReplayPs, replayPosition.transform.position,
                replayPosition.transform.rotation).GetComponent<ParticleSystem>();
            _psOnReplayAfterPaused.Stop();
        }
    }

}