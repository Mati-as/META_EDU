using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 게임마다 Image_move 방식에서 RaySynchronizer로 클래스 수정
/// 게임마다 ray 동기화는 해당 클래스에서 처리.
/// -1/22/24
/// </summary>
public class RaySynchronizer : MonoBehaviour
{
    public static Ray initialRay { get; set; }
   
    private IGameManager gameManager;
    private GameObject uiCamera;
    
 
    private readonly string GAME_MANAGER = "GameManager";
    //public Image imageA;
    
    public GameObject UI_Canvas;
    public Camera _uiCamera;
    
    public InputAction _spaceAction;
    public GraphicRaycaster GR { get; private set; }
    public PointerEventData PED{ get; private set; }
    public List<RaycastResult> raycastResults{ get; private set; }
    public Vector3 screenPosition;
    public Button button;

    public static event Action OnGetInputFromUser;

    //ball Position 미사옹으로 legacy 1/22
    //public float moveSpeed;
    //public Vector3 moveDirection;
    //public float movement;

  public void Awake()
      {
          Init();
      }  

    public virtual void Init()
    {
        //각 씬의 Overlay-UICamera Tag 할당 필요

        if (SceneManager.GetActiveScene().name != "Launcher_METAEDU") GameObject.FindWithTag("UICamera").TryGetComponent(out _uiCamera);
        GameObject.FindWithTag(GAME_MANAGER).TryGetComponent(out gameManager);
        
        Debug.Assert(gameManager!=null);
        //newInputSystem 에서 SpaceBar를 InputAction으로 사용하는 바인딩 로직
        // _spaceAction = new InputAction("Space", binding: "<Keyboard>/space", interactions: "press");
        _spaceAction = new InputAction("Space", binding: "<Mouse>/leftButton", interactions: "press");
        _spaceAction.performed += OnKeyPressed;
    }

    public void Start()
    {
        SetUIEssentials();
    }

    public void SetUIEssentials()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(EventSystem.current);
    }


    /// <summary>
    ///     OnEnable,Disable에서 InputSystem관련 Action을 사용여부를 끄거나 켜줘야합니다.(구독,해제)
    /// </summary>
    public void OnEnable()
    {
        _spaceAction.Enable();
    }

    public void OnDisable()
    {
        _spaceAction.Disable();
    }

    // public void Update()
    // {
    //     Move();
    // }
    
    
    public void Temp_1203()
    {
        ShootRay();

        //GameManager의 RayCast를 발생 
        OnGetInputFromUser?.Invoke();
    }


    public void OnKeyPressed(InputAction.CallbackContext context)
    {
        //UI클릭을 위한 RayCast를 발생 및 Ray저장 
        ShootRay();
      
    }
    
    /// <summary>
    ///     1. GameManager에서 로직처리를 위한 ray 정보를 업데이트
    ///     2. UI에 rayCast하고 Button 컴포넌트의 onClick이벤트 실행
    /// </summary>
    public virtual void ShootRay()
    {
       
        
        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();
        
        //spacebar 및 공 위치를 기반으로 하고싶은 경우.
        //screenPosition = _uiCamera.WorldToScreenPoint(transform.position);
        
        initialRay = Camera.main.ScreenPointToRay(screenPosition);
        
        PED.position = screenPosition;
        
        raycastResults = new List<RaycastResult>();
        GR.Raycast(PED, raycastResults);

        if (raycastResults.Count > 0)
            for (var i = 0; i < raycastResults.Count; i++)
            {
                raycastResults[i].gameObject.TryGetComponent<Button>(out button);
                button?.onClick?.Invoke();
            }
        

        OnGetInputFromUser?.Invoke();
       
#if UNITY_EDITOR
   
#endif
      
    }
    
    // ball position 사용시 move() 사용
    // public void Move()
    // {
    //     var horizontalInput = Input.GetAxis("Horizontal");
    //     var verticalInput = Input.GetAxis("Vertical");
    //     moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
    //     movement = moveSpeed * Time.deltaTime;
    //     transform.Translate(moveDirection * movement);
    // }
}