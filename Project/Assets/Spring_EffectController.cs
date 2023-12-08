using UnityEngine;

public class Spring_EffectController : Base_EffectController
{
    
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point, usePsLifeTime: true,useSubEmitter:true);
            break;
        }
    }
}