using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Image_Move : MonoBehaviour
{
    public GameManager _gameManager;
    [SerializeField] private UIAudioController _uiAudioController;

    [SerializeField] private StoryUIController _storyUIController;

    public float moveSpeed;
    public Image imageA;
    private GameObject UI_Canvas;
    private Camera _uiCamera;
    private Camera _mainCamera;

    private GraphicRaycaster GR;
    private PointerEventData PED;
    private Vector3 Temp_position;

    private InputAction _spaceAction;

    // 현재는 SpaceBar click 시 입니다. 11/27/23
    public static event Action OnStep;

    private void Awake()
    {
        _uiCamera = GameObject.Find("UICamera")?.GetComponent<Camera>();
        _mainCamera = GameObject.Find("UICamera")?.GetComponent<Camera>();

        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        _storyUIController = GameObject.Find("StoryUI").GetComponent<StoryUIController>();
        _uiAudioController = GameObject.Find("AudioManager").GetComponent<UIAudioController>();

        _uiCamera = Camera.main;
        _spaceAction = new InputAction("Space", binding: "<Keyboard>/space", interactions: "press");
        _spaceAction.performed += OnSpaceBarPressed;

        // _spaceAction.performed += _gameManager.OnClick();
    }

    private void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        _uiCamera = Camera.main;

        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);

        StartCoroutine(MoveObject());
    }

    private void Update()
    {
        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        movement = moveSpeed * Time.deltaTime;

        // if (Input.GetKeyDown(KeyCode.Space)) ShootRay();
    }

    private float movement;
    private Vector3 moveDirection;


    private IEnumerator MoveObject()
    {
        while (true)
        {
            transform.Translate(moveDirection * movement);
            yield return null; // 한 프레임 기다림
        }
    }

    private void ShootRay()
    {
        Temp_position = _uiCamera.WorldToScreenPoint(transform.position);


        var ray = Camera.main.ScreenPointToRay(Temp_position);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) Debug.Log(hit.transform.name);


        PED.position = Temp_position;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
            for (var i = 0; i < results.Count; i++)
            {
#if UNITY_EDITOR
                Debug.Log(results[i].gameObject.name);
#endif
                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
    }


    private void OnEnable()
    {
        _spaceAction.Enable();
    }

    private void OnDisable()
    {
        _spaceAction.Disable();
    }


    private void OnSpaceBarPressed(InputAction.CallbackContext context)
    {
        MoveMouseToCurrentObjectPosition();
        _gameManager._ray = Camera.main.ScreenPointToRay(Temp_position);
       
        ShootRay();


        if (!_uiAudioController.narrationAudioSource.isPlaying)
        {
            GameManager.isGameStopped = false;
            _storyUIController.gameObject.SetActive(false);
            OnStep?.Invoke();
        }
        
       
        //  ExecuteButtonClick();
    }

    public static Vector3 screenPosition;

    private void MoveMouseToCurrentObjectPosition()
    {   Vector3 objectPosition = transform.position;
        screenPosition = _uiCamera.WorldToScreenPoint(objectPosition);
    
        // 마우스의 위치를 원하는 위치로 설정
        //Mouse.current.WarpCursorPosition(new Vector2(screenPosition.x, screenPosition.y));
    }
    

    /// <summary>
    /// UI도 클릭 가능하게 하는 메소드 입니다. 
    /// </summary>
    // private void ExecuteButtonClick()
    // {
    //     PED =new PointerEventData(EventSystem.current);
    //     PED.position = screenPosition;
    //
    //     List<RaycastResult> results = new List<RaycastResult>();
    //     EventSystem.current.RaycastAll(PED, results);
    //
    //     foreach (RaycastResult result in results)
    //     {
    //         if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
    //         {
    //             // UI 버튼을 찾아 클릭 이벤트 실행
    //             ExecuteEvents.Execute(result.gameObject, PED, ExecuteEvents.submitHandler);
    //             break;
    //         }
    //     }
    // }
}