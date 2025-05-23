using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandFootFlip_CollisionController : MonoBehaviour
{
    //reference
    private BB003_HandFootFlipBaseGameManager _gm;
    void Start()
    {
        _gm = GameObject.FindWithTag("GameManager").GetComponent<BB003_HandFootFlipBaseGameManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        _gm.FlipAndChangeColor(other.transform);
    }

    private void OnDestroy()
    {
        Destroy(this.gameObject);
    }
}
