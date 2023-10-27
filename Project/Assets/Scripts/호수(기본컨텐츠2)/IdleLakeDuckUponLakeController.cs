using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
#if UNITY_EDITOR
#endif

public class IdleLakeDuckUponLakeController : MonoBehaviour
{
    private enum Sound
    {
        Squeak,
        Dive
    }

    [Header("DoTween Parameters")] public float shakeStrength;
    public int vibrato;

    public readonly int IDLE_ANIM = Animator.StringToHash("idle");
    public readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public readonly int SWIM_ANIM = Animator.StringToHash("Swim");
    public readonly int FAST_RUN_ANIM = Animator.StringToHash("FastRun");
    public readonly int IS_ON_LAKE = Animator.StringToHash("IsOnLake");

    public Transform[] jumpSurpPath = new Transform[3];
    private readonly Vector3[] _jumpSurpPathVec = new Vector3[3];

    public Transform[] patrolPath = new Transform[4];
    private readonly Vector3[] _patrolPathVec = new Vector3[4];

    private float _defaultYCoordinate;
    private Animator _animator;
    private float _defaultAnimSpeed;

    private ParticleSystem _particle;

    //더블클릭 방지용으로 콜라이더를 설정하기위한 인스턴스 선언
    private Collider _collider;
    private AudioSource _audioSourceDive;
    private AudioSource _audioSourceSqueak;
    public AudioClip[] audioClips;

    private void Awake()

    {
        _particle = GetComponentInChildren<ParticleSystem>();
        _collider = GetComponent<Collider>();

        var audioSources = GetComponents<AudioSource>();
        _audioSourceDive = audioSources[(int)Sound.Dive];
        _audioSourceSqueak = audioSources[(int)Sound.Squeak];


        //DoPath,Shake등을 사용 후 Y값이 일정하지 않게 변하는 것을 방지하기 위해 사용.
        _defaultYCoordinate = transform.position.y;

        for (var i = 0; i < 4; i++) _patrolPathVec[i] = patrolPath[i].position;

        _animator = GetComponent<Animator>();
        _defaultAnimSpeed = _animator.speed;
        _animator.SetBool(IS_ON_LAKE, true);
    }

    private void Start()
    {
        var trigger = GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(data => { OnClicked(); });
        trigger.triggers.Add(entry);
       
        PatrolAround();
    }

    [FormerlySerializedAs("animationSpeed")] [Range(0, 40)]
    public float increasedAnimationSpeed;

    public float durationOfGoingBackToInitialSpot;

    public float jumpDuration;

    private void OnClicked()
    {
        _collider.enabled = false;
        Lake_SoundManager.PlaySound(_audioSourceSqueak, audioClips[(int)Sound.Squeak]);
        
        DOVirtual.Float(_defaultAnimSpeed, increasedAnimationSpeed, 1.5f, newValue => {
            _animator.speed = newValue;
        }).SetEase(Ease.InOutQuad);
        
        _animator.SetBool(FAST_RUN_ANIM, true);
        for (var i = 0; i < Mathf.Min(jumpSurpPath.Length, _jumpSurpPathVec.Length); i++)
            _jumpSurpPathVec[i] = jumpSurpPath[i].position;
        

        DOTween.Kill(transform);

        var seq = DOTween.Sequence();
        var lookAtTarget = _jumpSurpPathVec[0];
        if (jumpDuration > 2f) 
        {
            seq.Append(transform.DOLookAt(lookAtTarget, 0.8f));
        }
        seq.Append(transform.DOPath(_jumpSurpPathVec, jumpDuration, PathType.CatmullRom).SetEase(Ease.InOutQuad));
        seq.InsertCallback(0.18f, () =>
        {
            _particle.transform.position = _jumpSurpPathVec[0];
            _particle.Play();
            Lake_SoundManager.PlaySound(_audioSourceDive, audioClips[(int)Sound.Dive]);
        });
        seq.OnComplete(ReturnToPatrol);
    }

    private void ReturnToPatrol()
    {
        var directionToLook = _patrolPathVec[0];
        var seq = DOTween.Sequence();
        seq.Append(transform.DOLookAt(directionToLook, 0.8f).OnComplete(() =>
        {
            _animator.SetBool(FAST_RUN_ANIM, false);
            transform
                .DOMove(_patrolPathVec[0], durationOfGoingBackToInitialSpot)
                .OnStart(() => DOVirtual.Float(increasedAnimationSpeed, _defaultAnimSpeed,
                    durationOfGoingBackToInitialSpot,
                    newSpeed => { _animator.speed = newSpeed; }))
                .OnComplete(PatrolAround);
        }));
    }

    public float oneCycleDuration;

    private void PatrolAround()
    {
        #if UNITY_EDITOR
        Debug.Log($"{gameObject.name} 순찰중..");
        #endif
      
        var rotateSequecne = DOTween.Sequence();
        rotateSequecne.Append(transform.DOLookAt(_patrolPathVec[1], 1).OnComplete(() =>
        {
            _collider.enabled = true;
            transform.DOPath(_patrolPathVec, oneCycleDuration, PathType.CatmullRom)
                .SetLookAt(0.01f);
        }));
        
    }
}