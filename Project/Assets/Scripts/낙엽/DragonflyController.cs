using UnityEngine;

public class DragonflyController : MonoBehaviour
{
    public float movingTimeSec;

    public Transform landingPositionA;
    public Transform landingPositionB;
    public Transform landingPositionC;
    public Transform landingPositionD;
    public Transform landingPositionE;
    public Transform landingPositionF;

    public Transform moveAwayPositionA;
    public Transform moveAwayPositionB;
    public Transform moveAwayPositionC;
    public Transform moveAwayPositionD;
    public Transform moveAwayPositionE;

    /// <summary>
    ///     잠자리가 준비되었는지 체크하는 함수.
    /// </summary>
    public float moveSpeed;


    private readonly int _dragonflyFly = Animator.StringToHash("fly");
    private Animator _animator;
    private Transform _currentLandingPosition;
    private string _dragonflyName;

    private float _elapsedTime;


    private bool _isClicked;


    private Transform[] _landingPositions;

    private int _landPositionIndex;
    private int _moveAwayPositionIndex;
    private Transform[] _moveAwayPositions;
    private float _randomSpeed;
    private readonly string dragonfly = "dragonfly";
    private readonly string upper = "upper";


    private void Awake()
    {
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

    // Update is called once per frame
    private void Update()
    {
        _elapsedTime += Time.deltaTime;


        if (Input.GetMouseButtonDown(0)) CheckClickedAndAnimateDragonfly();

        if (_isClicked)
            MoveUpToDisappear();
        else LandToGround();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(upper))
        {
            _isClicked = false;
            _elapsedTime = 0f;
            _animator.SetBool(_dragonflyFly, false);
        }

        if (other.CompareTag(dragonfly))
        {
            _landPositionIndex = Random.Range(0, 6);
            _isClicked = false;
        }
    }

    /// <summary>
    ///     잠자리를 올리는 함수.
    ///     잠자리 클릭 시, 카메라에 안보이는 위(y축) 방향으로 상승했다가, 지정한 포지션 중 랜덤 한 곳에 떨어지도록 설계
    /// </summary>
    private void MoveUpToDisappear()
    {
        var direction = _moveAwayPositions[_moveAwayPositionIndex].position - transform.position;
        direction.y = 0;
        var rotation = Quaternion.LookRotation(direction);
        
        var t = _elapsedTime / movingTimeSec;
        transform.position = Vector3.Lerp(transform.position, _moveAwayPositions[_moveAwayPositionIndex].position,
            t * moveSpeed * _randomSpeed);
    }

    private void LandToGround()
    {
       
        var t = _elapsedTime / movingTimeSec;
        transform.position = Vector3.Lerp(transform.position, _landingPositions[_landPositionIndex].position,
            t * moveSpeed * _randomSpeed);

        var direction = _landingPositions[_landPositionIndex].position - transform.position;
        direction.y = 0;
        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    private void CheckClickedAndAnimateDragonfly()
    {
        System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name != _dragonflyName) return;
            _elapsedTime = 0f;

            _landPositionIndex = Random.Range(0, 6);
            _moveAwayPositionIndex = Random.Range(0, 5);

            _randomSpeed = Random.Range(0.8f, 2);


            _animator.SetBool(_dragonflyFly, true);

            _currentLandingPosition.position = transform.position;

            _isClicked = true;
        }
    }
}