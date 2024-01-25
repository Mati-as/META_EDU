using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Destroy_prefab : RaySynchronizer
{
    private EffectManager _effectManager;
    private GameObject uiCamera;
    private readonly string GAME_MANAGER = "GameManager";

    private float timer=0f;

    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _effectManager);
    }

    void Start()
    {
        //1212 수정
        base.Start();
        base.Temp_1203();
    }

    public override void ShootRay()
    {
        screenPosition = _uiCamera.WorldToScreenPoint(transform.position);

        //GameManager에서 Cast할 _Ray를 업데이트.. (플레이 상 클릭)
        //  Event처리로 미사용1/19
        //Debug.Assert(_baseEffectManager != null);

        ray_ImageMove = Camera.main.ScreenPointToRay(screenPosition);
        
        //  Event처리로 미사용1/19
        // _baseEffectManager.ray_EffectManager = ray_ImageMove;

#if UNITY_EDITOR
#endif

   


        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
            for (var i = 0; i < results.Count; i++)
            {
#if UNITY_EDITOR
                Debug.Log($"UI 관련 오브젝트 이름: {results[i].gameObject.name}");
#endif
                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
            Destroy_obj();
        }
    }

    void Destroy_obj()
    {
        Destroy(this.gameObject);
    }
}
