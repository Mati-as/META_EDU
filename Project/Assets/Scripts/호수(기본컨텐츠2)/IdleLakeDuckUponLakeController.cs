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
        
        AudioSource[] audioSources = GetComponents<AudioSource>();
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

    private void OnClicked()
    {
        _collider.enabled = false;
        DOTween.Kill(transform);

        _animator.SetBool(FAST_RUN_ANIM, true);
        for (var i = 0; i < Mathf.Min(jumpSurpPath.Length, _jumpSurpPathVec.Length); i++)
            _jumpSurpPathVec[i] = jumpSurpPath[i].position;

        Lake_SoundManager.PlaySound(_audioSourceSqueak, audioClips[(int)Sound.Squeak]);
        Transform transform1;

        DOTween
            .Sequence((transform1 = transform).DOPath(_jumpSurpPathVec, 0.28f, PathType.CatmullRom))
            .SetEase(Ease.InOutQuad)
            //사운드 싱크 맞추기위한 InsertCallback.
            .InsertCallback(0.18f, () => { Lake_SoundManager.PlaySound(_audioSourceDive, audioClips[(int)Sound.Dive]); })
            .OnComplete(() =>
            {
                var shakeStrengthVec = new Vector3(shakeStrength, 0, shakeStrength); // This will fix the y axis
                transform.DOShakePosition(0.13f, shakeStrengthVec, vibrato)
                    .OnComplete(() =>
                    {
                        _particle.Play();
                        _animator.SetBool(FAST_RUN_ANIM, false);

                        Vector3 position;
                        transform.DOMove
                            (new Vector3((position = transform.position).x, _defaultYCoordinate, position.z)
                                , 0.55f)
                            .SetEase(Ease.InOutBounce);
                    });
            });

        var directionToLook = _patrolPathVec[0] - transform1.transform.position;
        var lookRotation = Quaternion.LookRotation(directionToLook);
        transform.DORotate(lookRotation.eulerAngles, 0.4f)
            .OnStart(() =>
            {
                DOVirtual.Float(increasedAnimationSpeed, _defaultAnimSpeed, durationOfGoingBackToInitialSpot,
                    newSpeed => { _animator.speed = newSpeed; });
            })
            .SetDelay(1.0f) //회전하고 OnComplete실행까지의 대기시간.
            .OnComplete(() =>
            {
                _animator.speed = increasedAnimationSpeed;
                transform.DOMove(_patrolPathVec[0], durationOfGoingBackToInitialSpot)
                    .OnComplete(() =>
                    {
                        _animator.speed = _defaultAnimSpeed;
                        _collider.enabled = true;
                        PatrolAround();
                    });
            });
    }

    public float oneCycleDuration;

    private void PatrolAround()
    {
        transform.DOPath(_patrolPathVec, oneCycleDuration, PathType.CatmullRom)
            //.SetDelay(0f)
            .SetLookAt(0.01f)
            .SetOptions(true)
            .SetEase(Ease.InOutQuad).OnComplete(PatrolAround);
    }
}