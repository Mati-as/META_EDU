using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEffectManagerGlow : EffectManager
{
    
    protected override void OnGmRaySyncedByOnGm()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point
                 
                 );
            break;
        }
    }
}
