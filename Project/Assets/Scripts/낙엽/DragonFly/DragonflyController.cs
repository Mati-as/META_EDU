using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DragonflyController : MonoBehaviour
{
    [Header("DragonFly Movement Settings")] [Space(10f)]
    

    [SerializeField] public Transform landingPositionA;
    [SerializeField] public Transform landingPositionB;
    [SerializeField] public Transform landingPositionC;
    [SerializeField] public Transform landingPositionD;
    [SerializeField] public Transform landingPositionE;
    [SerializeField] public Transform landingPositionF;

    [SerializeField] public Transform moveAwayPositionA;
    [SerializeField] public Transform moveAwayPositionB;
    [SerializeField] public Transform moveAwayPositionC;
    [SerializeField] public Transform moveAwayPositionD;
    [SerializeField] public Transform moveAwayPositionE;

  
    private Transform _currentLandingPosition;
    private string _dragonflyName;
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
        randomNumberForFrequency = Random.Range(minIdlePlayRandomTime, maxIdlePlayRandomTime);
        _currentLandingPosition = transform;

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


        _animator = GetComponent<Animator>();
       
        _dragonflyName = gameObject.name;
#if DEFINE_TEST
        Debug.Log($"gameobject name is {_dragonflyName}");
#endif
    }

    private void Start()
    {
        PlayIdleAnim();
    }

    private bool _initialMoveStart;
    
    // Update is called once per frame
    private void Update()
    {
      
        
        
        if (Input.GetMouseButtonDown(0))
        {
            CheckClickedAndAnimateDragonfly();
        }
        
        if (_initialMoveStart)
        {
            if (_isClicked) MoveUpToDisappear();
            else LandToGround();
        
            PlayIdleAnim();
        }
        else
        {
            LandToGround();
        }
     
        
    }

    private bool isIdleAnimPlayable;
    private int randomNumberForFrequency;

    private void PlayIdleAnim()
    {
        if (_elapsedTimeForIdleAnim > randomNumberForFrequency) isIdleAnimPlayable = true;

        if (isIdleAnimPlayable)
        {
            var randomNumberForAnimation = Random.Range(0, 2);

            if (randomNumberForAnimation%2 == 0)
                _animator.SetTrigger(DRAGONFLY_ANIM_IDLE_A);
            else
                _animator.SetTrigger(DRAGONFLY_ANIM_IDLE_B);


            // initialize..
            _elapsedTimeForIdleAnim = 0f;
           
            randomNumberForFrequency = Random.Range(minIdlePlayRandomTime, maxIdlePlayRandomTime);
            isIdleAnimPlayable = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
        if (other.CompareTag(DRAGONFLY))
        {
            _landPositionIndex = Random.Range(0, 6);
            _isClicked = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(LAND))
        {
#if UNITY_EDITOR
            Debug.Log(" 잠자리 착지");
#endif
            _initialMoveStart = true;
            _elapsedTimeForMoveUp = 0f;
            _animator.SetBool(DRAGONFLY_ANIM_FLY, false);
            _animator.SetTrigger(LANDING);
        }

        if (other.CompareTag(upper))
        {
            _isClicked = false;
        }
    }

    /// <summary>
    ///     잠자리를 올리는 함수.
    ///     잠자리 클릭 시, 카메라에 안보이는 위(y축) 방향으로 상승했다가, 지정한 포지션 중 랜덤 한 곳에 떨어지도록 설계
    /// </summary>
    private void MoveUpToDisappear()
    {
        _elapsedTimeForMoveUp += Time.deltaTime;
        _elapsedForMoveBack = 0f;
        var direction = _moveAwayPositions[_moveAwayPositionIndex].position - transform.position;
        direction.y = 0;
        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
        
        
        transform.position = Vector3.Lerp(transform.position, _moveAwayPositions[_moveAwayPositionIndex].position,
            _elapsedTimeForMoveUp / moveUpTime);
    }

    public float moveUpTime;
    public float moveBackTime;
    private float _elapsedForMoveBack;
    private bool _isInitialLanding;
    public int initialLandingPosition;
    private void LandToGround()
    {
        _elapsedForMoveBack += Time.deltaTime;

       
        //맨 처음에 잠자리 끼리 부딫히지 않도록 하기위한 로직입니다. 추후 좀 더 추상화 진행 해야합니다 9/14/23
        if (!_isInitialLanding)
        {
            transform.position = Vector3.Lerp(transform.position, _landingPositions[initialLandingPosition].position,
                _elapsedForMoveBack / moveBackTime);

            var direction = _landingPositions[initialLandingPosition].position - transform.position;
            direction.y = 0;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
        
        
        else if(_elapsedForMoveBack > 4)
        {
            transform.position = Vector3.Lerp(transform.position, _landingPositions[_landPositionIndex].position,
                _elapsedForMoveBack / moveBackTime);

            var direction = _landingPositions[_landPositionIndex].position - transform.position;
            direction.y = 0;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
        
        
       
    }

    private void CheckClickedAndAnimateDragonfly()
    {
        System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name != _dragonflyName) return;
           
            Debug.Log("잠자리 선택");

            _landPositionIndex = Random.Range(0, 6);
            _moveAwayPositionIndex = Random.Range(0, 5);

            _randomSpeed = Random.Range(0.8f, 2);


            _initialMoveStart = true;
            _animator.SetBool(DRAGONFLY_ANIM_FLY, true);
            _isInitialLanding = true;
            _currentLandingPosition.position = transform.position;

            _isClicked = true;
        }
    }
}