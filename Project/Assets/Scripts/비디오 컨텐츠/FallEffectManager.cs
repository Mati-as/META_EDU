using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallEffectManager : Base_EffectManager
{
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point,
                 false);
            break;
        }
    }
}
