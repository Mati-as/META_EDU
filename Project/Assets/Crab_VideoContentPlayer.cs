using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;

public class Crab_VideoContentPlayer : Base_VideoContentPlayer
{
    
    private bool _isShaked;

    private bool _isReplayEventTriggered;
    // Start is called before the first frame update
    void Start()
    {
        Init();
        
        Crab_EffectController.onClicked -= OnClicked;
        Crab_EffectController.onClicked += OnClicked;
    }

    private void Update()
    {
        if (videoPlayer.isPlaying && !_isReplayEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴
            double currentTime = videoPlayer.time;
            double totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.95)
            {
                TriggerEvent();
                _isReplayEventTriggered = true; // 이벤트가 한 번만 실행되도록 함
            }
        }
    }

    private void OnDestroy()
    {
        Crab_EffectController.onClicked -= OnClicked;
    }

    protected override void Init()
    {
        base.Init();
        
        DOVirtual.Float(1,0,3f,speed=>
        {
            videoPlayer.playbackSpeed = speed;
        });
    }

    void OnClicked()
    {
        if (!_initiailized) return;
        
        if (!_isShaked)
        {
            DOVirtual.Float(0,1,2f,speed=>
            {
                videoPlayer.playbackSpeed = speed;
            }).OnComplete(() =>
            {
                _isShaked = true;
            });
            
            transform.DOShakePosition(2.25f, 2.5f, randomness: 90,vibrato:5);
            
        }
    }
}
