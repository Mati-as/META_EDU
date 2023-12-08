using UnityEngine;
using UnityEngine.InputSystem;


public class SummerNight_EffectController : Base_EffectController
{

    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point
                
                , true);
        }
       
    }

 
}
