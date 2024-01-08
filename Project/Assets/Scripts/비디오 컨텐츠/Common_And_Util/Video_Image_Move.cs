
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Video_Image_Move : Image_Move
{
    private Base_EffectController _base_effectController;
    private GameObject uiCamera;
    private readonly string GAME_MANAGER = "GameManager";
    
  
    
    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _base_effectController);
    }

    public override void ShootRay()
    { 
        Debug.Assert(_base_effectController!=null);
        base.ShootRay();
       // ray_ImageMove = Camera.main.ScreenPointToRay(screenPosition);
        _base_effectController.ray_BaseController = ray_ImageMove;
       
#if UNITY_EDITOR
       
#endif
        
    }

}
