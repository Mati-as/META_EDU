using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class Desert_CrowController : MonoBehaviour
{
      private enum Sound
    {
        Squeak,
        Dive
    }
      
 


  

      [Header("DoTween Parameters")] public float speed;
    public int vibrato;

    public readonly int IDLE_ANIM = Animator.StringToHash("idle");
    public readonly int EAT_ANIM = Animator.StringToHash("Eat");
    public readonly int SWIM_ANIM = Animator.StringToHash("Swim");
    public readonly int FAST_RUN_ANIM = Animator.StringToHash("FastRun");
    public readonly int FLY_ANIM = Animator.StringToHash("Fly");
    public readonly int IS_ON_LAKE = Animator.StringToHash("IsOnLake");

    public Transform[] landableTransforms = new Transform [8];
    public static bool[] isOnThisPlace = new bool[8];
    private int _nextLocationIndex;
    
    public Transform[] jumpSurpPath = new Transform[3];
    private readonly Vector3[] _jumpSurpPathVec = new Vector3[3];

    public Transform[] patrolPath = new Transform[4];
    private readonly Vector3[] _patrolPathVec = new Vector3[4];

    private float _defaultYCoordinate;
    private Animator _animator;
    private float _defaultAnimSpeed;

    private ParticleSystem _particle;

    //더블클릭 방지용으로 콜라이더를 설정하기위한 인스턴스 선언
    private Collider _collider;
    private AudioSource _audioSourceDive;
    private AudioSource _audioSourceSqueak;
    public AudioClip[] audioClips;
    
    
    private Camera _camera;
    private InputAction _mouseClickAction;

    private void Awake()
    {
        _camera = Camera.main;
        
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;
        
        _particle = GetComponentInChildren<ParticleSystem>();
        _collider = GetComponent<Collider>();

    
        // var audioSources = GetComponents<AudioSource>();
        // _audioSourceDive = audioSources[(int)Sound.Dive];
        // _audioSourceSqueak = audioSources[(int)Sound.Squeak];


        // //DoPath,Shake등을 사용 후 Y값이 일정하지 않게 변하는 것을 방지하기 위해 사용.
        // _defaultYCoordinate = transform.position.y;
        // for (var i = 0; i < 4; i++) _patrolPathVec[i] = patrolPath[i].position;

        _animator = GetComponent<Animator>();
        _defaultAnimSpeed = _animator.speed;
        _animator.SetBool(IS_ON_LAKE, true);
    }
    
    private void Start()
    {
        _camera = Camera.main;
    }


    private void OnEnable()
    {
        _mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        _mouseClickAction.Disable();
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        #if UNITY_EDITOR
        Debug.Log("Crow Clicked!");
        #endif

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                OnClicked();
            }
        }
    }

    

    [Range(0, 40)]
  
    private Coroutine _flyCoroutine;

    private float _elapsedForNextPatol;
    public float patrolInterval; 
//     // ReSharper disable Unity.PerformanceAnalysis
//     private IEnumerator FlyAround()
//     {
//         _elapsedForNextPatol = 0f;
//         _nextLocationIndex = Random.Range(0, 8);
//         
//         //까마귀 중복된 곳 위치하는 경우 방지
//         while (isOnThisPlace[_nextLocationIndex] == true)
//         {
//             _nextLocationIndex = Random.Range(0, 8);
//         }
//         
//         _elapsedForNextPatol = 0f;
//         float randomizedInterval = patrolInterval + Random.Range(-5, 5);
//
//         while (_elapsedForNextPatol <= randomizedInterval && !_isClicked)
//         {
//             _elapsedForNextPatol += Time.deltaTime;
//             yield return null;
//         }
//         
//         
//         transform
//             .DOMove(landableTransforms[_currentLocationIndex].position, speed)
//             .OnStart(() =>
//             {
// #if UNITY_EDITOR
//                 Debug.Log($"{gameObject.name} 순찰중..");
// #endif
//                 _currentLocationIndex = _nextLocationIndex;
//                 isOnThisPlace[_nextLocationIndex] = true;
//             })
//             .SetSpeedBased()
//             .OnComplete(() =>
//             {
//                 _collider.enabled = true;
//                 _isClicked = false;
//                 StartCoroutine(FlyAround());
//             });
//     
//     }

    private bool _isClicked;
    public static bool[] _isOnThisPlace = new bool[8];
    private int _currentLocationIndex;

    private void OnClicked()
    {
        if (!_isClicked)
        {
            _isClicked = true;
        
#if UNITY_EDITOR
            Debug.Log($"Crow is Clicked On!");
#endif

            _animator.SetBool(FLY_ANIM, true);
            _nextLocationIndex = Random.Range(0, 8);
        
            while (_isOnThisPlace[_nextLocationIndex])
            {
                _nextLocationIndex = Random.Range(0, 8);
            }

            // Set the value of the new location to `true`.
         
            _isOnThisPlace[_nextLocationIndex] = true;

            transform.DOLookAt(landableTransforms[_nextLocationIndex].position, 0.55f)
                .OnComplete(() =>
                {
                    // Move to the new location.
                    transform
                        .DOMove(landableTransforms[_nextLocationIndex].position, speed)
                        .OnStart(() =>
                        {
#if UNITY_EDITOR
                            Debug.Log($"{gameObject.name} 순찰중..");
#endif
                            _isOnThisPlace[_currentLocationIndex] = false;
                            _currentLocationIndex = _nextLocationIndex;
                        })
                        .SetSpeedBased()
                        .OnComplete(() =>
                        {
                            _animator.SetBool(FLY_ANIM, false);
                            _collider.enabled = true;
                            _isClicked = false;
                        });
                });
        }
    
    }
}
