using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;
using Random = System.Random;


public class Desert_BullController : MonoBehaviour
{
    public  readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    public  readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public  readonly int WALK_ANIM = Animator.StringToHash("Walk");
    public  readonly int ATTACK_ANIM = Animator.StringToHash("Attack");

    private Animator _animator;
    private float _defaultAnimationSpeed;
    private float _currentAnimationSpeed;
    private float _elapsedTime;
    
    public Transform[] transforms;
    private Transform _currentTargetTransform;
    public float animSpeed_Rotate;
    public float animSpeed_Walk;
    private float _defaultAnimSpeed;
   
    [Range(0,30)]
    public float randomMoveDurationMax;
    [Range(0,60)]
    public float randomMoveDurationMin;
    [Range(0,30)]
    public float randomEatDurationMax;
    [Range(0,60)]
    public float randomEatDurationMin;
    [Space(10f)]
    [Range(0,15)]
    public float idleDuration;
    
    private float _moveDuration;
    private float _eatDuration;
    private float _idleDuration;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _defaultAnimSpeed = _animator.speed; 
        SetRandomMoveAnimDuration();
        SetRandomEatAnimDuration();
        _idleDuration = idleDuration;
    }

    private void Start()
    {
        DoRandomMove( SetRandomPosition(),_moveDuration);
    }

    private int _currentIndex;
    private int _previousIndex;
    private Vector3 SetRandomPosition()
    {
        while (_currentIndex == _previousIndex)
        {
            _currentIndex = UnityEngine.Random.Range(0, 6);
        }
        
        _previousIndex = _currentIndex;
        
        _currentTargetTransform = transforms[_currentIndex];
        return _currentTargetTransform.position;
    }

    private float SetRandomMoveAnimDuration()=>
        _moveDuration = UnityEngine.Random.Range(randomMoveDurationMin,randomEatDurationMax);
    
    private float SetRandomEatAnimDuration()=>
        _eatDuration = UnityEngine.Random.Range(randomEatDurationMin,randomEatDurationMax);
    

    private void DoRandomMove(Vector3 pos,float dur = 10)
    {
        SetRandomMoveAnimDuration();
        SetRandomEatAnimDuration();
        
        SetAnimation(EAT_ANIM,false);
        SetAnimation(WALK_ANIM);
        
        DOVirtual.
            Float(_defaultAnimSpeed, animSpeed_Rotate, 2f,value =>
            {
                _animator.speed = value;
            });

        

        transform.DOLookAt(pos, 5f)
            .OnComplete(() =>
            {
                DOVirtual.
                    Float(animSpeed_Rotate, animSpeed_Walk, 3f,value =>
                    {
                         _animator.speed = value;
                    });
                
                SetAnimation(WALK_ANIM,false);
                SetAnimation(IDLE_ANIM);
                
                transform.DOMove(pos, dur)
                    .SetDelay(2f)
                    .SetEase(Ease.InOutQuad)
                    .OnStart(() =>
                    {
                        SetAnimation(IDLE_ANIM,false);
                        SetAnimation(WALK_ANIM);
                    })
                    .OnComplete(() =>
                    {
                       
                        #if UNITY_EDITOR
                        Debug.Log("cow is moving..");
                        #endif
                        SetAnimation(WALK_ANIM,false);
                        DOVirtual.
                            Float(animSpeed_Walk, 1f, 2f,value =>
                            {
                                _animator.speed = value;
                            });
                        _eatAnimCoroutine = StartCoroutine(PlayEatAnimCoroutine());
                    });
            });
    
    }


    private Coroutine _eatAnimCoroutine;
    private IEnumerator PlayEatAnimCoroutine(float dur = 10)
    {
        SetAnimation(WALK_ANIM,false);
        SetAnimation(EAT_ANIM);
        _elapsedTime = 0f;
        
        while (true)
        {
            _elapsedTime += Time.deltaTime;
#if UNITY_EDITOR
            Debug.Log($"eating grass..eatingDuration: {_eatDuration}, elapsed Time: {_elapsedTime}");
#endif

            if (_elapsedTime > _eatDuration)
            {
                DoRandomMove(SetRandomPosition(),_moveDuration);
                
                break;
                
            }
            yield return null;
        }

        if(_eatAnimCoroutine!=null) StopCoroutine(_eatAnimCoroutine);
      
       
    }
    private void SetAnimation(int animHash, bool value = true)
    {
        _animator.SetBool(animHash,value);
    }
}
