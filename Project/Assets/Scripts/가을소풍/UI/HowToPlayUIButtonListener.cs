using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUIButtonListener : MonoBehaviour
{
    private Button _button;
    
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
            Debug.Log("설명 끝, 스토리 UI시작.");
            textBoxUIController.OnUIFinished();
            _storyUIController.OnHowToPlayUIFinished();
            _isUIPlayed = true;
        }
     
    }
}
