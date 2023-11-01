
using System;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

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


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Invoke(nameof(MoveToRandomPosition), delayBetweenMoves);
    }

    private int _order;
    private readonly int FRONT = 0;
    private readonly int BACK = 1;

    private void MoveToRandomPosition()
    { 
        _animator.SetBool(WALK_ANIM, true);
        
        var moveDirection = _currentDirection == MoveDirection.Front ? moveFront : moveBack;
        

        var randomPosition = new Vector3(
            moveDirection.x + Random.Range(minPosition.x, maxPosition.x),
            moveDirection.y + Random.Range(minPosition.y, maxPosition.y),
            moveDirection.z + Random.Range(minPosition.z, maxPosition.z)
        );

        transform
            .DOLookAt(transform.position + randomPosition, 2f)
            //.OnStart(() => _animator.SetBool(WALK_ANIM, false))
            .OnComplete(() =>
            {
                transform.DOMove(transform.position + randomPosition, moveDuration).OnComplete(() =>
                {
                    _animator.SetBool(WALK_ANIM, false);

                    ToggleDirection();
                    Invoke(nameof(MoveToRandomPosition), delayBetweenMoves);
                });
            });
    }

    void ToggleDirection()
    {
        Debug.Log($"sheep move direction{(MoveDirection)_currentDirection}");
        _currentDirection = _currentDirection == MoveDirection.Front ? MoveDirection.Back : MoveDirection.Front;
    }
}
