using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crab_EffectController : Base_EffectController
{
    public static event Action onClicked; 
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point);
            
#if UNITY_EDITOR
            Debug.Log("Clicked");
#endif
            if (!Crab_VideoContentPlayer._isShaked)
            {
                onClicked?.Invoke();
            }
         
            
            break;
            
        }

    }
}
