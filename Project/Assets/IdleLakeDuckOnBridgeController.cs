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

private void Awake()
{
    _animator = GetComponent<Animator>();
    
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

private void OnClicked()
{
#if UNITY_EDITOR
    Debug.Log("Ducks on the bridge Clicked!");
#endif

    if (!_isClickedAnimStarted)
    {
        _isClickedAnimStarted = true;
        
        
        _animator.SetBool(FAST_RUN_ANIM,true);
        float duration = 1.1f;  // 움직임의 전체 기간 설정
    
        transform.DOPath(_duckFlyRouteAVector, duration, PathType.CatmullRom)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                _animator.SetBool(FAST_RUN_ANIM, false);
                var directionToLook = _duckAwayRouteVector[1] - transform.transform.position;
                var lookRotation = Quaternion.LookRotation(directionToLook);
                transform.DORotate(lookRotation.eulerAngles, 1.6f)
                
                    .OnComplete(() =>
                    {
                        _animator.SetBool(FAST_RUN_ANIM, false);
                        _animator.SetBool(SWIM_ANIM, true);
                        transform.DOPath(_duckAwayRouteVector, 20.0f, PathType.CatmullRom)
                            .SetDelay(0f)
                            .SetLookAt(0.01f)
                            .SetEase(Ease.InOutQuad).OnComplete(() =>
                            {
                                _animator.SetBool(SWIM_ANIM, false);
                                _isClickedAnimStarted = false;
                            });
                    });
            
            });
    }
  
    
   
}
}
