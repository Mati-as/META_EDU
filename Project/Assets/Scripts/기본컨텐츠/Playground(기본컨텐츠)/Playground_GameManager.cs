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

    
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        
        
        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            
            //hit.transform.TryGetComponent(out _currentRigidBody);
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>(); // 부딪힌 물체에 Rigidbody 컴포넌트가 있는지 확인합니다.

          
            if (rb != null)
            {
              
                Vector3 forceDirection =  rb.transform.position - hit.point + Vector3.up*upOffset;
                
                rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);
            }
        }
    }
}
