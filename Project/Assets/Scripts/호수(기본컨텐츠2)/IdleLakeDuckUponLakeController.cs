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

public class IdleLakeDuckUponLakeController : MonoBehaviour
{
   public  readonly int IDLE_ANIM = Animator.StringToHash("idle");
   public  readonly int EAT_ANIM = Animator.StringToHash("Eat");
   public  readonly int FAST_RUN_ANIM = Animator.StringToHash("FastRun");
   public  readonly int SWIM_ANIM = Animator.StringToHash("Swim");
   
   public Transform[] jumpSurpPath = new Transform[3];
   private Vector3[] _jumpSurpPathVec = new Vector3[3];
   
   
   public Transform[] patrolPath = new Transform[4];
   private Vector3[] _patrolPathVec = new Vector3[4];

   private void Awake()
   {
      for (int i = 0; i < 4; i++)
      {
         _patrolPathVec[i] = patrolPath[i].position;
      }

  
   }
   private void Start()
   {
      var trigger = GetComponent<EventTrigger>();
      var entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerClick;
      entry.callback.AddListener(data => { OnClicked(); });
      trigger.triggers.Add(entry);
      PatrolAround();
   }

   private void OnClicked()
   {
      DOTween.Kill(transform);

      for (int i = 0; i < 3; i++)
      {
         _jumpSurpPathVec[i] = jumpSurpPath[i].position;
      }
      transform.DOPath(_jumpSurpPathVec, 0.5f, PathType.CatmullRom); 
      
      
      var directionToLook = _patrolPathVec[0] - transform.transform.position;
      var lookRotation = Quaternion.LookRotation(directionToLook);
      transform.DORotate(lookRotation.eulerAngles, 0.4f)
         .SetDelay(1.5f)
         .OnComplete(() =>
         {
            transform.DOMove(_patrolPathVec[0], 13f)
               .OnComplete(PatrolAround);
         });
   }

   public float oneCycleDuration;
   private void PatrolAround()
   {
      transform.DOPath(_patrolPathVec, oneCycleDuration, PathType.CatmullRom)
         .SetDelay(0f)
         .SetLookAt(0.01f)
         .SetOptions(true)
         .SetEase(Ease.InOutQuad).OnComplete(PatrolAround);
   }
}
