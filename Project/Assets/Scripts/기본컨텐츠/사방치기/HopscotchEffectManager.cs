using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사방치기에서는 BaseEffectController가 effect연출 및 GameManager역할 모두 수행.
/// 게임 규모 확장 시 GameManager와 분리 권장 1/15/24 
/// </summary>
public class HopscotchEffectManager : Base_EffectManager
{
    

    public static event Action Hopscotch_OnClick; 
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {

            PlayParticle(particlePool,hit.point);
#if UNITY_EDITOR
         
#endif
            Hopscotch_OnClick?.Invoke();
            break;
        }
    }
    
    // protected override void Init()
    // {
    //     base.Init();
    //     // SetPool(ref _clickInducingParticle);
    //     // SetPool(ref _successInterfaceParticle);
    //     
    // }
}
