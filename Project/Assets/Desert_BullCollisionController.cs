using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Desert_BullCollisionController : MonoBehaviour
{
    
    
    private ContactPoint contact;
    public GameObject collisionPoint
    {
        get;
        private set;
    }

    private void Awake()
    {
        collisionPoint = new GameObject();
    }

    public static event Action onCollisionEvent;
    
    private void OnCollisionEnter(Collision collision)
    {
        
#if UNITY_EDITOR
        Debug.Log($"that's not a cow..");
#endif
        if (collision.gameObject.name == "DetectiveCollider")
        {
            contact = collision.contacts[0];
          
            collisionPoint.transform.position = contact.point;
            onCollisionEvent?.Invoke();
        }
    }


}
