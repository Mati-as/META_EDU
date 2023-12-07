using UnityEngine;

public class Spring_EffectController : Base_EffectController
{
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point, usePsLifeTime: false, emitAmount: emitAmount,wait:5.5f);

            break;
        }
    }
}