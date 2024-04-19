using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeadsDrum_BeadsController : MonoBehaviour,IBeadOnClicked
{


    [Range(0, 50)] public float _power;
    private Rigidbody _rb;
    
    //드럼위 (가장자리 제외)에 있는경우, 다른 로직 수행
    private readonly string DRUM_AREA_RIGHT ="InDrumAreaRight";
    private readonly string DRUM_AREA_LEFT ="InDrumAreaLeft";
    private bool _isOnDrumLeft;
    private bool _isOnDrumRight;

    private float _powerOnFloorMin = 0.85f;
    private float _powerOnFloorMax = 0.95f;
    
    private float _powerOnDrumMin = 1.6f;
    private float _powerOnDrumMax = 1.8f;

    private Vector3 _drumLeftLocation;
    private Vector3 _drumRightLocation;

    private void Awake()
    {
        _drumLeftLocation = GameObject.Find("DrumLeft").transform.position;
        _drumRightLocation = GameObject.Find("DrumRight").transform.position;
        
        BeadsDrum_Controller.OnStickHitRight -= OnStickHitRight;
        BeadsDrum_Controller.OnStickHitRight += OnStickHitRight;
        
        BeadsDrum_Controller.OnStickHitLeft -= OnStickHitLeft;
        BeadsDrum_Controller.OnStickHitLeft += OnStickHitLeft;
    }
    private void OnDestroy()
    {
        BeadsDrum_Controller.OnStickHitRight -= OnStickHitRight;
        BeadsDrum_Controller.OnStickHitLeft -= OnStickHitLeft;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

  

    public void OnClicked()
    {

        if (_isOnDrumLeft || _isOnDrumRight)
        {
            var powerMode = Random.Range(_powerOnDrumMin, _powerOnDrumMax);
            Vector3 forceDirection = Vector3.up * powerMode
                                     + Vector3.back * powerMode
                                     + Vector3.forward * powerMode
                                     + Vector3.right * powerMode
                                     + Vector3.left * powerMode;
            _rb.AddForce(_power * forceDirection, ForceMode.Impulse);

        }
        else
        {
            var random = Random.Range(0, 1f);
            if (random > 0.3f)
            {
                var leftDistance = Vector3.Distance(_drumLeftLocation, transform.position);
                var rightDistance = Vector3.Distance(_drumRightLocation, transform.position);


                if (leftDistance < rightDistance)
                {
                    var powerMode = Random.Range(_powerOnFloorMin, _powerOnFloorMax);
                    Vector3 forceDirection = (_drumLeftLocation - transform.position).normalized + Vector3.up*2f;
                    _rb.AddForce(_power * forceDirection * powerMode, ForceMode.Impulse);
                }
                else
                {
                    var powerMode = Random.Range(_powerOnFloorMin, _powerOnFloorMax);
                    Vector3 forceDirection = (_drumRightLocation - transform.position).normalized + Vector3.up*2f;
                    _rb.AddForce(_power * forceDirection * powerMode, ForceMode.Impulse);
                }
            }
            else
            {
                var powerMode = Random.Range(_powerOnFloorMin, _powerOnFloorMax);
                Vector3 forceDirection = Vector3.up * powerMode
                                         + Vector3.back * powerMode * 1.3f
                                         + Vector3.forward * powerMode *1.3f
                                         + Vector3.right * powerMode* 1.3f
                                         + Vector3.left * powerMode * 1.3f;
                _rb.AddForce(_power * forceDirection, ForceMode.Impulse);
            }
         
            
        }

  
        
        

        
      

    }

    private bool _isSoundPlaying;

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.gameObject.name == DRUM_AREA_LEFT)
        {
            _isOnDrumLeft = true;
        }
        if (other.transform.gameObject.name == DRUM_AREA_RIGHT)
        {
            _isOnDrumRight = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.name == DRUM_AREA_LEFT)
        {
            _isOnDrumLeft = true;
        }
        if (other.transform.gameObject.name == DRUM_AREA_RIGHT)
        {
            _isOnDrumRight = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.name == DRUM_AREA_LEFT)
        {
            _isOnDrumLeft = false;
        }
        if (other.transform.gameObject.name == DRUM_AREA_RIGHT)
        {
            _isOnDrumRight = false;
        }
    }

    private void OnStickHitRight()
    {
        if (_isOnDrumRight)
        {
            var ranForce = Random.Range(_powerOnFloorMin, _powerOnFloorMax);
            
            _rb.AddForce(_power * ranForce
                                * (Vector3.up * 1.3f
                                   + ranForce * Vector3.back
                                   + ranForce * Vector3.forward
                                   + ranForce * Vector3.right
                                   + ranForce * Vector3.left
                                )
                , ForceMode.Impulse);
        }

    }
    
    private void OnStickHitLeft()
    {

        if (_isOnDrumLeft)
        {

            var ranForce = Random.Range(_powerOnFloorMin, _powerOnFloorMax);

            
            _rb.AddForce(_power * ranForce
                                * (Vector3.up * 1.3f
                                   + ranForce * Vector3.back
                                   + ranForce * Vector3.forward
                                   + ranForce * Vector3.right
                                   + ranForce * Vector3.left
                                )
                , ForceMode.Impulse);
        }

    }
}
