using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif

public class IdleLakeDuckOnBridgeController : MonoBehaviour
{
    enum Sound
    {
        Squeak,
        Dive,
    }
    public readonly int IDLE_ANIM = Animator.StringToHash("idle");
    public readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public readonly int FAST_RUN_ANIM = Animator.StringToHash("FastRun");
    public readonly int SWIM_ANIM = Animator.StringToHash("Swim");

#if UNITY_EDITOR

    [NamedArrayAttribute(new[]
    {
        "Start", "Max_Height", "End"
    })]
#endif
    public Transform[] duckFlyRoute = new Transform[3];
    private readonly Vector3[] _duckFlyRouteAVector = new Vector3[3];


#if UNITY_EDITOR

    [NamedArrayAttribute(new[]
    {
        "Start", "Max_Height", "End"
    })]
#endif
    public Transform[] duckAwayRoute = new Transform[3];
    private readonly Vector3[] _duckAwayRouteVector = new Vector3[4];
    private Animator _animator;
    private bool _isClickedAnimStarted;
    private float _defaultAnimSpeed;
        
    [Header("Sound Effects")]
    private AudioSource _audioSourceDive;
    private AudioSource _audioSourceSqueak;
    public AudioClip[] audioClips;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _defaultAnimSpeed = _animator.speed;
        
        AudioSource[] audioSources = GetComponents<AudioSource>();
        _audioSourceDive = audioSources[(int)Sound.Dive];
        _audioSourceSqueak = audioSources[(int)Sound.Squeak];
        
        for (var i = 0; i < _duckFlyRouteAVector.Length; i++)
        {
            _duckFlyRouteAVector[i] = duckFlyRoute[i].position;
        }
        for (var i = 0; i < _duckAwayRouteVector.Length; i++)
        {
            _duckAwayRouteVector[i] = duckAwayRoute[i].position;
        }
    }

    private void Start()
    {
        var trigger = GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(data => { OnClicked(); });
        trigger.triggers.Add(entry);
    }

    public ParticleSystem waterEffect;
    [Range(0, 40)] public float comingBackDuration;
    [Range(0, 40)] public float increasedAnimationSpeed;

    public float jumpDuration;
    private void OnClicked()
    {
#if UNITY_EDITOR
        Debug.Log("Ducks on the bridge Clicked!");
#endif

        if (!_isClickedAnimStarted)
        {
            Lake_SoundManager.PlaySound(_audioSourceSqueak,audioClips[(int)Sound.Squeak]);
            
            _animator.SetBool(FAST_RUN_ANIM, true);
            _isClickedAnimStarted = true;
            
            
            
                 DOTween
                .Sequence(transform.DOPath(_duckFlyRouteAVector, jumpDuration, PathType.CatmullRom))
                .SetEase(Ease.InOutQuad)
                //사운드 싱크 맞추기위한 InsertCallback.
                .InsertCallback(jumpDuration - 0.35f,
                    () => { Lake_SoundManager.PlaySound(_audioSourceDive, audioClips[(int)Sound.Dive]); })
                .InsertCallback(jumpDuration - 0.1f, () =>
                {
                    waterEffect.transform.position = duckFlyRoute[2].position;
                    waterEffect.Play();
                })
                .OnComplete(() =>
                {
                    _animator.SetBool(FAST_RUN_ANIM, false);
                    _animator.speed += 5;
                    var directionToLook = _duckAwayRouteVector[1] - transform.transform.position;
                    var lookRotation = Quaternion.LookRotation(directionToLook);

                    transform.DORotate(lookRotation.eulerAngles, 1.6f)
                        .OnStart(() =>
                        {
                            DOVirtual.Float(_defaultAnimSpeed, increasedAnimationSpeed, 8f,
                                newSpeed => { _animator.speed = newSpeed; });
                        })
                        .OnComplete(() =>
                        {
                            _animator.SetBool(SWIM_ANIM, true);
                            
                            _animator.speed = increasedAnimationSpeed;
                            DOTween
                                .Sequence()
                                .Append(transform.DOPath(_duckAwayRouteVector, comingBackDuration, PathType.CatmullRom)
                                //.SetDelay(0f)
                                .SetLookAt(0.01f)
                                .SetEase(Ease.InOutQuad))
                                .InsertCallback(7f, // ㄲInsertCall 사용을 위해 애니메이션 시퀀스로 구성.
                                    () => _animator.speed = _defaultAnimSpeed) // 애니메이션 중간에 속도를 기본값으로 설정
                                .OnComplete(() =>
                                {
                                    _animator.SetBool(SWIM_ANIM, false);
                                    _isClickedAnimStarted = false;
                                });
                        });
                });
        }
    }
}