using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring_EffectController : Base_EffectController
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
   

    private void Awake()
    {
        Init();
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

        
        _particles = new ParticleSystem[transform.childCount];
        int index = 0;
        foreach (Transform child in transform)
        {
            ParticleSystem ps = child.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                _particles[index++] = ps;
            }
        }

        foreach (var ps in _particles)
        {
            particlePool.Enqueue(ps);
            ps.gameObject.SetActive(false);
        }
    }



    public int waitCount;
    private int _currentCount;
    private RaycastHit hit;

    protected override void OnClicked()
    {
        
#if UNITY_EDITOR
        Debug.Log("OnClicked 실행 , 재생여부 모름");
#endif

            hits = Physics.RaycastAll(ray_BaseController);
            foreach (var hit in hits)
            {
#if UNITY_EDITOR
                Debug.Log("OnClicked재생");
#endif
                PlayParticle(hit.point,
                    _adSources, _burstAdSources,
                    ref _currentCountForBurst, false,usePsMainDuration:true,emitAmount:emitAmount);
               
                break;
            }
        
       
    }
}
