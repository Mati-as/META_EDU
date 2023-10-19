using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using MyCustomizedEditor;
#endif

public class IdleLakeAnimalController : MonoBehaviour
{

    public ReactiveProperty<bool> isArrivedAtDrinkablePosition { get; set; }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IdleLakeGameDrinkablePosition"))
        {
            isArrivedAtDrinkablePosition.Value = true;
        } ;
    }
}
