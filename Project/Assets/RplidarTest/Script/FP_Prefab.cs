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
    private Image _imageComponent;

    public FP_controller FPC;
    private float Timer = 0f;
    //public static float Limit_Time { get; set; }

    private RectTransform FP;
    private GameObject Image;
    private static bool _isImageOn;
    
    public static event Action onPrefabInput; 
    private MetaEduLauncher _launcher;
    private string name =string.Empty;

    //public bool isRayEnabled = true;

    private void Awake()
    {
        name = gameObject.name;
        _imageComponent = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public override void Init()
    {
        
        GameObject.FindWithTag("UICamera").TryGetComponent(out _uiCamera);
        
         _rectTransform = GetComponent<RectTransform>();
         _imageComponent = GetComponent<Image>();
       
    }


    protected override void OnEnable()
    {
        
        if (name.Contains("Real"))
        {
            _imageComponent.enabled = SensorManager.isRealImageActive;
            if (!SensorManager.isRealRayActive)
            {
                return;
            }
        }
        
        if (name.Contains("Normal"))
        {
            _imageComponent.enabled = SensorManager.isNormalImageActive;
            if (!SensorManager.isNormalRayActive)
            {
                return;
            }
        }
        
        
        base.OnEnable();
        

        Logger.SensorRelatedLog($"FP_Prefab OnEnable{gameObject.name}");
        //모드설정에따라 이미지 활성화 비활성화
        Debug.Assert(_imageComponent != null);

      
        //[수정] BallActive 상관없이 구동될 수 있도록
        //런처에서는 자동 꺼지되 콘텐츠에서는 자동으로 켜지므로 비활성화함
        //_image.enabled = true;

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
    private string excludeMask = "Non_Sensor_Interactable_UIs";
    public override void ShootRay()
    {
        if (!isRayEnabled) return; //Raycast 작동 여부 제어

        if (Managers.isGameStopped || _rectTransform == null) return;
        if (Managers.UserInfo.CurrentActiveSceneName.Contains("LAUNCHER"))
        {
            return;
        }

        screenPosition = _uiCamera.WorldToScreenPoint(_rectTransform.position);
        initialRay = Camera.main.ScreenPointToRay(screenPosition);

      
        int layerMask = ~LayerMask.GetMask(excludeMask);
        
        if (Physics.Raycast(initialRay, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.Log($"Hit Object: {hit.collider.gameObject.name}");
        }

        // 🔹 UI 요소 Raycast (GraphicRaycaster 사용)
        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        foreach (RaycastResult result in results)
        {
            // 🚨 특정 레이어 제외 (Non_Sensor_Interactable_UIs 레이어 감지 안 함)
            if (result.gameObject.layer == LayerMask.NameToLayer(excludeMask))
            {
                //Logger.LogError("센서랑 상호작용안함-----------");
                continue;
            }

            //마우스 클릭시 두번이 발생하는 부분은 아래 때문
            result.gameObject.TryGetComponent(out _btn);
            _btn?.onClick?.Invoke();

            result.gameObject.TryGetComponent(out UI_EventHandler eventHandler);
            eventHandler?.OnClickHandler?.Invoke();
        }

        // 🚨 최적화
        if (SceneManager.GetActiveScene().name.Contains("METAEDU"))
        {
            if (_launcher == null)
            {
                GameObject launcherObj = GameObject.Find("@LauncherRoot");
                launcherObj?.TryGetComponent(out _launcher);
            }

            if (_launcher != null)
            {
                _launcher.currentPrefabPosition = this._rectTransform.position;
                onPrefabInput?.Invoke();
            }
            else
            {
                Logger.Log("Launcher is null");
            }
        }
    }
    }
    

