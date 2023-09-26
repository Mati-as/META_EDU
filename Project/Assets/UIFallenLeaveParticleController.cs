using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIFallenLeaveParticleController : MonoBehaviour
{

    [SerializeField] private ParticleSystem particleSystemYellowRight;
    [SerializeField] private ParticleSystem particleSystemOrangeRight;
    
    [SerializeField] private ParticleSystem particleSystemYellowLeft;
    [SerializeField] private ParticleSystem particleSystemOrangeLeft;
    void Start()
    {
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnRightUIActivate()
    {
        particleSystemYellowLeft.Stop();
        particleSystemOrangeLeft.Stop();
        
        particleSystemYellowRight.Play();
        particleSystemOrangeRight.Play();
    }
    private void OnLeftUIActivate()
    {
        particleSystemYellowRight.Stop();
        particleSystemOrangeRight.Stop();
        
        particleSystemYellowLeft.Play();
        particleSystemOrangeLeft.Play();
    }

    private void StopAllParticles()
    {
       
        particleSystemYellowRight.Stop();
        particleSystemOrangeRight.Stop();
        particleSystemYellowLeft.Stop();
        particleSystemOrangeLeft.Stop();
        
        particleSystemYellowRight.Clear();
        particleSystemOrangeRight.Clear();
        particleSystemYellowLeft.Clear();
        particleSystemOrangeLeft.Clear();

    }
    private void Subscribe()
    {
        FallenLeafInstructionButtonEventListener.FallenLeaveStartButtonEvent
            -= StopAllParticles;
        FallenLeafInstructionButtonEventListener.FallenLeaveStartButtonEvent
            += StopAllParticles;
        TextBoxUIController.TextBoxLeftUIEvent -= OnLeftUIActivate;
        TextBoxUIController.TextBoxRightUIEvent -= OnRightUIActivate;
        TextBoxUIController.TextBoxLeftUIEvent += OnLeftUIActivate;
        TextBoxUIController.TextBoxRightUIEvent += OnRightUIActivate;
    }

    private void Unsubscribe()
    {
        
        FallenLeafInstructionButtonEventListener.FallenLeaveStartButtonEvent
            -= StopAllParticles;
        TextBoxUIController.TextBoxLeftUIEvent -= OnLeftUIActivate;
        TextBoxUIController.TextBoxRightUIEvent -= OnRightUIActivate;
    }
}
