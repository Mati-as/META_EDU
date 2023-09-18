using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class UIAudioController : MonoBehaviour
{

 public AudioClip roundStartClip;
 public AudioClip correctClip;
 public AudioClip finishedClip;

    private AudioSource _audioSource;
    private void Awake()
    {
        SubscribeGameManagerEvents();
        _audioSource = GetComponent<AudioSource>();
        
    }
    

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }


    private void OnGameStart()
    {
        _audioSource.mute = true;
    }

    private void OnRoundReady()
    {
      
        _audioSource.mute = true;
    }
   
    private void OnRoundStarted()
    {
        _audioSource.clip = roundStartClip;
        _audioSource.Play();
    }

    private void OnCorrect()
    {
        _audioSource.clip = correctClip;
        _audioSource.Play();
    }
    
    private void OnRoundFinished()
    {
        _audioSource.mute = true;
    }
    
    private void OnGameFinished()
    { 
        _audioSource.clip = finishedClip;
        _audioSource.Play();
       
    }
    
    
    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
        
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onCorrectedEvent += OnCorrect;

        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundFinishedEvent += OnRoundFinished;

        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onRoundStartedEvent += OnRoundStarted;
        
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }
    
    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }
    
}
