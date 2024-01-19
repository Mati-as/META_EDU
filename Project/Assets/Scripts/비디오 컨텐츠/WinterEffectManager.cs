using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class WinterEffectManager : Base_EffectManager
{

   
    protected override void OnGmRaySyncedByOnGm()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point);
            break;
        }

       
    }
}
