using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwlEffectManager : Base_EffectManager
{
    
    void Start()
    {
        Managers.Sound.Play(SoundManager.Sound.Bgm,
            "Audio/Gamemaster Audio - Fun Casual Sounds/Ω_Bonus_Music/music_candyland",volume:0.125f);
    }


}
