using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;

public abstract class Base_EffectController : MonoBehaviour
{
   
    public ParticleSystem[] _particles;
    private int _currentCountForBurst;
    private readonly float _returnWaitForSeconds = 3f;
    public ParticleSystem[] particleSystem;
    
    [HideInInspector]
    public Camera _camera;
    public InputAction _mouseClickAction;
    public Queue<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
        
    [Header("Particle Emission Setting")]
    private int _count;
    public int emitAmount;
    public int burstAmount;
    public int burstCount;
    public float targetVol;
   
    [Header("Audio Setting")]
    public int audioSize;
    public int _burstAudioSize;
    
    public AudioClip _effectClip;
    public AudioClip _burstClip;
    
    private AudioSource[] _audioSources;
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
        Video_Image_Move.OnStep -= OnClicked;
    }

    protected virtual void SetPool()
    {
        particlePool = new Queue<ParticleSystem>();
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

    protected virtual void BindEvent()
    {
        Video_Image_Move.OnStep -= OnClicked;
        Video_Image_Move.OnStep += OnClicked;
    }

    private void SetAudio()
    {
        _audioSources = new AudioSource[audioSize];

        for (var i = 0; i < audioSize; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].clip = _effectClip;
            _audioSources[i].playOnAwake = false;
            _audioSources[i].pitch = UnityEngine.Random.Range(0.75f, 1.4f);
        }

        _burstAudioSources = new AudioSource[_burstAudioSize];

        for (var i = 0; i < _burstAudioSize; i++)
        {
            _burstAudioSources[i] = gameObject.AddComponent<AudioSource>();
            _burstAudioSources[i].clip = _burstClip;
            _burstAudioSources[i].playOnAwake = false;
            _burstAudioSources[i].pitch = UnityEngine.Random.Range(0.95f, 1.3f);
        }
    }

    protected void GrowPool(ParticleSystem original, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var newInstance = Instantiate(original,transform);
            newInstance.gameObject.SetActive(false);
            particlePool.Enqueue(newInstance);
        }
    }

    protected void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,float duration =1f)
    {
        audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
       
#endif
            audioSource.Stop();
        });
    }

    protected void FadeInSound( AudioSource audioSource,float targetVolume = 1f,float duration = 0.3f)
    {
#if UNITY_EDITOR
     
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
    }
    
    

    protected virtual void PlayParticle(Vector3 position, bool isBurstMode = false,
        int emitAmount = 2, int burstCount = 10, int burstAmount = 5, float wait = 3f,bool usePsMainDuration = false)
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
                // if (particlePool.Count <= burstAmount)
                //     foreach (var ps in _particles)
                //         GrowPool(ps);

              
                TurnOnParticle(position, loopCount:emitAmount, wait,usePsMainDuration);
                FindAndPlayAudio(_burstAudioSources);
                _currentCountForBurst = 0;
            }
            else
            {
              
                TurnOnParticle(position, loopCount:emitAmount, wait,usePsMainDuration);
                FindAndPlayAudio(_audioSources);
                _currentCountForBurst++;
            }
        }
    }


    protected void TurnOnParticle(Vector3 position, int loopCount = 1, float delayToReturn = 3f, bool isWaitTimeManuallySet = false)
    {
        for (var i = 0; i < loopCount; i++)
        {
            ParticleSystem ps = particlePool.Dequeue();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();

           
            if (isWaitTimeManuallySet)
            {
                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.startLifetime.constantMax));
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
    
    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float wait = 3f,bool isWaitTimeManuallySet = false)
    {
        if (wait_ == null)
        {
            if (isWaitTimeManuallySet)
            {
#if UNITY_EDITOR
                Debug.Log($"PS.LIFETIME.CONSTANTMAX : {ps.main.startLifetime.constantMax}");
#endif
                wait_ = new WaitForSeconds(ps.main.startLifetime.constantMax);
            }
            else
            {
                wait_ = new WaitForSeconds(wait);
            }
            
        }
        
        
        yield return new WaitForSeconds(ps.main.startLifetime.constantMax);
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Enqueue(ps); // Return the particle system to the pool
    }
    
    protected void FindAndPlayAudio(AudioSource[] audioSources,bool isBurst = false, bool recursive = false)
    {
        if (!isBurst)
        {
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

            if (availableAudioSource != null)
            {
                FadeInSound(availableAudioSource,targetVolume:targetVol);
            }
            else
            {
#if UNITY_EDITOR
                
#endif
            }
        }
        

    }




}
