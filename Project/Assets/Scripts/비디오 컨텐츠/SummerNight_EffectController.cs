using UnityEngine;
using UnityEngine.InputSystem;


public class SummerNight_EffectController : Base_EffectController
{
      private ParticleSystem[] particleSystem;

    private AudioSource[] _adSources;
    private AudioSource[] _burstAdSources;

    public AudioClip _effectClip;
    public AudioClip _burstClip;

    public int audioSize;
    private int _currentCountForBurst;
    private float _returnWaitForSeconds = 25f;
    
    private readonly string LAYER_NAME = "UI";
    private RaycastHit _hit;

    [Space(20f)]
    
    [Range(0.5f,2f)]
    public float pitch;
    [Range(0.5f,2f)]
    public float pitch_brust;
    [Range(0f,1f)]
    public float interval;

    

    private void Awake()
    {
        Init();
        //SetInputSystem();
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
      
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point, _adSources, _burstAdSources
                , ref _currentCountForBurst
                , true);
        }
       
    }

    protected override void Init()
    {
        burstAmount = 100;
        base.Init();

        _adSources = new AudioSource[audioSize];

        for (var i = 0; i < audioSize; i++)
        {
            _adSources[i] = gameObject.AddComponent<AudioSource>();
            _adSources[i].clip = _effectClip;
            _adSources[i].playOnAwake = false;
            _adSources[i].pitch = Random.Range(pitch - interval, pitch + interval);
        }

        _burstAdSources = new AudioSource[_burstAudioSize];

        for (var i = 0; i < _burstAudioSize; i++)
        {
            _burstAdSources[i] = gameObject.AddComponent<AudioSource>();
            _burstAdSources[i].clip = _burstClip;
            _burstAdSources[i].playOnAwake = false;
            _burstAdSources[i].pitch = Random.Range(pitch_brust - interval, pitch_brust + interval);
        }

        wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in _particles) particlePool.Enqueue(ps);
    }

    // public void SetInputSystem()
    // {
    //     _camera = Camera.main;
    //     _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
    //     _mouseClickAction.performed += OnMouseClick;
    // }


    // private void OnMouseClick(InputAction.CallbackContext context)
    // {
    //     var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
    //     RaycastHit hit;
    //
    //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
    //         PlayParticle(hit.point,
    //             _adSources, _burstAdSources,
    //             ref _currentCountForBurst, true, wait: _returnWaitForSeconds);
    // }
}
