using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    [SerializeField]
    private AnimalData _animalData;

    
    void Start()
    {
        //중복 구독 방지
        // GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
        // GameManager.AllAnimalsInitialized += OnAllAnimalsInitialized;
    }
    private void Awake()
    {
        _animalData.initialPosition = transform.position;
        _animalData.initialRotation = transform.rotation;

        // Instantiate(_animalData.animalPrefab, _animalData.initialPosition, _animalData.initialRotation);
        Destroy(gameObject);
        //GameManager.AnimalInitialized();
       
    }
    
    // 이벤트 사용 로직 09/11/23 미사용 중
    // private void OnAllAnimalsInitialized()
    // {
    //   
    // }
    //
    // void OnDestroy()
    // {
    //     GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
    // }
}
