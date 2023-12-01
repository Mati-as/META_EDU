using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimalTrip_Image_Move : Image_Move
{
    private AnimalTrip_GameManager _animalTripGameManager;
    
    
    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag("GameManager").TryGetComponent(out _animalTripGameManager);
    }
 
    
    public override void ShootRay()
    {
        Debug.Assert(_animalTripGameManager!=null);
        
        base.ShootRay();
        
        //GameManager에서 Cast할 _Ray를 업데이트.. (플레이 상 클릭)
        _animalTripGameManager._ray = ray;
       
#if UNITY_EDITOR
        Debug.Log($"override ShootRay 호출");
        Debug.Log($"ray point: {_animalTripGameManager._ray}");
#endif

    }
}

