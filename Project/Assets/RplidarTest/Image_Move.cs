using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private Camera _mainCamera;
    
    private GraphicRaycaster GR;
    private PointerEventData PED;
    private Vector3 screenPosition;
    private InputAction _spaceAction;
    
    private float movement;
    private Vector3 moveDirection;

    // 현재는 SpaceBar click 시 입니다. 11/27/23
    public static event Action OnStep;

    private void Awake()
    {
        _mainCamera = Camera.main;
        
        _gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        
        //newInputSystem 에서 SpaceBar를 InputAction으로 사용하는 바인딩 로직
        _spaceAction = new InputAction("Space", binding: "<Keyboard>/space", interactions: "press");
        _spaceAction.performed += OnSpaceBarPressed;
        
        //-----가을 소풍에서만 필요한 스크립트(컴포넌트) 입니다.-----
        _storyUIController = GameObject.Find("StoryUI").GetComponent<StoryUIController>();
        _uiAudioController = GameObject.Find("AudioManager").GetComponent<UIAudioController>();
        
    }

    private void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);
    }

    private void OnEnable()
    {
        _spaceAction.Enable();
    }

    private void OnDisable()
    {
        _spaceAction.Disable();
    }

    private void Update()
    {
        Move();
    }
    
    
    
    private void OnSpaceBarPressed(InputAction.CallbackContext context)
    {
        //UI클릭을 위한 RayCast를 발생 및 Ray저장 
        ShootRay();
        
        //GameManager의 RayCast를 발생 
        OnStep?.Invoke();
     
        
        //-----게임이 가을소풍인 경우에만 실행-----
        CloseStoryUI();
    }
    /// <summary>
    /// 1. GameManager에서 로직처리를 위한 ray 정보를 업데이트
    /// 2. UI에 rayCast하고 Button 컴포넌트의 onClick이벤트 실행
    /// </summary>
    private void ShootRay()
    {
        screenPosition = _mainCamera.WorldToScreenPoint(transform.position);
        
        //GameManager에서 Cast할 _Ray를 업데이트.. (플레이 상 클릭)
        _gameManager._ray = Camera.main.ScreenPointToRay(screenPosition);
        
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
                Debug.Log(results[i].gameObject.name);
#endif
                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
    }
    /// <summary>
    /// 가을소풍에서 TimeScale이 0이 되는 로직상에서, UI를 진행시키기 위한 메소드입니다.
    /// 다른게임에서는 해당 메소드를 사용하지 않을 예정입니다 11/27/23
    /// </summary>
    private void CloseStoryUI()
    {
        if (_uiAudioController != null)
        {
            if (_uiAudioController.narrationAudioSource != null
                && !_uiAudioController.narrationAudioSource.isPlaying)
            {
                GameManager.isGameStopped = false;
                _storyUIController.gameObject.SetActive(false);
          
            }
        }
    }


    

    private void Move()
    {
        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        movement = moveSpeed * Time.deltaTime;
        transform.Translate(moveDirection * movement);
    }

    


}