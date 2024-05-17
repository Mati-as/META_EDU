using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundVer2_GameManager : Playground_GameManager
{
    public static event Action OnScoreValueChange;
     
    private int _scoreLeft; // 백킹 필드
    private int _scoreRight; // 백킹 필드
    
    public int scoreLeft
    {
        get => _scoreLeft;
        set
        {
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
            if (_scoreRight != value)
            {
                _scoreRight = value;
                OnScoreValueChange?.Invoke();
            }
        }
    }
    
    


}
