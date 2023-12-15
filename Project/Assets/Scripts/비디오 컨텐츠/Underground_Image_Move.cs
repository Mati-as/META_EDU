using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Underground_Image_Move : Image_Move
{

    private FootstepManager _footstepManager;
    private GameObject uiCamera;
    
    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag("GameManager").TryGetComponent(out _footstepManager);
    }


    public override void ShootRay()
    {
        Debug.Assert(_footstepManager!=null);
        
        base.ShootRay();
        
        //GameManager에서 Cast할 _Ray를 업데이트.. (UI가 아닌 게임오브젝트 클릭)
        _footstepManager.ray = ray_ImageMove;
       
#if UNITY_EDITOR
        Debug.Log($"override ShootRay 호출");
        Debug.Log($"ray point: {_footstepManager.ray}");
#endif
        
//         // GameManger에서 Ray 발생시키므로, 아래 로직 미사용 (11/27/23)
//         // var ray = Camera.main.ScreenPointToRay(screenPosition);
//         // RaycastHit hit;
//         // if (Physics.Raycast(ray, out hit)) Debug.Log(hit.transform.name);
//
//         
//         
//         
//         // UI 클릭 로직
//         PED.position = screenPosition;
//         var results = new List<RaycastResult>();
//         GR.Raycast(PED, results);
//
//         if (results.Count > 0)
//             for (var i = 0; i < results.Count; i++)
//             {
// #if UNITY_EDITOR
//                 Debug.Log($"UI 관련 오브젝트 이름: {results[i].gameObject.name}");
// #endif
//                 results[i].gameObject.TryGetComponent(out Button button);
//                 button?.onClick?.Invoke();
//             }
    }
    
  
  
    
    
    
    
}
