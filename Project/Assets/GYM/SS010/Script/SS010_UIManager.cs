using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class SS010_UIManager : Base_UIManager
{
    private enum UI
    {
        TimerUI,
        

    }

    private enum Btn
    {
        
    }

    private enum Tmp
    {
        TMP_OnTimerUI,
        
        
    }

    private SS010_GameManager _gameManager;
    
    
    public float _currentGameTimer;
    private GameObject _timerUI;
    private TextMeshProUGUI _timerTmp;
    private Tween _timerTween;
    
    public override void ExplicitInitInGame()
    {
        base.ExplicitInitInGame();

        _gameManager = FindAnyObjectByType<SS010_GameManager>();
        
        BindObject(typeof(UI));
        BindButton(typeof(Btn));
        BindTMP(typeof(Tmp));
        
        _timerUI = GetObject((int)UI.TimerUI);
        _timerTmp = GetTMP((int)Tmp.TMP_OnTimerUI);

        _currentGameTimer = _gameManager.gameTimer;
        _timerUI.SetActive(false);
        
        Logger.CoreClassLog("EA038_UIManager Init ---------------");
    }

    public void StartTimer(Action onCompleteCallback = null)
    {
        _timerUI.SetActive(true);
        
        _timerTween?.Kill();

        _timerTween = DOVirtual.Float(
                _currentGameTimer, // 시작
                0f, // 끝
                _currentGameTimer, // 유지시간
                value => // 매 프레임 마다
                {
                    int seconds = Mathf.CeilToInt(value);
                    _timerTmp.text = seconds.ToString();
                }
            )
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                OnTimerEnded();
                _gameManager.playingGame = false;

                _gameManager.PlayVictorySoundAndEffect();
                onCompleteCallback?.Invoke();
            });
        
    }
    
    private void OnTimerEnded()     // 필요 시 리셋하거나 다음 로직 호출
    {
        Debug.Log("타이머 종료!");

        _currentGameTimer = _gameManager.gameTimer;
        DOVirtual.DelayedCall(1f, () => _timerUI.SetActive(false));

    }

    
    
}
