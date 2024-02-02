using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurtleEffectManager : EffectManager
{
    protected override void OnGmRaySyncedByOnGm()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits) PlayParticle(particlePool,hit.point);
    }
}