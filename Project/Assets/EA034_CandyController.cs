using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class EA034_CandyController : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _collider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name.Contains("Cake"))
        {
            DOVirtual.Float(0, 1, 0.5f, _ =>
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            });
            _rb.useGravity = false;
            _collider.enabled = false;
        }
        
    }
}
