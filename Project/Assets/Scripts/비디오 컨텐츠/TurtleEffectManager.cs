using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurtleEffectManager : Base_EffectManager
{
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits) PlayParticle(particlePool,hit.point);
    }
}