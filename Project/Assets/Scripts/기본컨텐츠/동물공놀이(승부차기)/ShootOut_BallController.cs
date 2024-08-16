using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootOut_BallController : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.transform.gameObject.name == "ColliderRight" 
           || other.transform.gameObject.name == "ColliderLeft")
        {
            Managers.soundManager.Play(SoundManager.Sound.Effect,
                "Audio/Gamemaster Audio - Fun Casual Sounds/Ω_150_Bonus_Sounds/bell_small_ringing_03",0.7f,Random.Range(0.8f,1f));
        }
        
       
    }
}
