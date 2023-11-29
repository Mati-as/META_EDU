using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Desert_BullController : MonoBehaviour
{
    public  readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    public  readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public  readonly int WALK_ANIM = Animator.StringToHash("Walk");

    private Animator _animator;
    private float _defaultAnimationSpeed;
    private float _currentAnimationSpeed;
    
    private float _elapsedTime;
    
    public float moveSpeed;


    public Transform[] route;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        throw new NotImplementedException();
    }
}
