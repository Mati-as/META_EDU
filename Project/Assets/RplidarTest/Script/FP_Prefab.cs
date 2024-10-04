using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class FP_Prefab : RaySynchronizer
{
    private VideoContentBaseGameManager _videoContentBaseGameManager;
    private readonly string GAME_MANAGER = "GameManager";
    private Image _image;

    public FP_controller FPC;
    private float Timer = 0f;
    //public static float Limit_Time { get; set; }

    private RectTransform FP;
    private GameObject Image;
    private static bool _isImageOn;
    
    public static event Action onPrefabInput; 
    private MetaEduLauncher _launcher;

    public override void Init()
    {
        base.Init();
         //   GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out _effectManager);
         _rectTransform = GetComponent<RectTransform>();
         _image = GetComponent<Image>();
    }




    protected override void OnEnable()
    {
        base.OnEnable();
        
        if(_image==null) _image = GetComponent<Image>();
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        
        //모드설정에따라 이미지 활성화 비활성화
        Debug.Assert(_image != null);
        
        _image.enabled = SensorManager.BallActive;
        
        FP = this.GetComponent<RectTransform>();
        FPC = Manager_Sensor.instance.Get_RPC();
        //Image = this.transform.GetChild(0).gameObject;
        Image = gameObject;
        //Debug.Log(FP.anchoredPosition.x + "," + FP.anchoredPosition.y);
        if (FPC.Check_FPposition(FP))
        {
            Image.SetActive(true);
            base.Start();
            base.InvokeRayEvent();
        }

    }

    private void OnDestroy()
    {
        
        Destroy(this.gameObject);
    }

    private Button _btn;
    private RectTransform _rectTransform;
    public override void ShootRay()
    {
        if (Managers.isGameStopped || _rectTransform==null) return;
        
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
            
            result.gameObject.TryGetComponent(out UI_EventHandler eventHandler);
            eventHandler?.OnClickHandler?.Invoke();
       
        }

        
        if (SceneManager.GetActiveScene().name.Contains("METAEDU"))
        {
            GameObject.Find("@LauncherRoot").TryGetComponent(out _launcher);

            if (_launcher != null)
            {
#if UNITY_EDITOR
//                Debug.Log($"prefabInput invoke-------------------");
#endif
                _launcher.currentPrefabPosition = this._rectTransform.position;
                onPrefabInput?.Invoke();
            }
            else
            {
                Logger.Log("laucnher is null");
            }
        }

        
    }
    

}