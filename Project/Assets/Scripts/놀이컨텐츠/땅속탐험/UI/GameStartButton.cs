using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameStartButton : MonoBehaviour
{
    [FormerlySerializedAs("gameManager")] [SerializeField] 
    private GroundGameController gameController;

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
        if (gameController.isStageStartButtonClicked.Value == false)
        {
            gameController.isStageStartButtonClicked.Value = true;
        }
        else
        {
            gameController.isStageStartButtonClicked.Value = false;
        }
        
        
        
        if (uiAudioSource != null)
        {
            uiAudioSource.clip = buttonSound;
            uiAudioSource.Play();
        }
    }
    
   


}
