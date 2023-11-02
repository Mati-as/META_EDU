using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Desert_GoatController : MonoBehaviour
{
    public readonly int WALK_ANIM = Animator.StringToHash("Walk");
    private Animator _animator;

    private enum MoveDirection
    {
        Front,
        Back
    }

    private MoveDirection _currentDirection = MoveDirection.Front;

    public Vector3 moveFront;
    public Vector3 moveBack;

    public Vector3 minPosition;
    public Vector3 maxPosition;
    public float moveDuration;
    public float delayBetweenMoves;

    public Transform[] wayPoints;
    private Transform[] _wayPointsCopy;


    [Header("Walk Away Params")] public float walAwayloopTime;
    public float walkAwayRanIntervalMin;
    public float walkAwayRanIntervalMax;
    private float _elapsedForWalkingAway;
    private float _walkAwayInterval;


    [Header("Move Sideways Params")] public float moveSidewayDuration;
    public float moveSidewaysRanIntervalMin;
    public float moveSidewaysRanIntervalMax;
    public Transform targetSpot;
    private Vector3 _defaultPosition;
    private float _elapsedForMoveSideways;
    private float _moveSidewaysInterval;


    //Tweening Parameters
    private Sequence _walkAwaySeq;
    private Sequence _moveSidewaysSeq;
    private Sequence _moveRandomlySeq;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _defaultPosition = transform.position;

        if (wayPoints.Length >= 4)
        {
            _wayPointsCopy = wayPoints.ToArray();
            _wayPointsCopy[3] = transform;
        }
        else
        {
            Debug.LogError("wayPoints의 크기가 4 미만입니다.");
        }
    }

    private void Start()
    {
        _walkAwayInterval = Random.Range(walkAwayRanIntervalMin, walkAwayRanIntervalMax);
        _moveSidewaysInterval = Random.Range(moveSidewaysRanIntervalMin, moveSidewaysRanIntervalMax);

        Invoke(nameof(MoveToRandomPosition), delayBetweenMoves);


        _moveRandomlySeq = DOTween.Sequence();
        _moveSidewaysSeq = DOTween.Sequence();
        _walkAwaySeq = DOTween.Sequence();
    }


    private bool _isWalkingAway;
    private bool _isMovingSideways;
    private bool _isOnDefaultPosition;

    private void Update()
    {
        _elapsedForMoveSideways += Time.deltaTime;
        _elapsedForWalkingAway += Time.deltaTime;

        if (_elapsedForWalkingAway > _walkAwayInterval && !_isWalkingAway)
        {
            _isWalkingAway = true;
            _elapsedForWalkingAway = 0f;
            _walkAwayInterval = Random.Range(walkAwayRanIntervalMin, walkAwayRanIntervalMax);

            WalkAwayAndComeBack();
        }

        if (_elapsedForMoveSideways > _moveSidewaysInterval && !_isMovingSideways && !_isWalkingAway)
        {
            _elapsedForMoveSideways = 0f;
            _isMovingSideways = true;
            _moveSidewaysInterval = Random.Range(moveSidewaysRanIntervalMin, moveSidewaysRanIntervalMax);

            if (_isOnDefaultPosition)
            {
                MoveSideways(targetSpot.position);
                _isOnDefaultPosition = false;
            }
            else
            {
                MoveSideways(_defaultPosition);
                _isOnDefaultPosition = true;
            }
        }
    }

    private int _order;
    private readonly int FRONT = 0;
    private readonly int BACK = 1;

    private void MoveToRandomPosition()
    {
        if (_isMovingSideways && _isWalkingAway)
        {
            _moveRandomlySeq?.Pause();
            return;
        }


        _animator.SetBool(WALK_ANIM, true);

        var moveDirection = _currentDirection == MoveDirection.Front ? moveFront : moveBack;


        var randomPosition = new Vector3(
            moveDirection.x + Random.Range(minPosition.x, maxPosition.x),
            moveDirection.y + Random.Range(minPosition.y, maxPosition.y),
            moveDirection.z + Random.Range(minPosition.z, maxPosition.z)
        );


        _moveRandomlySeq.Append(transform
            .DOLookAt(transform.position + randomPosition, 0.5f)
            //.OnStart(() => _animator.SetBool(WALK_ANIM, false))
            .OnComplete(() =>
            {
                transform.DOMove(transform.position + randomPosition, moveDuration).OnComplete(() =>
                {
                    _animator.SetBool(WALK_ANIM, false);

                    ToggleDirection();
                    Invoke(nameof(MoveToRandomPosition), delayBetweenMoves);
                });
            }));
    }

    private void WalkAwayAndComeBack()
    {
        _moveRandomlySeq.Pause();
        _moveSidewaysSeq.Pause();


        _animator.SetBool(WALK_ANIM, true);

        _walkAwaySeq
            .Append(
                transform.DOLookAt(_wayPointsCopy[0].position, 0.8f)
                    .OnComplete(() =>
                    {
                        transform
                            .DOPath(_wayPointsCopy.Select(w => w.position).ToArray(), walAwayloopTime,
                                PathType.CatmullRom)
                            .SetEase(Ease.Linear)
                            .SetSpeedBased()
                            .SetLookAt(0.01f)
                            .OnPlay(() => { _elapsedForWalkingAway = 0f; })
                            .OnStart(() =>
                            {
                                DOVirtual.Float(0.5f, 1.3f, 2.4f, value => { _animator.speed = value; });
                            })
                            .OnComplete(() =>
                            {
                                _isWalkingAway = false;
                                _animator.SetBool(WALK_ANIM, false);
                                _moveRandomlySeq.Play();
                                _moveSidewaysSeq.Play();
                                //MoveToRandomPosition();
                            });
                    }));
    }


  
  
    
    private void MoveSideways(Vector3 target)
    {
        _moveRandomlySeq.Pause();
        _animator.SetBool(WALK_ANIM, true);

        _moveSidewaysSeq.Append(
            transform.DOLookAt(target, 1.54f)
                .OnComplete(() =>
                {
                    transform.DOMove(target, moveSidewayDuration)
                        .SetEase(Ease.InOutQuad)
                        .OnStart(() =>
                        {
                            {
                                _animator.SetBool(WALK_ANIM, true);
                                DOVirtual.Float(1f, 1f, 2.4f, 
                                    value => { _animator.speed = value; });
                                Invoke(nameof(StartVirtualFloat),3f);
                            }
                        })
                        
                        .OnPlay(() =>
                        {

                            Debug.Log("아직 _elapsedForMoveSideways = 0 ");
                            _elapsedForMoveSideways = 0f;
                        })
                        .OnComplete(() =>
                        {
                            _animator.SetBool(WALK_ANIM, false);
                            _isMovingSideways = false;
                            _moveRandomlySeq.Play();
                        });
                }));
    }

    private void VirtualFloat(Animator animator, float from, float to, float duration )
    {
        DOVirtual.Float(from, to, duration, 
            value => { animator.speed = value; });
    }

    //wrapper 
    private void StartVirtualFloat()
    {
        VirtualFloat(_animator, 1.5f, 1.0f, 1f);
    }
    
    private void ToggleDirection()
    {
        Debug.Log($"sheep move direction{_currentDirection}");
        _currentDirection = _currentDirection == MoveDirection.Front ? MoveDirection.Back : MoveDirection.Front;
    }
}