using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painting_Image_Move : Image_Move
{
    private IGameManager gameManager;
    private GameObject uiCamera;
    private readonly string GAME_MANAGER = "GameManager";
    
  
    
    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out gameManager);
    }

    public override void ShootRay()
    { 
        Debug.Assert(gameManager!=null);
        base.ShootRay();
        // ray_ImageMove = Camera.main.ScreenPointToRay(screenPosition);
        
       
#if UNITY_EDITOR
       
#endif
        
    }
}
