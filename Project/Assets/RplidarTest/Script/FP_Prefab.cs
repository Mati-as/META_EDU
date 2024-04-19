using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class FP_Prefab : RaySynchronizer
{
    private EffectManager _effectManager;
    private readonly string GAME_MANAGER = "GameManager";

    public FP_controller FPC;
    private float Timer = 0f;
    public static float Limit_Time = 1.3f;

    private RectTransform FP;
    private GameObject Image;

    public override void Init()
    {
        base.Init();
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _effectManager);
      
    }

    void Start()
    {

        FP = this.GetComponent<RectTransform>();
        FPC = Manager_Sensor.instance.Get_RPC();
        Image = this.transform.GetChild(0).gameObject;

        //Debug.Log(FP.anchoredPosition.x + "," + FP.anchoredPosition.y);
        if (FPC.Check_FPposition(FP))
        {
            Image.SetActive(true);
            FPC.Add_FPposition(FP);
            //��ġ �߻� (3)
            base.Start();
            base.InvokeRayEvent();
        }
        else
        {
            Destroy_obj();
        }
    }
    public override void ShootRay()
    {
        screenPosition = _uiCamera.WorldToScreenPoint(transform.position);

        initialRay = Camera.main.ScreenPointToRay(screenPosition);


#if UNITY_EDITOR
#endif


        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
            for (var i = 0; i < results.Count; i++)
            {
#if UNITY_EDITOR
                //Debug.Log($"UI ���� ������Ʈ �̸�: {results[i].gameObject.name}");
#endif
                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
    }

    void Update()
    {
        if (Timer < Limit_Time)
        {
            Timer += Time.deltaTime;
        }
        else
        {
            Timer = 0f;
          
           
            FPC.Delete_FPposition();
            Destroy_obj();
        }
       
    }

    void Destroy_obj()
    {
        Destroy(this.gameObject);
    }
}