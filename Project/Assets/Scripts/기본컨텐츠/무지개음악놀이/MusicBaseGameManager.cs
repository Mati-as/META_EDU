using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicBaseGameManager : Base_GameManager
{
    protected override void OnGameStartStartButtonClicked()
    {
        initialMessage= "무지개 건반을 눌러 연주해보세요";
        _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
        base.OnGameStartStartButtonClicked();
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
#if UNITY_EDITOR
       // Debug.Log("eventAfterAGetRay Invoke");
#endif
    }
}
