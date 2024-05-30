using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class Playground_GameManager : IGameManager
{
    private RaycastHit[] _hits;
  //  private Rigidbody _currentRigidBody;

    public float forceAmount;
    public float upOffset;
    

    protected override void Init()
    {
        DEFAULT_SENSITIVITY = 0.1f;
        SHADOW_MAX_DISTANCE = 80f;
        base.Init();
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        
        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            if(hit.transform.gameObject.name == "Small" ||
               hit.transform.gameObject.name == "Medium"||
            hit.transform.gameObject.name == "Large")
                
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Playground/Ball",0.3f);
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
          
            if (rb != null)
            {
              
                Vector3 forceDirection =  rb.transform.position - hit.point + Vector3.up*upOffset;
                
                rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);
            }
        }
    }
}
