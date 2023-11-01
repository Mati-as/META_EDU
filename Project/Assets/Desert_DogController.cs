using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Desert_DogController : MonoBehaviour
{
    public readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    public readonly int SIT_ANIM = Animator.StringToHash("Sit");
    public readonly int WALK_ANIM = Animator.StringToHash("Walk");
    
    public Transform[] waypoints;
    private Animator _animator;
    private Camera _camera;

    public static bool[] isOnThisPlace = new bool[6];
    private int _currentLocationIndex;
    private int _nextLocationIndex;
    
    [Range(0,50)]
    public float intervalMin;
    [Range(0,50)]
    public float intervalMax;
    private InputAction _mouseClickAction;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
        
        _camera = Camera.main;
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;

    }

    private void Start()
    {
        _camera = Camera.main;
        WalkOnPath();
    }

    private void OnEnable()
    {
        _mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        _mouseClickAction.Disable();
    }

    [Range(0,50)]
    public float loopDuration;
    private Tween _pathTween;
    
    
    private void WalkOnPath()
    {
        _pathTween?.Kill();
        
        _animator.SetBool(WALK_ANIM,true);
        
     
        _pathTween = transform
            .DOPath
            (waypoints.Select(w => w.position)
                .ToArray(), loopDuration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .SetSpeedBased()
            .SetLoops(-1)
            .SetLookAt(0.01f)
            .OnWaypointChange(WaypointReached)
            .OnComplete(WalkOnPath);
    }
    
    private void OnMouseClick(InputAction.CallbackContext context)
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        
#if UNITY_EDITOR
        Debug.Log("Dog Clicked!");
#endif

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
#if UNITY_EDITOR
                Debug.Log($"{this.gameObject.name} is Clicked On!");
#endif
                _onClickedCoroutine= StartCoroutine(OnClicked());
            }
        }
    }

    private float _currentInterval;
    private void WaypointReached(int waypointIndex)
    {
        _currentInterval = Random.Range(intervalMin, intervalMax);
#if  UNITY_EDITOR
        Debug.Log("Reached waypoint: " + waypointIndex);
#endif
        _animator.SetBool(WALK_ANIM,false);
        transform.DOPause();
        DOVirtual.DelayedCall(_currentInterval, () =>
        {
            _animator.SetBool(WALK_ANIM,true);
            transform.DOPlay();
        });
    }
    
    private Collider _collider;
    private bool _isClicked;
    private Coroutine _onClickedCoroutine;
    private float elapsed;
     public float waypointAnimationInterval;
    
    private IEnumerator OnClicked()
    {
        if (!_isClicked)
        {
            _isClicked = true;
            _pathTween.Pause();
            elapsed = 0f;

            _animator.SetBool(SIT_ANIM, true);
            _collider.enabled = false;
                
            while (true)
            {
                _currentInterval = 987654321;
                elapsed += Time.deltaTime;
                if (elapsed > waypointAnimationInterval)
                {
                    break;
                }

                yield return null;
            }
            
            _animator.SetBool(SIT_ANIM, false);
            _collider.enabled = true;
            _currentInterval = Random.Range(intervalMin, intervalMax);
            _pathTween.Play();
            _isClicked = false;
            if(_onClickedCoroutine!=null)
            StopCoroutine(_onClickedCoroutine);
            
        }
    
    }
}
