using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EA020_ParkingGame_GameManager : Ex_BaseGameManager
{
    /// <summary>
    /// 1. 
    /// </summary>
    private enum MainSeq
    {
        Default,
        SeatSelection,
        CarParkRacing, //OnStart, OnRoundFinish,
        OnFinish
        
        
    }
    protected override void Init()
    {
        base.Init();
    }
}
