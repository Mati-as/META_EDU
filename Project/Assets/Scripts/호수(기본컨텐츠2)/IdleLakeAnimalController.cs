using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
#endif


public class IdleLakeAnimalController : MonoBehaviour
{
    private Animator _animator;
    private AudioSource _audioSource;
    public AudioClip _audioClip;
    public readonly int ON_CLICK_ANIM = Animator.StringToHash("Click");
    private Collider _collider;

    private Camera _camera;
    private InputAction _mouseClickAction;


    private void Awake()
    {
        _camera = Camera.main;
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;


        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
        _audioSource.clip = _audioClip;

        IdleLakeAnimalSetter.onArrival -= onArritvalAtLake;
        IdleLakeAnimalSetter.onLeaving -= OnLeavingFromLake;

        IdleLakeAnimalSetter.onArrival += onArritvalAtLake;
        IdleLakeAnimalSetter.onLeaving += OnLeavingFromLake;
    }

    private void OnDestroy()
    {
        IdleLakeAnimalSetter.onArrival -= onArritvalAtLake;
        IdleLakeAnimalSetter.onLeaving -= OnLeavingFromLake;
    }

    private void onArritvalAtLake()
    {
        _isClickable = true;
    }

    private void OnLeavingFromLake()
    {
        _isClickable = false;
    }

    private void OnEnable()
    {
        _mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        _mouseClickAction.Disable();
    }

    // private void Start()
    // {
    //    
    // }

    //DoVirtual -> 콜백사용을 위한 float선언
    private float _rubbishNum;
    public float soundDuration;

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
            if (hit.collider.gameObject == gameObject)
            {
                OnClicked();
                break;
            }
    }

    private bool _isClickable;
    private bool _isClicked;

    public Vector3 duck_rotationAngles;
    public Vector3 dog_rotationAngles; // 회전할 각도
    public float duration = 0.55f; // 회전하는 데 걸리는 시간 (초 단위)
    public Ease easeType = Ease.InOutQuad; // 회전의 이징 타입 (선택적)
    public LoopType loopType = LoopType.Incremental; // 반복 타입 (선택적)
    public int loops = 3; // 반복 횟수 (-1은 무한 반복)

    private void OnClicked()
    {
        //Clickable은 AnimalSetter에서 eventque방식으로 관리, isClciked 중복클릭방지를 위해 방어적 프로그래밍 적용
        if (!_isClicked && _isClickable)
        {
#if UNITY_EDITOR
            Debug.Log($"{gameObject.name} Clicked! : ONCLICK FUNCTION IS ON..");
#endif
            _isClicked = true;

            _animator.SetTrigger(ON_CLICK_ANIM);

            SoundManager.FadeInAndOutSound(_audioSource, fadeWaitTime: 3.5f);

            /*
             * rubbishNum은 아무런 역할도 하지않습니다.
             * 단 SoundDuration 만큼 대기 후, collider.enabled = false; 콜백사용을 위해 선언하였습니다.
             */

            DOVirtual.Float(0, 1, soundDuration, value => _rubbishNum = value).OnComplete(() =>
            {
                _isClicked = false;
            });

            if (gameObject.name == "오리")
                transform.DORotate(duck_rotationAngles, duration, RotateMode.FastBeyond360)
                    .SetEase(easeType)
                    .SetLoops(loops);

            if (gameObject.name == "개")
                transform.DORotate(dog_rotationAngles, duration)
                    .SetEase(easeType)
                    .OnComplete(
                        () =>
                        {
                            transform.DORotate(-dog_rotationAngles, duration)
                                .SetEase(easeType);
                        });
        }
    }
}