using UnityEngine;

public class SpringEffectManager : Base_EffectManager
{
    
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point);
            break;
        }
    }
}