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
        AnimalTripBaseGameManager.onCorrectedEvent -= OnCorrect;
        AnimalTripBaseGameManager.onCorrectedEvent += OnCorrect;
        
        AnimalTripBaseGameManager.onRoundReadyEvent -= OnReady;
        AnimalTripBaseGameManager.onRoundReadyEvent += OnReady;

        AnimalTripBaseGameManager.onRoundFinishedEvent -= OnRoundFinished;
        AnimalTripBaseGameManager.onRoundFinishedEvent += OnRoundFinished;
        
    }

    
    private void EnableCollider(Collider collider) =>   collider.enabled = true;

    private void DisableCollider(Collider collider) => collider.enabled = false;


    private void OnCorrect()
    {
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
        AnimalTripBaseGameManager.onCorrectedEvent -= OnCorrect;
        
        AnimalTripBaseGameManager.onRoundReadyEvent -= OnReady;
        
        AnimalTripBaseGameManager.onRoundFinishedEvent -= OnRoundFinished;
    }
    
    
}
