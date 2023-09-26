using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FallenLeafInstructionButtonEventListener : MonoBehaviour
{
    [SerializeField]
    private TextBoxUIController textBoxUIController;
    
    private Button _button;
    private readonly int START_PLAY = 1;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    private void Start()
    {
        _button.onClick.AddListener(ButtonClicked);
        
    }

    public static event Action FallenLeaveStartButtonEvent; 
    private bool _isButtonClicked;
    void ButtonClicked()
    {
        if (!_isButtonClicked)
        {
            FallenLeaveStartButtonEvent?.Invoke();
            textBoxUIController.OnUIFinished();
            _isButtonClicked = true;
            _audioSource.Play();
        }
      
    }
}
