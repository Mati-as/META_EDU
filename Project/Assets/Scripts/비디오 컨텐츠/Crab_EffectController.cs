using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crab_EffectController : Base_EffectController
{
    public static event Action onClicked;
    public static event Action OnClickForEachClick;
    public Vector3 hitPoint { get; private set; }

    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            
            hitPoint = hit.point;
            
            PlayParticle(particlePool,hit.point);
            

            if (!Crab_VideoContentPlayer._isShaked)
            {
                onClicked?.Invoke();
            }
            
            OnClickForEachClick?.Invoke();
            break;
            
        }

    }
}
