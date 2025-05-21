using DG.Tweening;
using UnityEngine;

public class DragonflyController : MonoBehaviour, Lake_IAnimalBehavior, IOnClicked
{
    [Header("DragonFly Movement Settings")] [Space(10f)] [SerializeField]
    private Transform landingPositionA;

    [SerializeField] private Transform landingPositionB;
    [SerializeField] private Transform landingPositionC;
    [SerializeField] private Transform landingPositionD;
    [SerializeField] private Transform landingPositionE;
    [SerializeField] private Transform landingPositionF;

    private static readonly bool[] _isOnThePlace = new bool[6];

    [SerializeField] private Transform moveAwayPositionA;
    [SerializeField] private Transform moveAwayPositionB;
    [SerializeField] private Transform moveAwayPositionC;
    [SerializeField] private Transform moveAwayPositionD;
    [SerializeField] private Transform moveAwayPositionE;

    [SerializeField] private Transform nearCamPosA;
    [SerializeField] private Transform nearCamPosB;
    

  
    private float _elapsedTimeForMoveUp;
    private bool _isClicked;


    private Transform[] _landingPositions;
    private int _landPositionIndex;

    private int _moveAwayPositionIndex;
    private Transform[] _moveAwayPositions;

    private float _randomSpeed;
    private readonly string DRAGONFLY = "dragonfly";
    private readonly string upper = "upper";
    private readonly string LAND = "land";


    [Header("DragonFly Animation Settings")] [Space(10f)] [Range(0, 20)]
    public int minIdlePlayRandomTime;

    [Range(0, 20)] public int maxIdlePlayRandomTime;

    private float _elapsedTimeForIdleAnim;

    private readonly int DRAGONFLY_ANIM_FLY = Animator.StringToHash("fly");
    private readonly int LANDING = Animator.StringToHash("Landing");
    private readonly int DRAGONFLY_ANIM_IDLE_A = Animator.StringToHash("Idle_a");
    private readonly int DRAGONFLY_ANIM_IDLE_B = Animator.StringToHash("Idle_b");


    private Animator _animator;


    private void Awake()
    {
        _randomNumberForFrequency = Random.Range(minIdlePlayRandomTime, maxIdlePlayRandomTime);

        _landingPositions = new Transform[6];
        _moveAwayPositions = new Transform[5];

        _landingPositions[0] = landingPositionA;
        _landingPositions[1] = landingPositionB;
        _landingPositions[2] = landingPositionC;
        _landingPositions[3] = landingPositionD;
        _landingPositions[4] = landingPositionE;
        _landingPositions[5] = landingPositionF;

        _moveAwayPositions[0] = moveAwayPositionA;
        _moveAwayPositions[1] = moveAwayPositionB;
        _moveAwayPositions[2] = moveAwayPositionC;
        _moveAwayPositions[3] = moveAwayPositionD;
        _moveAwayPositions[4] = moveAwayPositionE;

        _isOnThePlace[0] = true;
        _isOnThePlace[3] = true;
        _isOnThePlace[5] = true;

        _animator = GetComponent<Animator>();
        
    }

    private void Start()
    {
        PlayIdleAnim();
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private bool _initialMoveStart;
    private float _elapsed;
    public float _dragonFlySoundInterval;
    private AudioSource _audioSource;

    private void Update()
    {
   
        _elapsed += Time.deltaTime;
        if (_elapsed > _dragonFlySoundInterval)
        {
            _elapsed = -20f;

            _dragonFlySoundInterval = Random.Range(-7, 7) + _dragonFlySoundInterval; //Magic Number추후 필요 시 수정. 11/7/23
            SoundManager.FadeInAndOutSound(_audioSource, fadeWaitTime: 20f);
        }

        PlayIdleAnim();
    }


    private bool _isIdleAnimPlayable;
    private int _randomNumberForFrequency;

    private void PlayIdleAnim()
    {
        if (_elapsedTimeForIdleAnim > _randomNumberForFrequency) _isIdleAnimPlayable = true;

        if (_isIdleAnimPlayable)
        {
            var randomNumberForAnimation = Random.Range(0, 2);

            if (randomNumberForAnimation % 2 == 0)
                _animator.SetTrigger(DRAGONFLY_ANIM_IDLE_A);
            else
                _animator.SetTrigger(DRAGONFLY_ANIM_IDLE_B);

            // initialize..
            _elapsedTimeForIdleAnim = 0f;
            _randomNumberForFrequency = Random.Range(minIdlePlayRandomTime, maxIdlePlayRandomTime);
            _isIdleAnimPlayable = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(LAND))
        {
#if UNITY_EDITOR
            Debug.Log(" 잠자리 착지");
#endif
            _animator.SetBool(DRAGONFLY_ANIM_FLY, false);
            _animator.SetTrigger(LANDING);
        }
    }


    [SerializeField] private int _currentIndex;

    private void MoveAway()
    {
        _moveAwayPositionIndex = Random.Range(0, 5);
        
        transform.DOLookAt(_moveAwayPositions[_moveAwayPositionIndex].position, 1f)
            .OnStart(() => { })
            .OnComplete(() =>
            {
                Vector3[] randomPath = new Vector3[] 
                {
                    transform.position,
                    Random.Range(0,10) > 5? nearCamPosA.position : nearCamPosB.position,
                    _moveAwayPositions[_moveAwayPositionIndex].position
                };
                transform.DOPath(randomPath, moveAwayDuration)
                    .OnComplete(
                        () => { LandToGround(); });
            });
    }


    public float moveAwayDuration;
    public float moveBackDuration;
    private float _elapsedForMoveBack;
    private bool _isInitialLanding;
    public int initialLandingPosition;

    private void LandToGround()
    {
        transform.DOLookAt(_landingPositions[initialLandingPosition].position, 2f)
            .OnComplete(() =>
            {
                transform.DOMove(_landingPositions[_landPositionIndex].position, moveBackDuration).OnComplete(() =>
                {
                    _isClicked = false;
                    PlayIdleAnim();
                });
            });
    }

    public void OnClicked()
    {
#if UNITY_EDITOR
        Debug.Log("OnClicked진입");
#endif
        if (!_isClicked)
        {
            _isClicked = true;
            Debug.Log("잠자리 선택");


            _landPositionIndex = Random.Range(0, 6);
            
            _randomSpeed = Random.Range(0.8f, 2);

            while (_isOnThePlace[_landPositionIndex]) _landPositionIndex = Random.Range(0, 6);

            _isOnThePlace[_landPositionIndex] = true;
            _isOnThePlace[_currentIndex] = false;
            _currentIndex = _landPositionIndex;
         

            _animator.SetBool(DRAGONFLY_ANIM_FLY, true);
       

            MoveAway();
        }
    }
}