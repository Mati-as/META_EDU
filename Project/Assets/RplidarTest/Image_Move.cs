using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// -게임마다 Image_Move를 상속받아 클래스 구현
/// 게임이름 + Image_Move
/// </summary>
public class Image_Move : MonoBehaviour
{
    public GameManager _gameManager;

    public float moveSpeed;

    //public Image imageA;
    public GameObject UI_Canvas;
    public Camera _uiCamera;

    
    public GraphicRaycaster GR;
    public PointerEventData PED;
    public Vector3 screenPosition;
    public InputAction _spaceAction;

    public float movement;
    public Vector3 moveDirection;
    public Button button;

    // 현재는 SpaceBar click 시 입니다. 11/27/23
    public static event Action OnStep;


    public void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        //게임 오브젝트 이름과 결합도를 낮추기 위해 Tag사용
        GameObject.FindWithTag("UICamera").TryGetComponent(out _uiCamera);
        GameObject.FindWithTag("GameManager").TryGetComponent(out _gameManager);

        //newInputSystem 에서 SpaceBar를 InputAction으로 사용하는 바인딩 로직
        _spaceAction = new InputAction("Space", binding: "<Keyboard>/space", interactions: "press");
        _spaceAction.performed += OnSpaceBarPressed;
    }

    public void Start()
    {
        SetUIEssentials();
    }

    public virtual void SetUIEssentials()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);
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

    public void Update()
    {
        Move();
    }

    public void OnSpaceBarPressed(InputAction.CallbackContext context)
    {
        //UI클릭을 위한 RayCast를 발생 및 Ray저장 
        ShootRay();

        //GameManager의 RayCast를 발생 
        OnStep?.Invoke();
#if UNITY_EDITOR
 Debug.Log("SpaceBar Pressed");
#endif
    }

    public Ray ray;

    /// <summary>
    ///     1. GameManager에서 로직처리를 위한 ray 정보를 업데이트
    ///     2. UI에 rayCast하고 Button 컴포넌트의 onClick이벤트 실행

    /// </summary>
    public virtual void ShootRay()
    {
        screenPosition = _uiCamera.WorldToScreenPoint(transform.position);
        ray = Camera.main.ScreenPointToRay(screenPosition);

        //GameManager에서 Cast할 _Ray를 업데이트.. (플레이 상 클릭)
        if (_gameManager != null) _gameManager._ray = ray;


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
#if UNITY_EDITOR
                Debug.Log($"UI 관련 오브젝트 이름: {results[i].gameObject.name}");
#endif
                results[i].gameObject.TryGetComponent<Button>(out button);
                button?.onClick?.Invoke();
            }
        
#if UNITY_EDITOR
 
        Debug.Log($"Raycast 오브젝트 갯수: {results.Count}");
        if (button != null)
        {
            Debug.Log($"버튼 클릭 )");
        }
       
#endif

    }
    
    public void Move()
    {
        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        movement = moveSpeed * Time.deltaTime;
        transform.Translate(moveDirection * movement);
    }
}