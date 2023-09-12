using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimalController : MonoBehaviour
{
    
    public AnimalData _animalData;
   
    //▼ 동물 이동 로직
    private readonly string TAG_ARRIVAL= "arrival";
    private bool isTouchedDown;
    public bool IsTouchedDown
    {
        get { return isTouchedDown;}
        set { isTouchedDown = value; }
    }

    private void Awake()
    {
        IsTouchedDown = false;
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TAG_ARRIVAL))
        {
            isTouchedDown = true;
            Debug.Log("Touched Down!");
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TAG_ARRIVAL))
        {
            
         
        }
    }

    void Start()
    {
        //중복 구독 방지
        GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
        GameManager.AllAnimalsInitialized += OnAllAnimalsInitialized;

        InitializeTransform();
        // Instantiate(_animalData.animalPrefab, _animalData.initialPosition, _animalData.initialRotation);
    }
    private void OnAllAnimalsInitialized()
    {
        GameManager.isAnimalTransformSet = true;
    }
    
    void OnDestroy()
    {
        GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
    }

    private void Update()
    {
        if (GameManager.isRoundReady)
        {
            OnRoundReady();
        }
       
    }

    private void OnRoundReady()
    {
        isTouchedDown = false;
    }

    private void InitializeTransform()
    {
        _animalData.initialPosition = transform.position;
        _animalData.initialRotation = transform.rotation;
        
        if (GameManager.isAnimalTransformSet == false)
        { 
            GameManager.AnimalInitialized();
            Destroy(gameObject);
        }
    }
}
