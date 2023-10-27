using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryUIButtonListener : MonoBehaviour
{
    private Button _button;
    
    [SerializeField]
    private StoryUIController _storyUIController;

    [SerializeField]
    private AudioSource _audioSource;
    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    private void Start()
    {
        _button.onClick.AddListener(ButtonClicked);
    }
    
    void ButtonClicked()
    {
        //storyUI 구분을 위한 로직
        if (GameManager.isCameraArrivedToPlay)
        {
            GameManager.isGameStopped = false;
            _storyUIController.gameObject.SetActive(false);
        }
        else
        {
            UIManager.SetFalseAndTriggerStartButtonEvent();
            _storyUIController.gameObject.SetActive(false);
        }
      
        if(_audioSource != null) _audioSource.Play();
      
      
    }
}
