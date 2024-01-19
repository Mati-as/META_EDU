using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallEffectManager : Base_EffectManager
{
    protected override void OnGmRaySyncedByOnGm()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point,
                 false);
            break;
        }
    }
}
