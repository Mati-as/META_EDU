using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playground_ImageMove : Image_Move
{

    private Playground_GameManager playgroundGameManager;
    private GameObject uiCamera;
    
    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag("GameManager").TryGetComponent(out playgroundGameManager);
    }
    
    public override void ShootRay()
    {
        Debug.Assert(playgroundGameManager!=null);
        
        base.ShootRay();
        
        //GameManager에서 Cast할 _Ray를 업데이트.. (UI가 아닌 게임오브젝트 클릭)
        playgroundGameManager.ray = ray_ImageMove;
       
#if UNITY_EDITOR
        Debug.Log($"override ShootRay 호출");
        Debug.Log($"ray point: {playgroundGameManager.ray}");
#endif
        
    }
    
}
