using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// 1.상호작용이 가능한 비디오플레이어 생성용 클래스 입니다.
/// 2.기본적으로 Crab_Class와 구조 동일하나, 변수명을 좀 더 범용성있게 수정하였습니다.
/// 3.비디오 컨텐츠 개발 유형 및 상호작용 여부에 따라 Base_VideoContentPlayer를 사용하거나, 현재 클래스인
///     Interactive_VideoContentPlayer를 사용하면됩니다.
/// 4.예를들어,Crab이나 Owl 같은경우, 유저가 클릭하는 횟수에따라 상호작용하는데 이에 관한 공통적인 로직을 추출해 놓은것입니다.
/// 5.게임에 좀 더 특화해야할 경우 Interactive_VideoContentPlayer를 상속받아 사용합니다.
/// 6.단순 클릭 후, 일정시간 정지 및 재생, 리플레이 로직만 포함된 경우 해당 Interactive_VideoContentPlayer만 사용해도 문제되지 않습니다. 
/// </summary>
public class Base_VideoGameManager : Base_VideoContentPlayer
{
      
#if UNITY_EDITOR
    [Header("*****Debug Only*****")]
    public bool TriggerReplayEvent;

    [FormerlySerializedAs("_particleSystems")]
    [Space(15f)]
#endif

    [Header("Particle and Audio Setting")]
    
    private readonly static string prefix = "Video_";

    private string RESOURCE_PATH;
    private List<ParticleSystem> _particlesOnRewind;

    [SerializeField] 
    private AudioSource _particleSystemAudioSource;
    public static bool _isShaked;

    private bool _isRewindEventTriggered;

    public bool _isReplayAfterPausing { get; private set; }

    public static event Action OnReplay;
    public GameObject UI_Scene;

    private void Awake()
    {
        RESOURCE_PATH = $"게임별분류/비디오컨텐츠/{SceneManager.GetActiveScene().name.Substring(prefix.Length)}/CFX/" +"ReplayParticle";
    }

    private void Start()
    {
        Init();
        
        _isReplayAfterPausing = true;
        Base_EffectManager.onClicked -= OnClicked;
        Base_EffectManager.onClicked += OnClicked;
    }

    [FormerlySerializedAs("replayOffset")] public float rewindDuration;

    private void Update()
    {
        if (videoPlayer.isPlaying && !_isRewindEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            var totalDuration = videoPlayer.length;
           
            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.99
#if UNITY_EDITOR
                || TriggerReplayEvent
#endif
               )
            {
                DOVirtual.Float(0, 0, rewindDuration, nullParam => { })
                    .OnComplete(() =>
                    {
                        _isRewindEventTriggered = true;
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
        CrabEffectManager.onClicked -= OnClicked;
    }

    public float stopPointSecond;
    protected override void Init()
    {
        base.Init();
        UI_Scene.SetActive(false);

        DOVirtual.Float(1, 0, 1.5f, _ => _++).OnComplete(() => { UI_Scene.SetActive(true); });
        DOVirtual.Float(1, 0, 0.5f, speed =>
        {
            videoPlayer.playbackSpeed = speed;
            // 점프메세지 출력 이후 bool값 수정되도록 로직변경 필요할듯 12/26
        });
        
        _particlesOnRewind = new List<ParticleSystem>();
        
        GameObject prefab = Resources.Load<GameObject>(RESOURCE_PATH);
        if(prefab ==null) Debug.LogError($"Particle is null. Resource Path : {RESOURCE_PATH}");
         _particlesOnRewind.Add(prefab.GetComponent<ParticleSystem>());
        
        
    }

    public int clickAccountToReplayAfterPause;
    private int _currentClickCount;
    
    // 주로 동물이 나타나거나, 상호작용이 일어나는 시점에 OnRepalyStart 설정.
    public static event Action OnReplayStart;
    private void OnClicked()
    {
        if (!_initiailized) return;

        
        if (!_isShaked)
        {
            transform.DOShakePosition(2.25f, 1f+(0.1f * _currentClickCount), randomness: 90, vibrato: 5);
        }

        _currentClickCount++;

        if (_currentClickCount > clickAccountToReplayAfterPause)
        {
            OnReplayStart?.Invoke();
            _isReplayAfterPausing = false;
            DOVirtual
                .Float(0, 1, 1f, speed => { videoPlayer.playbackSpeed = speed; })
                .OnComplete(() => { _isShaked = true; });
        }
      
    }
    
    private void ReplayTriggerEvent()
    {
        foreach (var ps in _particlesOnRewind)
        {
            ps.Play();
        }

        _currentClickCount = 0;
        _isReplayAfterPausing = true;
        SoundManager.FadeInAndOutSound(_particleSystemAudioSource);
        OnReplay?.Invoke();
    }
}
