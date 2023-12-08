using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space_EffectControllerGlow : Base_EffectController
{
    
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point,
                 
                 wait: 3.4f,usePsLifeTime:false);
            break;
        }
    }
}
