
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Video_Image_Move : Image_Move
{
    private Base_VideoGameManager vGameManager;
    private GameObject uiCamera;
    private readonly string GAME_MANAGER = "GameManager";
    
  
    
    public override void Init()
    {
        base.Init();
       // GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out vGameManager);
    }

//     public override void ShootRay()
//     { 
//         Debug.Assert(_baseEffectManager!=null);
//         base.ShootRay();
//        // ray_ImageMove = Camera.main.ScreenPointToRay(screenPosition);
//         //_baseEffectManager.ray_EffectManager = ray_ImageMove;
//        
// #if UNITY_EDITOR
//        
// #endif
//         
//     }

}
