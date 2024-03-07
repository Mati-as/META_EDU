using System;
using DG.Tweening;
using UnityEngine;

public class Underground_UIManager : MonoBehaviour
{
    private GroundGameController _gameController;

    private GameObject StoryUI;
    private void Awake()
    {
        _gameController = GameObject.FindWithTag("GameController").GetComponent<GroundGameController>();
    }

    private void Start()
    {
        _gameController = GameObject.FindWithTag("GameController").GetComponent<GroundGameController>();

        UI_Scene_Button.onBtnShut -= TriggerStoryUI;
        UI_Scene_Button.onBtnShut += TriggerStoryUI;
#if UNITY_EDITOR
        if (_gameController == null)
        {
            Debug.LogError("GameController or GroundGameManager is null.");
        }
#endif
    }


    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= TriggerStoryUI;
    }

    private void TriggerStoryUI()
    {
        if (_gameController.isStartButtonClicked.Value == false) _gameController.isStartButtonClicked.Value = true;
    }
}