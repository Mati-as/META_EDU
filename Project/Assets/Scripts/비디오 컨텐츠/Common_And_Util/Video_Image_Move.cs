
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
        Stack<ParticleSystem> particlePool = new Stack<ParticleSystem>();
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _base_effectController);
    }

    public override void ShootRay()
    {
        
        screenPosition = _uiCamera.WorldToScreenPoint(transform.position);
        
        //GameManager에서 Cast할 _Ray를 업데이트.. (플레이 상 클릭)
        Debug.Assert(_base_effectController!=null);
       
        ray = Camera.main.ScreenPointToRay(screenPosition);
        _base_effectController.ray = ray;
       
#if UNITY_EDITOR
#endif
        
        // GameManger에서 Ray 발생시키므로, 아래 로직 미사용 (11/27/23)
        // var ray = Camera.main.ScreenPointToRay(screenPosition);
        // RaycastHit hit;
        // if (Physics.Raycast(ray, out hit)) Debug.Log(hit.transform.name);


        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
            for (var i = 0; i < results.Count; i++)
            {

                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
    }

}
