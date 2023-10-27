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

//     public ReactiveProperty<bool> isArrivedAtDrinkablePosition;
//     public ReactiveProperty<bool> isLeftAtDrinkablePosition;
//
//     private void Awake()
//     {
//         isArrivedAtDrinkablePosition = new ReactiveProperty<bool>(false);
//         isLeftAtDrinkablePosition = new ReactiveProperty<bool>(true);
//     }
//
//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("IdleLakeGameDrinkablePosition"))
//         {
//             if (isArrivedAtDrinkablePosition.Value)
//             {
//                 isArrivedAtDrinkablePosition.Value = false;
//             }
//             else
//             {
//                 isArrivedAtDrinkablePosition.Value = true;
//             }
//            
//             
//            
// #if UNITY_EDITOR
//             Debug.Log($"{(this)}동물 호수도착.");
//             Debug.Log($"isArrivedAtDrinkablePosition: {isArrivedAtDrinkablePosition.Value}");
// #endif
//         } 
//     }
//
//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("IdleLakeGameDrinkablePosition"))
//         {
// #if UNITY_EDITOR
//             Debug.Log($"{(this)}동물 호수에서 시작지점으로...");
// #endif
//             if (isLeftAtDrinkablePosition.Value)
//             {
//                 isLeftAtDrinkablePosition.Value = false;
//             }
//             else
//             {
//                 isLeftAtDrinkablePosition.Value = true;
//             }
//          
//         }

    //   
    // }
}
