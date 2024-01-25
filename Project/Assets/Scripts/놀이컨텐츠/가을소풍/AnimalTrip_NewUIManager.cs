using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
///     1/28/24
///     UI 시스템 변경건으로 새로 로직을 구성하기위해 만든 New_UImanager 입니다
///     기존 튜토리얼 박스의 버튼 클릭시 게임이 시작되는 로직을 시간 흐름에 따라 진행 될 수 있도록
///     변경한 것입니다.
/// </summary>
public class AnimalTrip_NewUIManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;


    [SerializeField] private StoryUIController _storyUIController;

    private void Awake()
    {
        _storyUIController = GameObject.Find("StoryUI").GetComponent<StoryUIController>();

      
        UI_Scene_Button.onBtnShut -= ShowStoryUI;
        UI_Scene_Button.onBtnShut += ShowStoryUI;
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= ShowStoryUI;
    }



    private bool _isUIPlayed;

    private void ShowStoryUI()
    {
       _storyUIController.OnHowToPlayUIFinished(); 
    }
}