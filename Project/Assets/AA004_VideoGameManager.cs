using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AA004_VideoGameManager : VideoContentBaseGameManager
{
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        initialMessage= "밤하늘을 터치하면 반딧불이가 나타나요!";
        _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
    }
}
