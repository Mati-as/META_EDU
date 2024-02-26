using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class sandwich_BigIngredientController : MonoBehaviour
{
//     private Rigidbody _rb;
//     private float _delay = 0.4f;
//     private float _dropDealy = 0.1f;
//     private bool _isOnPlate;
//     private Sandwitch_GameManager _gm;
//     private bool _isFalling;
//     
//
//     private void Start()
//     {
//         _gm = GameObject.FindWithTag("GameManager").GetComponent<Sandwitch_GameManager>();
//         _rb = GetComponent<Rigidbody>();
//         Sandwitch_GameManager.onIngDrop -= OnDrop;
//         Sandwitch_GameManager.onIngDrop += OnDrop;
//     }
//
//     private void OnCollisionEnter(Collision other)
//     {
//         
//         if (!Sandwitch_GameManager.isGameStart) return;
//         if (!_isFalling) return;
//         
//         
// #if UNITY_EDITOR
// Debug.Log($"dropped on Plate--------");
// #endif
//                 
//             DOVirtual.Float(0, 0, _delay, _ => { }).OnComplete(() =>
//             {
//             _rb.constraints = RigidbodyConstraints.FreezeAll;
//             _isFalling = false;
//             });
//         
//     }
//
//     private void OnDrop()
//     {
//
//         if (!_isOnPlate && _gm.currentClickedIng.gameObject.name.Substring("Plate".Length) == gameObject.name)
//         {
//             DOVirtual.Float(0, 0, _dropDealy, _ => { }).OnComplete(() =>
//             {
//                 _rb.constraints = RigidbodyConstraints.None;
//                 _isFalling = true;
//             });
// #if UNITY_EDITOR
// Debug.Log($"NONE CONSTARINT ----------");
// #endif
//         }
//   
//        
//     } 
}
