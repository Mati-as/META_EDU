using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AnimalTripTutorialButtonController : MonoBehaviour
{
    private Button _button;
    [SerializeField]
    private AudioSource _audioSource;
    [FormerlySerializedAs("textBoxUIController")] [SerializeField]
    private TextBoxUIController tutorialBoxController;
    
    [SerializeField]
    private StoryUIController _storyUIController;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    private void Start()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private bool _isUIPlayed;
    void OnButtonClicked()
    {
        if (!_isUIPlayed)
        {
            _audioSource.Play();
            tutorialBoxController.OnUIFinished();
            _storyUIController.OnHowToPlayUIFinished();
         
            _isUIPlayed = true;
        }
     
    }
}
