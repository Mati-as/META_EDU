using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hopscotch_EffectManager : VideoContentBaseGameManager
{
    protected override void SetVideo()
    {


        _initiailized = true;
        SCENE_NAME = SceneManager.GetActiveScene().name;

        SetPool(ref _particlePool);
        BindEvent();
    }
    

}
