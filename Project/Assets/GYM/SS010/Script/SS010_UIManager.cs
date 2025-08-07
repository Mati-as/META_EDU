using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class SS010_UIManager : Base_InGameUIManager
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

    public void StartTimer(int seqNum, Action onCompleteCallback = null)
    {
        _timerUI.SetActive(true);

        _timerTween?.Kill();

        _timerTween = DOVirtual.Float(
                _currentGameTimer, // 시작
                0f, // 끝
                _currentGameTimer, // 유지시간
                value => // 매 프레임 마다
                {
                    PlayNarrationMidNarration(value, seqNum);
                    int seconds = Mathf.CeilToInt(value);
                    _timerTmp.text = seconds.ToString();
                }
            )
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Debug.Log("타이머 종료!");

                _currentGameTimer = _gameManager.gameTimer;

                _gameManager.playingGame = false;
                _gameManager.PlayVictorySoundAndEffect();

                DOVirtual.DelayedCall(1f, () =>
                {
                    _timerUI.SetActive(false);
                    onCompleteCallback?.Invoke();
                });

            });

    }

    private int _lastCheckedSecond = -1;
    
    private void PlayNarrationMidNarration(float value, int num)
    {
        int seconds = Mathf.CeilToInt(value);

        if (seconds == _lastCheckedSecond)
            return;
        
        _lastCheckedSecond = seconds;

        if (seconds % 11 == 0 && seconds != 0)
        {
            switch (num)
            {
                case 1:
                    Logger.Log("1번 중간 나레이션");
                    Managers.Sound.Play
                    (
                        SoundManager.Sound.Narration,
                        "SS010/Audio/audio_5_친구들_한줄로_천천히_외나무_다리를_건너주세요"
                    );
                    break;
                case 2:
                    Logger.Log("2번 중간 나레이션");
                    Managers.Sound.Play
                    (
                        SoundManager.Sound.Narration,
                        "SS010/Audio/audio_12_친구들_한줄로_천천히_징검_다리를_건너주세요"
                    );
                    break;
                case 3:
                    Logger.Log("3번 중간 나레이션");
                    Managers.Sound.Play
                    (
                        SoundManager.Sound.Narration,
                        "SS010/Audio/audio_16_친구들_한줄로_천천히_징검다리를_건너주세요_"
                    );
                    break;
            }
        }
    }
}