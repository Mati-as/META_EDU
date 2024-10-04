using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class Destroy_prefab : RaySynchronizer
{
    private VideoContentBaseGameManager _videoContentBaseGameManager;
    //private GameObject uiCamera;
    private readonly string GAME_MANAGER = "GameManager";

    private float timer=0f;

    
    public static event Action onPrefabInput; 
    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _videoContentBaseGameManager);
    }

    void Start()
    {
        //1212 수정
        base.Start();
        base.InvokeRayEvent();
    }

    private MetaEduLauncher _launcher;
    public override void ShootRay()
    {
      
        screenPosition = _uiCamera.WorldToScreenPoint(transform.position);
        initialRay = Camera.main.ScreenPointToRay(screenPosition);
  
        
        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
            for (var i = 0; i < results.Count; i++)
            {

                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
        
        if (SceneManager.GetActiveScene().name == "METAEDU_LAUNCHER")
        {
            UI_Canvas.TryGetComponent(out _launcher);

            if (_launcher != null)
            {
#if UNITY_EDITOR
                Debug.Log($"prefabInput invoke-------------------");
#endif
                _launcher.currentPrefabPosition = this.transform.position;
                onPrefabInput.Invoke();
            }
        }

    }

  
    // Update is called once per frame
    // void Update()
    // {
    //     if (timer < FP_Prefab.Limit_Time)
    //     {
    //         timer += Time.deltaTime;
    //     }
    //     else
    //     {
    //         timer = 0f;
    //         //Destroy_obj();
    //     }
    // }

    // void Destroy_obj()
    // {
    //     Destroy(this.gameObject);
    // }
}
