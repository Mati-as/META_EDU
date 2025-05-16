using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMusicBaseGameManager : Base_GameManager
{
 
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        initialMessage= "바다위 색깔 블럭을 눌러 연주 해볼까요?";
        _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
    }
}
