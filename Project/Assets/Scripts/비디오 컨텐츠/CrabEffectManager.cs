using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabEffectManager : Base_EffectManager
{
    public static event Action Crab_OnClicked;
    public static event Action Crab_OnClickedSingle;
    public Vector3 hitPoint { get; private set; }

    protected override void OnGmRaySyncedByOnGm()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            
            hitPoint = hit.point;
            
            PlayParticle(particlePool,hit.point);
            

            if (!CrabVideoGameManager._isShaked)
            {
                Crab_OnClicked?.Invoke();
            }
            
            Crab_OnClickedSingle?.Invoke();
            break;
            
        }

    }
}
