using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class FP_Prefab : RaySynchronizer
{
    private EffectManager _effectManager;
    private readonly string GAME_MANAGER = "GameManager";

    public FP_controller FPC;
    private float Timer = 0f;
    public static float Limit_Time { get; set; }

    private RectTransform FP;
    private GameObject Image;

    public override void Init()
    {
        base.Init();
     //   GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _effectManager);
     _rectTransform = GetComponent<RectTransform>();

    }

    void OnEnable()
    {

        FP = this.GetComponent<RectTransform>();
        FPC = Manager_Sensor.instance.Get_RPC();
        //Image = this.transform.GetChild(0).gameObject;
        Image = gameObject;
        //Debug.Log(FP.anchoredPosition.x + "," + FP.anchoredPosition.y);
        if (FPC.Check_FPposition(FP))
        {
            Image.SetActive(true);
            //FPC.Add_FPposition(FP);
            //��ġ �߻� (3)
            base.Start();
            base.InvokeRayEvent();
        }
       //  else
       //  {
       //     Destroy_obj();
       //  }
    }

    private Button _btn;
    private RectTransform _rectTransform;
    public override void ShootRay()
    {
        screenPosition = _uiCamera.WorldToScreenPoint(_rectTransform.position);

        initialRay = Camera.main.ScreenPointToRay(screenPosition);


#if UNITY_EDITOR
#endif

        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);
        
        foreach (RaycastResult result in results)
        {
            result.gameObject.TryGetComponent(out _btn);
            _btn?.onClick?.Invoke();
            Debug.Log("Hit " + result.gameObject.name);
            if(_btn?.onClick == null)Debug.Log($"btn.callback is null: {result.gameObject.name}");
        }

        
    }

    // void Update()
    // {
    //     if (Timer < Limit_Time)
    //     {
    //         Timer += Time.deltaTime;
    //     }
    //     else
    //     {
    //         Timer = 0f;
    //       
    //        
    //    //    FPC.Delete_FPposition();
    //         //Destroy_obj();
    //     }
    //    
    // }

    void Destroy_obj()
    {
        Destroy(this.gameObject);
    }
}