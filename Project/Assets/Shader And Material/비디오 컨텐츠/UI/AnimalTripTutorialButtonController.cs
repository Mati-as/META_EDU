using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalTripTutorialButtonController : MonoBehaviour
{
    private Button _button;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private TextBoxUIController textBoxUIController;
    
    [SerializeField]
    private StoryUIController _storyUIController;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    private void Start()
    {
        _button.onClick.AddListener(ButtonClicked);
    }

    private bool _isUIPlayed;
    void ButtonClicked()
    {
        if (!_isUIPlayed)
        {
            _audioSource.Play();
            textBoxUIController.OnUIFinished();
            _storyUIController.OnHowToPlayUIFinished();
         
            _isUIPlayed = true;
        }
     
    }
}
