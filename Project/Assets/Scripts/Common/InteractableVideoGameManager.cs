using System;
using DG.Tweening;
using Unity.VisualScripting;
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
   [Header("*****Debug Only*****")] public bool DEBUG_manuallyTrigger;

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
 
    [SerializeField] protected float timeStampToStop;
    
    public int maxCount;
    private int _currentClickCount;
    

    // 주로 동물이 나타나거나, 상호작용이 일어나는 시점에 OnRepalyStart 설정.
    public static event Action onReplay;
    private double _totalDuration;

    protected virtual void Start()
    {
        Init();

        _isReplayAfterPausing = true;
     
        
        onReplay -= OnReplay;
        onReplay += OnReplay;
        
        onRewind -= OnRewind;
        onRewind += OnRewind;
    }
    private void OnDestroy()
    {
        onReplay -= OnReplay;
        onRewind -= OnRewind;
    }


    public float rewindDuration;
 

    protected virtual void Update()
    {
        if (videoPlayer.isPlaying && !_isRewindEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            _totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / _totalDuration >= 0.97 || DEBUG_manuallyTrigger)
            {
                DOVirtual.Float(0, 0, 0, _ => { })
                    .OnComplete(() =>
                    {
#if UNITY_EDITOR
                        Debug.Log($"처음부터 다시 재생 영상전체길이: {_totalDuration}");
#endif

                        DEBUG_manuallyTrigger = false;
                        _isRewindEventTriggered = true;
                        
                        RewindAndReplayTriggerEvent();
                    });


                DOVirtual.Float(1, 0, 2f, speed => { videoPlayer.playbackSpeed = speed; }).OnComplete(() =>
                {
                    videoPlayer.time = 0;

                    DOVirtual.Float(0, 0, 1f, _ => { })
                        .OnComplete(() =>
                        {
#if UNITY_EDITOR
                            DEBUG_manuallyTrigger = false;
#endif
                            _isRewindEventTriggered = false;
                            _isShaked = false;
                        });
                });
            }
        }
    }




    public float stopPointSecond;

    protected override void Init()
    {
        base.Init();
    

     
        DOVirtual.Float(1, 0, timeStampToStop, speed =>
        {
            videoPlayer.playbackSpeed = speed;
        });
    }

    /// <summary>
    /// #OnRaySynced는 사용자 입력을 기반으로한 게임상 입력의 핵심함수입니다. 
    /// RaySynchronizer에서 GameManager의 Ray와 처음으로 동기화 하는부분.
    /// 제일 첫번째로 수행되며, OnRaySyncFromGameManager는 **OnRaySynced에 의존합니다.**
    /// OnRaySynced가 동작하지 않는 경우, Ray를 활용한 게임내 로직 또한 동작하지 않아야합니다.  
    /// </summary>
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        OnRaySyncFromGameManager();
    }

   
    /// <summary>
    /// OnRaySynced가 동작하지 않는 경우, Ray를 활용한 게임내 로직 또한 동작하지 않아야합니다.
    /// </summary>
    protected virtual void OnRaySyncFromGameManager()
    {
        if (!_initiailized) return;
        if (!isStartButtonClicked) return;
        if (!_isShaked) transform.DOShakePosition(2.25f, 1f + 0.1f * (_currentClickCount % maxCount), randomness: 90, vibrato: 5)
            .OnComplete(() =>
            {
                transform.DOMove(_defaultPosition, 1f).SetEase(Ease.Linear);
            });;

        _currentClickCount++;

        if (CheckReplayCondition())
        {
            

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

        onRewind?.Invoke();
#if UNITY_EDITOR       
        Debug.Log("onRewind ------------------invoke!");
#endif
    }

    protected virtual void OnRewind()
    {
#if UNITY_EDITOR       
        Debug.Log("Rewind파티클 재생");
#endif
        _particleOnRewind.Stop();
        _particleOnRewind.Play();
        _currentClickCount = 0;
        _isReplayAfterPausing = true;
    
        Managers.Sound.Play(SoundManager.Sound.Effect, rewindParticleAudioPath, 0.1f);
      
    }
}