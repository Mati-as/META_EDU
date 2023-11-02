using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class Desert_SheepController : MonoBehaviour
{
    public readonly int WALK_ANIM = Animator.StringToHash("Walk");
    private Animator _animator;
    private enum MoveDirection { Front, Back }
    private MoveDirection _currentDirection = MoveDirection.Front;
    
    public Vector3 moveFront;
    public Vector3 moveBack;
    
    public Vector3 minPosition;
    public Vector3 maxPosition;
    public float moveDuration;
    public float delayBetweenMoves;

    
    public Transform[] wayPoints;
    private Transform[] _wayPointsCopy;

    public float ranIntervalMin;
    public float ranIntervalMax;
    [Range(0,30)]
    public float ranDelayMin;
    [FormerlySerializedAs("ranDlayMax")] [Range(0,30)]
    public float ranDelayMax;
    private float _elapsed;
    private float _interval;
    private Vector3 _defaultPos;
    private int _counter;
    private void Awake()
    {
        _interval = Random.Range(ranIntervalMin, ranIntervalMax);
        _animator = GetComponent<Animator>();
         // _rb = GetComponent<Rigidbody>();
        _defaultPos = transform.position;
        
        if (wayPoints.Length >= 4)
        {
            _wayPointsCopy = wayPoints.ToArray();
            _wayPointsCopy[3] = transform;
        }
        else
        {
            Debug.LogError("wayPoints의 크기가 4 미만입니다.");
            return;
        }
        _counter++;
        int seed = this.gameObject.GetInstanceID() + DateTime.Now.Millisecond + _counter;
        Random.InitState(seed);
    }

    public float randomAmount;
    private void Start()
    {
        delayBetweenMoves = Random.Range(ranDelayMin, ranDelayMax);
        Invoke(nameof(MoveToRandomPosition), delayBetweenMoves);
        _moveTweenSeq = DOTween.Sequence();
        _wayPointsCopy[3] = transform;
        SetRandomPoint(randomAmount);
    }

    private void SetRandomPoint(float amount)
    {
        for (int i = 0; i < 3; i++)
        {
            _wayPointsCopy[i].position =  wayPoints[i].position + (Vector3.right * Random.Range(-amount,amount) ) + (Vector3.forward * Random.Range(-amount,amount));
        }
       
        // for (int i = 1; i < 4; i++)
        // {
        //     Debug.Log($"{this.gameObject.name}position of {i} :{ _wayPointsCopy[0].position}");
        // }
    }
   
    private void Update()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed > _interval)
        {
            _elapsed = 0;
            _interval = Random.Range(ranIntervalMin, ranIntervalMax);
            WalkAwayAndComeBack();
            
        }
    }

    private int _order;
    private readonly int FRONT = 0;
    private readonly int BACK = 1;
    
    private Sequence _moveTweenSeq;
    
    
    private int _moveCount = 0;
    private Vector3 _moveDirection;
    private Vector3 _randomPosition;
    private Rigidbody _rb;
    private void MoveToRandomPosition()
    {

       
        _animator.SetBool(WALK_ANIM, true);
        
        if (_moveCount % 10 == 0)
        {
            _moveDirection = _currentDirection == MoveDirection.Front ? moveFront : moveBack;
        

            _randomPosition = new Vector3(
                _moveDirection.x + Random.Range(minPosition.x, maxPosition.x),
                _moveDirection.y + Random.Range(minPosition.y, maxPosition.y),
                _moveDirection.z + Random.Range(minPosition.z, maxPosition.z)
            );
        }
        else
        {
            _moveDirection = _defaultPos;
            _randomPosition = Vector3.zero;
        }
       

        _moveTweenSeq?.Kill();
        // Add the look at tween to the sequence
        _moveTweenSeq.Append(transform.DOLookAt(transform.position + _randomPosition, 2f)
            .OnComplete(() =>
            {
                transform.DOMove(transform.position + _randomPosition, moveDuration).OnComplete(() =>
                {
                    _animator.SetBool(WALK_ANIM, false);
                    ToggleDirection();
                    delayBetweenMoves = Random.Range(ranDelayMin, ranDelayMax);
                    _moveTweenSeq.OnComplete(() => Invoke(nameof(MoveToRandomPosition), delayBetweenMoves));
                });
            }));
  
      
    }

    void ToggleDirection()
    {
        Debug.Log($"sheep move direction{(MoveDirection)_currentDirection}");
        _currentDirection = _currentDirection == MoveDirection.Front ? MoveDirection.Back : MoveDirection.Front;
    }
   
    public float loopTime;
    private void WalkAwayAndComeBack()
    {
        SetRandomPoint(randomAmount);
        
        _moveTweenSeq?.Kill();
        _moveTweenSeq = DOTween.Sequence();
        
        _animator.SetBool(WALK_ANIM,true);
        _moveTweenSeq
            .Append(
                transform.DOLookAt(_wayPointsCopy[0].position, 0.8f)
                    .OnComplete(() =>
                    {
                        transform
                            .DOPath(_wayPointsCopy.Select(w => w.position).ToArray(), loopTime, PathType.CatmullRom)
                            .SetEase(Ease.Linear)
                            .SetLookAt(0.01f)
                            .OnStart(() =>
                            {
                                DOVirtual.Float(0.5f, 1.3f, 2.4f, value => { _animator.speed = value; });
                            })
                            .OnComplete(() =>
                            {
                                _animator.SetBool(WALK_ANIM, false);
                                MoveToRandomPosition();
                            });
                    }));
    }
}
