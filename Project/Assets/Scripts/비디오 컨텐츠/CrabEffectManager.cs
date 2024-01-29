using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabEffectManager : EffectManager
{
    public static event Action Crab_OnClicked;
  
  

    protected override void OnGmRaySyncedByOnGm()
    {
        base.OnGmRaySyncedByOnGm();
        
        if (!CrabVideoGameManager._isShaked)
        {
            Crab_OnClicked?.Invoke();
        }

    }
}
