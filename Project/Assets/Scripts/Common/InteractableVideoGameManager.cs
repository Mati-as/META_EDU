using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     1.상호작용이 가능한 비디오플레이어 생성용 클래스 입니다.
///     2.기본적으로 Crab_Class와 구조 동일하나, 변수명을 좀 더 범용성있게 수정하였습니다.
///     3.비디오 컨텐츠 개발 유형 및 상호작용 여부에 따라 Base_VideoContentPlayer를 사용하거나, 현재 클래스인
///     Interactive_VideoContentPlayer를 사용하면됩니다.
///     4.예를들어,Crab이나 Owl 같은경우, 유저가 클릭하는 횟수에따라 상호작용하는데 이에 관한 공통적인 로직을 추출해 놓은것입니다.
///     5.게임에 좀 더 특화해야할 경우 Interactive_VideoContentPlayer를 상속받아 사용합니다.
///     6.단순 클릭 후, 일정시간 정지 및 재생, 리플레이 로직만 포함된 경우 해당 Interactive_VideoContentPlayer만 사용해도 문제되지 않습니다.
/// </summary>
public abstract class InteractableVideoGameManager : Video_GameManager
{
#if UNITY_EDITOR
    [Header("*****Debug Only*****")] public bool TriggerReplayEvent;

    [FormerlySerializedAs("_particleSystems")]
    [Space(15f)]
#endif

    [Header("Particle and Audio Setting")]
    private static readonly string prefix = "Video_";

    protected string rewindPsPrefabPath;
    protected string rewindParticleAudioPath;
    [SerializeField] protected ParticleSystem _particleOnRewind;


    public static bool _isShaked;

    protected bool _isRewindEventTriggered;

    public bool _isReplayAfterPausing { get; private set; }

    public static event Action onRewind;
    public GameObject UI_Scene;
    [SerializeField] protected float timeStampToStop;
    
    public int maxCount;
    private int _currentClickCount;
    

    // 주로 동물이 나타나거나, 상호작용이 일어나는 시점에 OnRepalyStart 설정.
    public static event Action onReplay;

    private void Start()
    {
        Init();

        _isReplayAfterPausing = true;

        onReplay -= OnReplay;
        onReplay += OnReplay;
        
        onRewind -= OnRewind;
        onRewind += OnRewind;
    }


    public float rewindDuration;

    protected virtual void Update()
    {
        if (videoPlayer.isPlaying && !_isRewindEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            var totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.94
#if UNITY_EDITOR
                || TriggerReplayEvent
#endif
               )
            {
                DOVirtual.Float(0, 0, rewindDuration, nullParam => { })
                    .OnComplete(() =>
                    {
                        _isRewindEventTriggered = true;
                        RewindAndReplayTriggerEvent();
                    });


                DOVirtual.Float(1, 0, 2f, speed => { videoPlayer.playbackSpeed = speed; }).OnComplete(() =>
                {
                    videoPlayer.time = 0;

                    DOVirtual.Float(0, 0, 1f, duration => { })
                        .OnComplete(() =>
                        {
#if UNITY_EDITOR
                            TriggerReplayEvent = false;
#endif
                            _isRewindEventTriggered = false;
                            _isShaked = false;
                        });
                });
            }
        }
    }


    private void OnDestroy()
    {
        onReplay -= OnReplay;
        onRewind -= OnRewind;
    }


    public float stopPointSecond;

    protected override void Init()
    {
        base.Init();
        UI_Scene.SetActive(false);

        DOVirtual.Float(1, 0, 1.5f, _ => _++).OnComplete(() => { UI_Scene.SetActive(true); });
        DOVirtual.Float(1, 0, timeStampToStop, speed =>
        {
            videoPlayer.playbackSpeed = speed;
            // 점프메세지 출력 이후 bool값 수정되도록 로직변경 필요할듯 12/26
        });
    }

    protected override void OnRaySynced()
    {
        OnRaySyncFromGameManager();
    }


    protected virtual void OnRaySyncFromGameManager()
    {
        if (!_initiailized) return;
        if (!_isShaked) transform.DOShakePosition(2.25f, 1f + 0.1f * _currentClickCount, randomness: 90, vibrato: 5);

        _currentClickCount++;

        if (CheckReplayCondition())
        {
            
#if UNITY_EDITOR       
            Debug.Log("OnReplayAfterPasued Event Invoked------------------------");
#endif
            onReplay?.Invoke();
        }
    }

    protected virtual void OnReplay()
    {
        _isReplayAfterPausing = false;
        DOVirtual
            .Float(0, 1, 1f, speed => { videoPlayer.playbackSpeed = speed; })
            .OnComplete(() => { _isShaked = true; });
    }

    protected virtual bool CheckReplayCondition()
    {
        return false;
    }

    protected virtual void RewindAndReplayTriggerEvent()
    {
#if UNITY_EDITOR       
        Debug.Log("OnRewind Event Invoked------------------------");
#endif
        onRewind?.Invoke();
    }

    protected virtual void OnRewind()
    {
        _particleOnRewind.Play();
        _currentClickCount = 0;
        _isReplayAfterPausing = true;
        
        Managers.Sound.Play(SoundManager.Sound.Effect, rewindParticleAudioPath, 0.1f);
      
    }
}