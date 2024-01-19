using UnityEngine;
using UnityEngine.InputSystem;


public class SummerNightEffectManager : Base_EffectManager
{

    private readonly string AUDIO_PATH_EFFECT_A = "게임별분류/비디오컨텐츠/SummerNight/Video_SummerNight_effectClipA";
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            PlayParticle(particlePool,hit.point
                
                , true);
        }
       
    }

 
}
