using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFallenLeaveParticleController : MonoBehaviour
{

    [SerializeField] private ParticleSystem particleSystemYellow;
    [SerializeField] private ParticleSystem particleSystemOrange;
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
        particleSystemYellow.Play();
        particleSystemOrange.Play();
    }
    private void OnLeftUIActivate()
    {
        particleSystemYellow.Stop();
        particleSystemOrange.Stop();
    }
    private void Subscribe()
    {
        TextBoxUIController.TextBoxLeftUIEvent -= OnLeftUIActivate;
        TextBoxUIController.TextBoxRightUIEvent -= OnRightUIActivate;
        TextBoxUIController.TextBoxLeftUIEvent += OnLeftUIActivate;
        TextBoxUIController.TextBoxRightUIEvent += OnRightUIActivate;
    }

    private void Unsubscribe()
    {
        TextBoxUIController.TextBoxLeftUIEvent -= OnLeftUIActivate;
        TextBoxUIController.TextBoxRightUIEvent -= OnRightUIActivate;
    }
}
