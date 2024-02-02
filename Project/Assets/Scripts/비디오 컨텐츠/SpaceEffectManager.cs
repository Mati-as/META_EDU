using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceEffectManager : EffectManager
{

    public int waitCount;
    private int _currentCount;

    protected override void OnGmRaySyncedByOnGm()
    {
        if (_currentCount > waitCount)
        {
            hits = Physics.RaycastAll(ray_EffectManager);
            foreach (var hit in hits)
            {
                PlayParticle(particlePool,hit.point);
            }

            _currentCount = 0;
        }
        else
        {
            _currentCount++;
        }
       
    }

 
}