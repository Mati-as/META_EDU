using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space_EffectControllerGlow : Base_EffectController
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

        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in _particles)
        {
            particlePool.Enqueue(ps);
            ps.gameObject.SetActive(false);
        }
        
        
        
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

       
    }


    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point,
                _adSources, _burstAdSources,
                ref _currentCountForBurst, false, wait: 3.4f,usePsMainDuration:false,emitAmount:1);
            break;
        }
    }
}
