using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
///     게임마다 Image_move 방식에서 RaySynchronizer로 클래스 수정
///     게임마다 ray 동기화는 해당 클래스에서 처리.
///     -1/22/24
/// </summary>
public class RaySynchronizer : MonoBehaviour
{
    public static Ray initialRay { get; set; }

  
    //private GameObject uiCamera;


    private readonly string GAME_MANAGER = "GameManager";
    //public Image imageA;

    public GameObject UI_Canvas;
    public Camera _uiCamera;

    [FormerlySerializedAs("_spaceAction")] public InputAction _mouseAction;
    [FormerlySerializedAs("GR")] public GraphicRaycaster graphicRaycaster;
    public PointerEventData PointerEventData { get; private set; }
    public List<RaycastResult> raycastResults { get; protected set; }
    public Vector3 screenPosition;
    public Button btn;

    [SerializeField]
    private bool _isUIClickableBySesnor;

    public bool isRayEnabled = true;

    public static event Action OnGetInputFromUser;

    //ball Position 미사옹으로 legacy 1/22
    //public float moveSpeed;
    //public Vector3 moveDirection;
    //public float movement;



    public virtual void Init()
    { 
        //각 씬의 Overlay-UICamera Tag 할당 필요
       
        GameObject.FindWithTag("UICamera").TryGetComponent(out _uiCamera);
      
        
        // newInputSystem 에서 SpaceBar를 InputAction으로 사용하는 바인딩 로직
        //  _spaceAction = new InputAction("Space", binding: "<Keyboard>/space", interactions: "press");
        
        _mouseAction = new InputAction("Space", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseAction.performed += OnKeyPressed;
        _mouseAction?.Enable();
        
        
    }

    private void OnDestroy()
    {
     
      
        _mouseAction.performed -= OnKeyPressed;
        _mouseAction?.Disable(); // 액션 비활성화
    
    }
    


    public void Start()
    {
        Init();
        SetUIEssentials();
    }

    public void SetUIEssentials()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        graphicRaycaster = UI_Canvas.GetComponent<GraphicRaycaster>();
        PointerEventData = new PointerEventData(EventSystem.current);
    }


    /// <summary>
    ///     OnEnable,Disable에서 InputSystem관련 Action을 사용여부를 끄거나 켜줘야합니다.(구독,해제)
    /// </summary>
    protected virtual void OnEnable()
    {
       

        Debug.Assert(_mouseAction != null);
        _mouseAction.Enable();
    }

    public void OnDisable()
    {
        Debug.Assert(_mouseAction != null);
        _mouseAction.Disable();
    }

    public void InvokeRayEvent()
    {
        ShootRay();

        //GameManager의 RayCast를 발생 
        OnGetInputFromUser?.Invoke();
    }


    public void OnKeyPressed(InputAction.CallbackContext context)
    {
        //UI클릭을 위한 RayCast를 발생 및 Ray저장
        ShootRay();
        OnGetInputFromUser?.Invoke();
    }

    /// <summary>
    ///     1. GameManager에서 로직처리를 위한 ray 정보를 업데이트
    ///     2. UI에 rayCast하고 Button 컴포넌트의 onClick이벤트 실행
    /// </summary>
    public virtual void ShootRay()
    {

        //if (!isRayEnabled) return; //Raycast 작동 여부 제어

        if (Managers.UserInfo.CurrentActiveSceneName.Contains("LAUNCHER"))
        {
            Logger.Log($"런처에서는 센서동작하지 않음 ------------------{Managers.UserInfo.CurrentActiveSceneName}");
            return;
        }
        
        
        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();
        // check if the pointer is over any ui elements
     
        
        Logger.SensorRelatedLog("클릭 From Raysynchronizer");
        //spacebar 및 공 위치를 기반으로 하고싶은 경우.
        //screenPosition = _uiCamera.WorldToScreenPoint(transform.position);
        
        initialRay = Camera.main.ScreenPointToRay(screenPosition);

        PointerEventData.position = screenPosition;

        raycastResults = new List<RaycastResult>();
        graphicRaycaster.Raycast(PointerEventData, raycastResults);


        if (_isUIClickableBySesnor)
            foreach (var result in raycastResults)
            {
                result.gameObject.TryGetComponent(out btn);
                btn?.onClick?.Invoke();

                result.gameObject.TryGetComponent(out UI_EventHandler eventHandler);
                eventHandler?.OnClickHandler?.Invoke();
            }


        //  OnGetInputFromUser?.Invoke();

#if UNITY_EDITOR

#endif
    }
}