using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using Unity.VisualScripting;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif
public class IdleLakeDuckOnBridgeController : MonoBehaviour
{
    public  readonly int IDLE_ANIM = Animator.StringToHash("idle");
    public  readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public  readonly int FAST_RUN_ANIM = Animator.StringToHash("FastRun");
    public  readonly int SWIM_ANIM = Animator.StringToHash("Swim");
    
#if UNITY_EDITOR

[NamedArrayAttribute(new[]
{
    "Start", "Max_Height","End"
})]
#endif
public Transform[] duckFlyRoute = new Transform[3];

private Vector3[] _duckFlyRouteAVector = new Vector3[3];


#if UNITY_EDITOR

[NamedArrayAttribute(new[]
{
    "Start", "Max_Height","End"
})]
#endif
public Transform[] duckAwayRoute = new Transform[3];
private Vector3[] _duckAwayRouteVector = new Vector3[4];

private Animator _animator;
private bool _isClickedAnimStarted;
private float _defaultAnimSpeed;


private void Awake()
{
    _animator = GetComponent<Animator>();
    _defaultAnimSpeed = _animator.speed;
    for (int i = 0; i < 3; i++)
    {
        _duckFlyRouteAVector[i] = duckFlyRoute[i].position;
      
    }
    for (int i = 0; i < 4; i++)
    {
        _duckAwayRouteVector[i] = duckAwayRoute[i].position;
      
    }
   
}

private void Start()
{
    var trigger = GetComponent<EventTrigger>();
    var entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerClick;
    entry.callback.AddListener(data => { OnClicked(); });
    trigger.triggers.Add(entry);
}

public ParticleSystem waterEffect;
[Range(0,40)]
public float comingBackDuration;
[Range(0,40)]
public float increasedAnimationSpeed;
private void OnClicked()
{
#if UNITY_EDITOR
    Debug.Log("Ducks on the bridge Clicked!");
#endif

    if (!_isClickedAnimStarted)
    {
        // FSM에서 직접 다루거나 별도의 빠른재생용 애니메이션 로직 설정하지 않고, animator의 Speed로직을 설정합니다.


        _animator.SetBool(FAST_RUN_ANIM, true);
        _isClickedAnimStarted = true;
        var duration = 0.9f; // 움직임의 전체 기간 설정

        transform.DOPath(_duckFlyRouteAVector, duration, PathType.CatmullRom)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                waterEffect.transform.position = duckFlyRoute[2].position;
                waterEffect.Play();
                _animator.SetBool(FAST_RUN_ANIM, false);
                _animator.speed += 5;
                var directionToLook = _duckAwayRouteVector[1] - transform.transform.position;
                var lookRotation = Quaternion.LookRotation(directionToLook);
             
                transform.DORotate(lookRotation.eulerAngles, 1.6f)
                    .OnComplete(() =>
                    {   _animator.speed = increasedAnimationSpeed;
                        DOTween.Sequence()
                            .Append(transform.DOPath(_duckAwayRouteVector, comingBackDuration, PathType.CatmullRom)
                                .SetDelay(0f)
                                .SetLookAt(0.01f)
                                .SetEase(Ease.InOutQuad))
                            .InsertCallback(7f, // InsertCall 사용을 위해 애니메이션 시퀀스로 구성.
                                () => _animator.speed = _defaultAnimSpeed) // 애니메이션 중간에 속도를 기본값으로 설정
                            .OnComplete(() =>
                            {
                                _animator.SetBool(SWIM_ANIM, false);
                                _isClickedAnimStarted = false;
                            });
                    });
            });
    }
  
    
   
}
}
