using UnityEngine;
using UnityEngine.InputSystem;

public class Space_EffectController : Base_EffectController
{
    private ParticleSystem[] particleSystem;

    private AudioSource[] _adSources;
    private AudioSource[] _burstAdSources;

    public AudioClip _effectClip;
    public AudioClip _burstClip;

    public int audioSize;
    private int _currentCountForBurst;
    private readonly float _returnWaitForSeconds = 3f;
    
    private readonly string LAYER_NAME = "UI";
    private RaycastHit _hit;

    private void Awake()
    {
        Init();
        // SetInputSystem();
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
    

    protected override void Init()
    {
        base.Init();
        // 배열길이 참조 및 크기할당
       
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

        
        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in _particles) particlePool.Enqueue(ps);
    }



    public int waitCount;
    private int _currentCount;

    protected override void OnClicked()
    {
        if (_currentCount > waitCount)
        {
            hits = Physics.RaycastAll(ray_BaseController);
            foreach (var hit in hits)
            {
                PlayParticle(hit.point
                    
                    , wait: 3.4f,usePsLifeTime:true,emitAmount:emitAmount);
            }

            _currentCount = 0;
        }
        else
        {
            _currentCount++;
        }
       
    }

 
}