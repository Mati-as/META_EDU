using UnityEngine;
using System;
using UnityEngine.Serialization;

public class PropsController : MonoBehaviour
{

    [SerializeField] private Collider _touchDownBoxCollider;
    [SerializeField] private Collider _hillCollider;
    
    

    private void Awake()
    {
        _touchDownBoxCollider.enabled = false;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onCorrectedEvent += OnCorrect;
        
        GameManager.onRoundReadyEvent -= OnReady;
        GameManager.onRoundReadyEvent += OnReady;

        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundFinishedEvent += OnRoundFinished;
        
    }

    
    private void EnableCollider(Collider collider) =>   collider.enabled = true;

    private void DisableCollider(Collider collider) => collider.enabled = false;


    private void OnCorrect()
    {
        DisableCollider(_hillCollider);
        
        EnableCollider(_touchDownBoxCollider);
    }

    private void OnReady()
    {
        DisableCollider(_touchDownBoxCollider);
    }

    private void OnRoundFinished()
    {
        EnableCollider(_hillCollider);
    }
    private void OnDestroy()
    {
        GameManager.onCorrectedEvent -= OnCorrect;
        
        GameManager.onRoundReadyEvent -= OnReady;
        
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
    }
    
    
}
