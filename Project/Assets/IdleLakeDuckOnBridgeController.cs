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
    

    public static readonly int IDLE_ANIM = Animator.StringToHash("idle");
   
    public static readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public static readonly int FAST_RUN_ANIM = Animator.StringToHash("FastRun");
    public static readonly int SWIM_ANIM = Animator.StringToHash("Swim");
    
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
private Vector3[] _duckAwayRouteVector = new Vector3[3];

private Animator _animator;


private void Awake()
{
    _animator = GetComponent<Animator>();
    
    for (int i = 0; i < 3; i++)
    {
        _duckFlyRouteAVector[i] = duckFlyRoute[i].position;
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

private void OnClicked()
{
#if UNITY_EDITOR
    Debug.Log("Ducks on the bridge Clicked!");
#endif
    _animator.SetBool(FAST_RUN_ANIM,true);
    float duration = 2.5f;  // 움직임의 전체 기간 설정
    
    transform.DOPath(_duckFlyRouteAVector, duration, PathType.CatmullRom)
        .SetEase(Ease.InOutQuad)
        .SetOptions(true)
        .OnComplete(() =>
        {
            _animator.SetBool(FAST_RUN_ANIM,false);
            _animator.SetBool(SWIM_ANIM, true);
            transform.DOPath(_duckAwayRouteVector, 15.0f, PathType.CatmullRom)
                .SetDelay(2f)
                .SetOptions(true)
                .SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    _animator.SetBool(SWIM_ANIM, false);
                    
                });
        });
}
}
