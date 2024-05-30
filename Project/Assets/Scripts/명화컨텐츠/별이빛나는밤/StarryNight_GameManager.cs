using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarryNight_GameManager : MonoBehaviour
{
    private StarryNight_GameManager _gameManager;

    public static bool isGameStarted { get; set; }
    public static event Action onGameStarted;
    
    private void Start()
    {
        if (_gameManager == null)
        {
            _gameManager = new StarryNight_GameManager();
        }

        Application.targetFrameRate = 60; 
    }


    private void OnIntroCamArrived()
    {
        onGameStarted?.Invoke();
    }

}
