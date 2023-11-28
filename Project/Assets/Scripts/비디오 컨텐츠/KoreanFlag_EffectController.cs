using UnityEngine;
using UnityEngine.InputSystem;

public class KoreanFlag_EffectController : Base_EffectController
{
    private ParticleSystem[] particleSystems;

    private AudioSource[] _adSources;
    private AudioSource[] _burstAdSources;

    public AudioClip _effectClip;
    public AudioClip _burstClip;

    public int audioSize;
    private readonly int _burstAudioSize = 10;
    private readonly float _returnWaitForSeconds = 3f;

    private int _currentCountForBurst;


    private void Awake()
    {
        Init();
        // SetInputSystem();
    }

//     private void OnMouseClick(InputAction.CallbackContext context)
//     {
//         var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
//         RaycastHit hit;
//
//         if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
//             PlayParticle(hit.point, _adSources, _burstAdSources
//                 , ref _currentCountForBurst
//                 , true, burstCount: 15, emitAmount: 1);
//
// #if UNITY_EDITOR
//         Debug.Log($"currentCountForBust:   {_currentCountForBurst}");
// #endif
//     }

   
    protected override void OnClicked()
    {
        //var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        hits = Physics.RaycastAll(ray);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point, _adSources, _burstAdSources
                , ref _currentCountForBurst
                , true, burstCount: 15, emitAmount: 1);
            break;
        }
       
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


    private readonly string LAYER_NAME = "UI";
    private RaycastHit _hit;

    // public void SetInputSystem()
    // {
    //     _camera = Camera.main;
    //     _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
    //     _mouseClickAction.performed += OnMouseClick;
    // }


    protected override void Init()
    {
        base.Init();

        _adSources = new AudioSource[audioSize];

        for (var i = 0; i < audioSize; i++)
        {
            _adSources[i] = gameObject.AddComponent<AudioSource>();
            _adSources[i].clip = _effectClip;
            _adSources[i].playOnAwake = false;
            _adSources[i].pitch = Random.Range(0.75f, 1.4f);
        }

        _burstAdSources = new AudioSource[_burstAudioSize];

        for (var i = 0; i < _burstAudioSize; i++)
        {
            _burstAdSources[i] = gameObject.AddComponent<AudioSource>();
            _burstAdSources[i].clip = _burstClip;
            _burstAdSources[i].playOnAwake = false;
            _burstAdSources[i].pitch = Random.Range(0.95f, 1.3f);
        }

        wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _particles = GetComponentsInChildren<ParticleSystem>();


        foreach (var ps in _particles)
            if (ps != null)
                particlePool.Push(ps);
    }

}