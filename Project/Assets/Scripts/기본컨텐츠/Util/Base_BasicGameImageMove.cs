using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_BasicGameImageMove : Image_Move
{
    private Base_BasicGameManager _baseBasicGameManager;
    private GameObject uiCamera;
    private readonly string GAME_MANAGER = "GameManager";
    

    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _baseBasicGameManager);
    }

    public override void ShootRay()
    { 
        Debug.Assert(_baseBasicGameManager!=null);
        
        base.ShootRay();
        _baseBasicGameManager.ray_GameManager = ray_ImageMove;
        // ray_ImageMove = Camera.main.ScreenPointToRay(screenPosition);
        
       
#if UNITY_EDITOR
        
#endif
 
   
    }
}
