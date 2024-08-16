using UnityEngine;

public class KoreanFlagVidoContentGameManager : VidoContentGameManager
{
    protected override void OnGmRaySyncedByOnGm()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point, burstCount: 15);
            break;
        }
    }
}