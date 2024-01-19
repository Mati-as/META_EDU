using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Owl_VideoGameManager : Base_Interactable_VideoGameManager
{
    
        
    protected override void Init()
    {
        base.Init();

        Managers.Sound.Play(SoundManager.Sound.Bgm,
            "Audio/Gamemaster Audio - Fun Casual Sounds/Ω_Bonus_Music/music_candyland",volume:0.125f);
        
        rewindParticlePrefabPath = "게임별분류/비디오컨텐츠/Owl/CFX/CFX_OnRewind";
        rewindParticleAudioPath =  "Audio/비디오 컨텐츠/Owl/Leaves";
            
        
        GameObject prefab = Resources.Load<GameObject>(rewindParticlePrefabPath);
        if(prefab ==null) Debug.LogError($"Particle is null. Resource Path : {rewindParticlePrefabPath}");
        else
        {
            GameObject rewindPsPosition = GameObject.Find("Position_Rewind_Particle");
            _particleOnRewind = Instantiate(prefab, rewindPsPosition.transform.position,
                rewindPsPosition.transform.rotation).GetComponent<ParticleSystem>();
        }
       
        
    }



    
}
