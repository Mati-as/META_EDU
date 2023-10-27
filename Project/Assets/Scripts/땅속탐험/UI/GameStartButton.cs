using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameStartButton : MonoBehaviour
{
    [SerializeField] 
    private GroundGameManager gameManager;

    private Button _button;

    [SerializeField] private AudioSource uiAudioSource;
    public AudioClip buttonSound;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (gameManager.isStageStartButtonClicked.Value == false)
        {
            gameManager.isStageStartButtonClicked.Value = true;
        }
        else
        {
            gameManager.isStageStartButtonClicked.Value = false;
        }
        
        
        
        if (uiAudioSource != null)
        {
            uiAudioSource.clip = buttonSound;
            uiAudioSource.Play();
        }
    }
    
   


}
