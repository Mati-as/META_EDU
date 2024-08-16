using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundVer2_GameManager : Playground_GameManager
{
    public static event Action OnScoreValueChange;
    public static event Action OnTimeOver;
    private int _scoreLeft; // 백킹 필드
    private int _scoreRight; // 백킹 필드
    private int _currentTime;  

    private int TIME_LIMIT = 15;
    private float _elapsedTime;
    private bool _isRoundFinished;
    public bool isReInitializing { get; private set; }
    public int currentTime
    {
        get => _currentTime;
        set
        {
            if (_isRoundFinished) return;
            _currentTime = value;
        }
    }
    
    public int scoreLeft
    {
        get => _scoreLeft;
        set
        {
            if (_isRoundFinished) return;
            if (_scoreLeft != value)
            {
                _scoreLeft = value;
                OnScoreValueChange?.Invoke();
            }
        }
    }
    public int scoreRight
    {
        get => _scoreRight;
        set
        {
            if (_isRoundFinished) return;
            if (_scoreRight != value)
            {
                _scoreRight = value;
                OnScoreValueChange?.Invoke();
            }
        }
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
    }

    protected override void Init()
    {
        base.Init();
        PlaygroundVer2_UIManager.OnReInitUIFinished -= Restart;
        PlaygroundVer2_UIManager.OnReInitUIFinished += Restart;
    }

    private void OnDestroy()
    {
        PlaygroundVer2_UIManager.OnReInitUIFinished -= Restart;
    }

    private void Update()
    {
        if (!_isRoundFinished && !isReInitializing)
        {
            _elapsedTime += Time.deltaTime;
            currentTime = (int)(TIME_LIMIT - _elapsedTime);
        }
        if (currentTime <= 0 && !_isRoundFinished)
        {
            _isRoundFinished = true;
            isReInitializing = true;
            OnTimeOver?.Invoke();
        }
    }

    private void Restart()
    {
        currentTime = (int)TIME_LIMIT;
        _elapsedTime = 0;
        
        _isRoundFinished = false;
        isReInitializing = false;
        scoreLeft = 0;
        scoreRight = 0;
    }
}
