using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public abstract class Base_EffectController : MonoBehaviour
{
    public ParticleSystem[] _particles;
    private int _currentCountForBurst;
    private readonly float _returnWaitForSeconds = 3f;
    public ParticleSystem[] particleSystem;

    [HideInInspector] public Camera _camera;
    public InputAction _mouseClickAction;
    public Queue<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
    public WaitForSeconds subWait_;

    [Header("Particle Emission Setting")] private int _count;
    public int emitAmount;
    public float targetVol;
    
    [Header("Burst Setting")]
    public int burstAmount;
    public int burstCount;

    [Header("SubEmitter Setting")] 
    public float subEmitLifetime;
    
    [Header("Particle Emission Setting")] 

    [Header("Audio Setting")]
    public int audioSize;
    public int _burstAudioSize;

    public AudioClip _effectClip;
    public AudioClip _subAudioClip;
    public AudioClip _burstClip;

    private AudioSource[] _audioSources;
    private AudioSource[] _subAudioSources;
    private AudioSource[] _burstAudioSources;


    public Ray ray_BaseController { get; set; }
    public RaycastHit[] hits;
    protected abstract void OnClicked();

    protected virtual void Init()
    {
        SetPool();
        SetAudio();
        BindEvent();
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        _mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        _mouseClickAction.Disable();
    }

    protected virtual void OnDestroy()
    {
        Image_Move.OnStep -= OnClicked;
    }

    protected virtual void SetPool()
    {
        particlePool = new Queue<ParticleSystem>();
        _particles = new ParticleSystem[transform.childCount];
        var index = 0;
        foreach (Transform child in transform)
        {
            var ps = child.GetComponent<ParticleSystem>();
            if (ps != null) _particles[index++] = ps;
        }

        foreach (var ps in _particles)
        {
            particlePool.Enqueue(ps);
            ps.gameObject.SetActive(false);
        }
    }

    protected virtual void BindEvent()
    {
        Image_Move.OnStep -= OnClicked;
        Image_Move.OnStep += OnClicked;
    }

    private void SetAudio()
    {
        _audioSources = new AudioSource[audioSize];

        for (var i = 0; i < audioSize; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].clip = _effectClip;
            _audioSources[i].spatialBlend = 0f;
            _audioSources[i].outputAudioMixerGroup = null;
            _audioSources[i].playOnAwake = false;
            _audioSources[i].pitch = Random.Range(0.75f, 1.4f);
        }

        _subAudioSources = new AudioSource[audioSize + 10];
        
        for (var i = 0; i < audioSize; i++)
        {
            _subAudioSources[i] = gameObject.AddComponent<AudioSource>();
           
            _subAudioSources[i].clip = _subAudioClip;
            _subAudioSources[i].spatialBlend = 0f;
            _subAudioSources[i].outputAudioMixerGroup = null;
            _subAudioSources[i].playOnAwake = false;
            _subAudioSources[i].pitch = Random.Range(0.9f, 1.2f);
        }
        
        
        _burstAudioSources = new AudioSource[_burstAudioSize];

        for (var i = 0; i < _burstAudioSize; i++)
        {
            _burstAudioSources[i] = gameObject.AddComponent<AudioSource>();
            _burstAudioSources[i].clip = _burstClip;
            _burstAudioSources[i].spatialBlend = 0f;
            _burstAudioSources[i].outputAudioMixerGroup = null;
            _burstAudioSources[i].playOnAwake = false;
            _burstAudioSources[i].pitch = Random.Range(0.95f, 1.3f);
        }
        
        
      
    }

    protected void GrowPool(ParticleSystem original, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var newInstance = Instantiate(original, transform);
            newInstance.gameObject.SetActive(false);
            particlePool.Enqueue(newInstance);
        }
    }



    protected virtual void PlayParticle(Vector3 position, bool isBurstMode = false,
        int emitAmount = 2, int burstCount = 10, int burstAmount = 5, float wait = 3f, bool usePsLifeTime = false
        , bool useSubEmitter = false)
    {
        //UnderFlow를 방지하기 위해서 선제적으로 GrowPool 실행 
        if (particlePool.Count < emitAmount || particlePool.Count < burstCount)
        {
            // 에디터상에서 배치한 순서대로 파티클을 Push하기 위해 for문 사용합니다. 
            for (var i = 0; i < burstAmount; i++)
                foreach (var ps in _particles)
                    GrowPool(ps);

#if UNITY_EDITOR

#endif
        }

        if (particlePool.Count >= emitAmount)
        {
            if (_currentCountForBurst > burstCount && isBurstMode)
            {
                TurnOnParticle(position, emitAmount, wait, usePsLifeTime,useSubEmitter);
                FindAndPlayAudio(_burstAudioSources);
                _currentCountForBurst = 0;
            }
            else
            {
                TurnOnParticle(position, emitAmount, wait, usePsLifeTime,useSubEmitter);
                FindAndPlayAudio(_audioSources);
                _currentCountForBurst++;
            }
        }
    }


    protected void TurnOnParticle(Vector3 position, int loopCount = 1, float delayToReturn = 3f,
        bool usePsLifeTime = false,bool useSubEmitter =false)
    {
        for (var i = 0; i < loopCount; i++)
        {
            var ps = particlePool.Dequeue();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();


            if (usePsLifeTime)
            {
                if (useSubEmitter)
                {
                    StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.startLifetime.constantMax,useSubEmitter:useSubEmitter));
                }
                else
                {
                    StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.startLifetime.constantMax,useSubEmitter:useSubEmitter));
                }
             
               
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"delayToReturn: {delayToReturn}");
#endif
                StartCoroutine(ReturnToPoolAfterDelay(ps, delayToReturn));
            }
        }
    }

    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float wait = 3f ,bool useSubEmitter =false)
    {
        if (wait_ == null)
        {
            wait_ = new WaitForSeconds(wait);
        }

        if (useSubEmitter && subWait_==null)
        {
#if UNITY_EDITOR
            Debug.Log($"WaitForSeconds 생성");
#endif
            subWait_ = new WaitForSeconds(subEmitLifetime);
            
        }
    

        yield return wait_;
        
        FindAndPlayAudio(_subAudioSources);

        if (subWait_!=null) yield return subWait_;
       
#if UNITY_EDITOR
        Debug.Log($"delayToReturn: {subWait_}{subEmitLifetime}");
#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Enqueue(ps); // Return the particle system to the pool
        
       
    }
    

    protected void FindAndPlayAudio(AudioSource[] audioSources, bool isBurst = false, bool recursive = false)
    {
        if (!isBurst)
        {
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

            if (availableAudioSource != null)
            {
                FadeInSound(availableAudioSource, targetVol);
            }
            
#if UNITY_EDITOR
#endif
           
        }
    }
    
    protected void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,
        float duration = 1f)
    {
        audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
#endif
            audioSource.Stop();
        });
    }

    protected void FadeInSound(AudioSource audioSource, float targetVolume = 1f, float duration = 0.3f)
    {
#if UNITY_EDITOR

#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
    }

}