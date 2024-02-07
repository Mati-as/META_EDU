using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandFootFlip_CollisionController : MonoBehaviour
{
    //reference
    private HandFootFlip_GameManager _gm;
    void Start()
    {
        _gm = GameObject.FindWithTag("GameManager").GetComponent<HandFootFlip_GameManager>();
    }


    
       


        private void OnTriggerEnter(Collider other)
        {
            _gm.FlipAndChangeColor(other.transform);
        }
}
