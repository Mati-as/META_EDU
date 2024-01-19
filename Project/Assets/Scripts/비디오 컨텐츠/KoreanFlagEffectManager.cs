using UnityEngine;

public class KoreanFlagEffectManager : Base_EffectManager
{
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point, burstCount: 15);
            break;
        }
    }
}