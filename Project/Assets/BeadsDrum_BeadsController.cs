using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeadsDrum_BeadsController : MonoBehaviour,IBeadOnClicked
{


    [Range(0, 50)] public float _power;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void OnClicked()
    {
        _rb.AddForce(_power * Random.Range(0.75f, 1.25f)
                            * (Vector3.up * 1.3f
                               + Random.Range(0.8f, 1.2f) * Vector3.back
                               + Random.Range(0.8f, 1.2f) * Vector3.forward
                               + Random.Range(0.8f, 1.2f) * Vector3.right
                               + Random.Range(0.8f, 1.2f) * Vector3.left
                            )
            , ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        var randomChar = (char)Random.Range('A', 'D' + 1);
    
    }
}
