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
    
    [HideInInspector] private Camera _camera;
    public InputAction _mouseClickAction;
    public Queue<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
    public WaitForSeconds subWait_;

    [Header("Particle Emission Setting")] private int _count;
    public int emitAmount;
   

    [Header("Burst Setting")] public int burstAmount;
    public int burstCount;

    [Header("SubEmitter Setting")] public float subEmitLifetime;

    [Header("Particle Emission Setting")]
    [Header("Audio Setting")]
    public int audioSize;

    public int _burstAudioSize;

    public AudioClip _effectClipA;
    [Range(0f, 1f)] public float volumeA;
    public AudioClip _effectClipB;
    [Range(0f, 1f)] public float volumeB;
    public AudioClip _subAudioClip;
    [Range(0f, 1f)] public float volumeSub;
    public AudioClip _burstClip;
    [Range(0f, 1f)] public float volumeBurst;

    private AudioSource[] _audioSourcesA;
    private AudioSource[] _audioSourcesB;
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

    protected virtual void SetAudio()
    {
        _audioSourcesA = new AudioSource[audioSize];
        SetAudioSettings(_audioSourcesA, _effectClipA, volumeA);

        _audioSourcesB = new AudioSource[audioSize];
        if (_effectClipB != null) SetAudioSettings(_audioSourcesB, _effectClipB, volumeB);

        _subAudioSources = new AudioSource[audioSize];
        SetAudioSettings(_subAudioSources, _subAudioClip, volumeSub);

        _burstAudioSources = new AudioSource[_burstAudioSize];
        SetAudioSettings(_burstAudioSources, _burstClip, volumeBurst);
    }

    private void SetAudioSettings(AudioSource[] audioSources, AudioClip audioClip, float volume = 1f,float interval = 0.15f)
    {
        for (var i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = audioClip;
            audioSources[i].volume = volume;
            audioSources[i].spatialBlend = 0f;
            audioSources[i].outputAudioMixerGroup = null;
            audioSources[i].playOnAwake = false;
            audioSources[i].pitch = Random.Range(1 - interval, 1 + interval);
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
       int burstCount = 10, int burstAmount = 5, float wait = 3f, bool usePsLifeTime = false
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
                TurnOnParticle(position, wait, usePsLifeTime, useSubEmitter);
                FindAndPlayAudio(_burstAudioSources,volume:volumeBurst);
                _currentCountForBurst = 0;
            }
            else
            {
                TurnOnParticle(position, wait, usePsLifeTime, useSubEmitter);
                FindAndPlayAudio(_audioSourcesA,volume:volumeA);
                if (_effectClipB != null) FindAndPlayAudio(_audioSourcesB,volume:volumeB);
                _currentCountForBurst++;
            }
        }
    }


    protected void TurnOnParticle(Vector3 position, float delayToReturn = 3f,
        bool usePsLifeTime = false, bool useSubEmitter = false)
    {
        for (var i = 0; i < emitAmount; i++)
        {
            var ps = particlePool.Dequeue();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();


            if (usePsLifeTime)
            {
                if (useSubEmitter)
                    StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.startLifetime.constantMax, useSubEmitter));
                else
                    StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.startLifetime.constantMax, useSubEmitter));
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

    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float wait = 3f, bool useSubEmitter = false)
    {
        if (wait_ == null) wait_ = new WaitForSeconds(wait);

        if (useSubEmitter && subWait_ == null)
        {
#if UNITY_EDITOR
            Debug.Log("WaitForSeconds 생성");
#endif
            subWait_ = new WaitForSeconds(subEmitLifetime);
        }


        yield return wait_;

        if(useSubEmitter) FindAndPlayAudio(_subAudioSources, volume:volumeSub);

        if (subWait_ != null) yield return subWait_;

#if UNITY_EDITOR

#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Enqueue(ps); // Return the particle system to the pool
    }


    protected void FindAndPlayAudio(AudioSource[] audioSources, bool isBurst = false, bool recursive = false,
    float volume = 0.8f)
    {
        if (!isBurst)
        {
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

            if (availableAudioSource != null) FadeInSound(availableAudioSource, volume);

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