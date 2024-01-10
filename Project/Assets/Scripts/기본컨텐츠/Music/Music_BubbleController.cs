using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Music_BubbleController : MonoBehaviour
{
   

    
    [Header("Reference")] 
    [SerializeField] private Music_GameManager _gameManager;

    public float randomTime;
    private float _currentTime;

    private Stack<ParticleSystem> _clickEffectPoolSmall;
    private Stack<ParticleSystem> _clickEffectPoolBig;

    private WaitForSeconds _poolReturnWait;

    
    [Space(10f)] 
    [Header("particle control")] 
    public float clickRadius;
    public float setLifeTime;
    private ParticleSystem[] particleSystems;
    public static event Action bigBubbleEvent;
    private ParticleSystem _effect_Small;
    private ParticleSystem _effect_Big;
    private float closestDistance;
    private int closestIndex;


    private AudioClip _audioClip;
    private AudioSource[] _smallBubbleAudioSources;
    private AudioSource[] _bigBubbleAudioSources;
    private AudioSource[] _clearBubbleSoundAudioSource;
    private readonly int _audioClipCount = 5;
    
    
    private readonly string AUDIO_SMALL_BUBBLE_PATH = "게임별분류/기본컨텐츠/SkyMusic/Audio/bubble_explode_small";
    private readonly string AUDIO_BIG_BUBBLE_PATH =   "게임별분류/기본컨텐츠/SkyMusic/Audio/bubble_explode_big";
    private readonly string AUDIO_CLEAR_BUBBLE =   "게임별분류/기본컨텐츠/SkyMusic/Audio/clear_bubble";

    private readonly string PREFAB_EFFECT_PARTICLE_PATH_SMALL = "게임별분류/기본컨텐츠/SkyMusic/Prefab/bubble_explode_small";
    private readonly string PREFAB_EFFECT_PARTICLE_PATH_BIG =   "게임별분류/기본컨텐츠/SkyMusic/Prefab/bubble_explode_big";

    private RaycastHit rayCastHitForBubble;
    private Ray ray;
    public Vector3 hitOffset;

    private void Awake()
    {
        BindEvent();
       
    }


    private void Start()
    {
        Init();
        

        var childParticleSystems = new List<ParticleSystem>();


        foreach (Transform child in transform)
        {
            var ps = child.GetComponent<ParticleSystem>();
            if (ps != null) childParticleSystems.Add(ps);
        }

        randomTime = Random.Range(40, 50);
        particleSystems = childParticleSystems.ToArray();
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime > randomTime)
        {
            var layerMask = LayerMask.GetMask("Screen");

            foreach (var ps in particleSystems)
                if (Physics.Raycast(ray, out rayCastHitForBubble, Mathf.Infinity, layerMask))
                {
#if UNITY_EDITOR
                    Debug.Log("clear off all the bubbles");
#endif
                    ClickEventApplyRadialForce(rayCastHitForBubble.point, ps, 100);
                    
                }

            //오디오 일괄재생
            FindAndPlayAudio(_smallBubbleAudioSources);
            FindAndPlayAudio(_bigBubbleAudioSources);
            FindAndPlayAudio(_clearBubbleSoundAudioSource);
            
            
            _currentTime = 0;
            randomTime = Random.Range(20, 25);
        }
    }


    private void Init()
    {
        _clickEffectPoolSmall = new Stack<ParticleSystem>();
        _clickEffectPoolBig = new Stack<ParticleSystem>();

        _effect_Small = Resources.Load<GameObject>(PREFAB_EFFECT_PARTICLE_PATH_SMALL).GetComponent<ParticleSystem>();
        _effect_Big = Resources.Load<GameObject>(PREFAB_EFFECT_PARTICLE_PATH_BIG).GetComponent<ParticleSystem>();

        SetPool(_clickEffectPoolSmall, PREFAB_EFFECT_PARTICLE_PATH_SMALL);
        SetPool(_clickEffectPoolBig, PREFAB_EFFECT_PARTICLE_PATH_BIG, 3);
        
        
        _smallBubbleAudioSources = InitializeAudioSource(_smallBubbleAudioSources, AUDIO_SMALL_BUBBLE_PATH);
        _bigBubbleAudioSources = InitializeAudioSource(_bigBubbleAudioSources, AUDIO_BIG_BUBBLE_PATH);
        _clearBubbleSoundAudioSource = InitializeAudioSource(_clearBubbleSoundAudioSource, AUDIO_CLEAR_BUBBLE,1);
        
    }

    private void OnClicked()
    {
        ray = _gameManager.ray_GameManager;

        var layerMask = LayerMask.GetMask("Screen");


        if (Physics.Raycast(ray, out rayCastHitForBubble, Mathf.Infinity, layerMask))
            RemoveClosestParticle(rayCastHitForBubble.point + hitOffset);
    }

    protected virtual void OnDestroy()
    {
        Music_GameManager.eventAfterAGetRay -= OnClicked;
    }

    protected virtual void BindEvent()
    {
        Music_GameManager.eventAfterAGetRay -= OnClicked;
        Music_GameManager.eventAfterAGetRay += OnClicked;
    }

#if UNITY_EDITOR //같이 빌드하지 말 것.

    private void OnDrawGizmos()
    {
        var layerMask = LayerMask.GetMask("Screen");
        if (Physics.Raycast(ray, out rayCastHitForBubble, Mathf.Infinity, layerMask))
        {
            // 레이를 그립니다.
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ray.origin, rayCastHitForBubble.point+hitOffset);

            // 히트 지점에 작은 구체를 그립니다.
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(rayCastHitForBubble.point +hitOffset, 3f);
        }
    }

#endif

    // objectPool-related methods-------------------------------------------------

    private ParticleSystem GetFromPool(Stack<ParticleSystem> effectPool, string path)
    {
        if (effectPool.Count < 0) GrowPool(effectPool, path);

        var currentEffect = effectPool.Pop();
        return currentEffect;
    }

    private void GrowPool(Stack<ParticleSystem> pool, string path)
    {
        var ps = new ParticleSystem();

        if (path == PREFAB_EFFECT_PARTICLE_PATH_SMALL)
            ps = Instantiate(_effect_Small, transform);
        else
            ps = Instantiate(_effect_Big, transform);

        pool.Push(ps);
    }

    private void PlayParticle(Stack<ParticleSystem> effectPool, Vector3 position, string path)
    {
        var ps = GetFromPool(effectPool, path);
        if (ps != null)
        {
            ps.Stop();
            ps.gameObject.SetActive(true);

            ps.transform.position = position;

            ps.Play();

            StartCoroutine(ReturnToPoolAfterDelay(ps, effectPool));
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("particle is null.");
#endif
        }
    }

    private void SetPool(Stack<ParticleSystem> effectPool, string path, int poolCount = 5)
    {
        var prefab = Resources.Load<GameObject>(path);

        if (prefab != null)
        {
            for (var poolSize = 0; poolSize < poolCount; poolSize++)
            {
                var effect = Instantiate(prefab, transform);
                effect.SetActive(false);
                var ps = new ParticleSystem();

                effect.TryGetComponent(out ps);
                if (ps != null) effectPool.Push(ps);
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("this gameObj to pool is null.");
#endif
        }
    }


    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, Stack<ParticleSystem> particlePool)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;

#if UNITY_EDITOR

#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }

    // particle-related methods-------------------------------------------------

    private void RemoveClosestParticle(Vector3 position)
    {
        var closestDistance = float.MaxValue;
        ParticleSystem closestParticleSystem = null;
        var closestParticleIndex = -1;

        foreach (var ps in particleSystems)
        {
            var (index, distance) = FindClosestParticle(position, ps);


            if (distance < closestDistance)
            {
                //1-9-24 Unit 불일치 의심으로 /100 수행
                closestDistance = distance;
                closestParticleSystem = ps;
                closestParticleIndex = index;
            }
        }

        // 가장 가까운 파티클을 제거합니다.
        if (closestParticleSystem != null && closestParticleIndex != -1 && closestDistance < clickRadius)
        {
            var particles = new ParticleSystem.Particle[closestParticleSystem.particleCount];
            closestParticleSystem.GetParticles(particles);

            particles[closestParticleIndex].remainingLifetime = 0;

            PlayParticle(_clickEffectPoolSmall, particles[closestParticleIndex].position,
                PREFAB_EFFECT_PARTICLE_PATH_SMALL);
            FindAndPlayAudio(_smallBubbleAudioSources);

            if (particles[closestParticleIndex].startSize > 3)
            {
                PlayParticle(_clickEffectPoolBig, particles[closestParticleIndex].position,
                    PREFAB_EFFECT_PARTICLE_PATH_BIG);
                FindAndPlayAudio(_bigBubbleAudioSources);


                bigBubbleEvent?.Invoke();
            }

            // 최종적으로, 파티클 배열의 길이에서 제거된 파티클을 반영합니다.
            closestParticleSystem.SetParticles(particles, particles.Length);

#if UNITY_EDITOR
            Debug.Log($"가장 가까운 파티클 제거 인덱스: {closestParticleIndex}, 최단거리 : {closestDistance}");
#endif
        }
    }

    private (int, float) FindClosestParticle(Vector3 position, ParticleSystem particleSystem)
    {
        var particles = new ParticleSystem.Particle[particleSystem.particleCount];
        var particleCount = particleSystem.GetParticles(particles);

        closestDistance = float.MaxValue;
        closestIndex = -1;

        for (var i = 0; i < particleCount; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return (closestIndex, closestDistance);
    }

    private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem, float clickRadious)
    {
        var particles = new ParticleSystem.Particle[particleSystem.particleCount];
        var particleCount = particleSystem.GetParticles(particles);

        var closestDistance = float.MaxValue;
        var closestIndex = -1;

        // 가장 가까운 파티클을 찾습니다.
        for (var i = 0; i < particleCount; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        var numParticlesAlive = particleSystem.GetParticles(particles);

        //   파티클을 제거합니다.
        if (closestIndex != -1)
        {
            for (var i = 0; i < numParticlesAlive; i++)
            {
                var distance = Vector3.Distance(position, particles[i].position);

                if (distance < clickRadious) particles[i].remainingLifetime = 0.8f;
            }

            particleSystem.SetParticles(particles, particleCount);
        }
    }


// audio-related methods------------------------------------------------------

    private AudioClip LoadSound(string path)
    {
        var _audioClip = Resources.Load<AudioClip>(path);
        return _audioClip;
    }

    private AudioSource[] InitializeAudioSource(AudioSource[] audioSources, string path, float volume = 0.5f,int size =5)
    {
        var _audioClip = LoadSound(path);

        audioSources = new AudioSource[size];
        for (var i = 0; i < audioSources.Length; ++i)
        {
            audioSources[i] =  gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = _audioClip;
            audioSources[i].volume = volume;
            audioSources[i].spatialBlend = 0f;
            audioSources[i].outputAudioMixerGroup = null;
            audioSources[i].playOnAwake = false;
            audioSources[i].pitch = Random.Range(0.9f, 1.1f);
        }

        return audioSources;
    }

    protected void FindAndPlayAudio(AudioSource[] audioSources,
        float volume = 0.8f)
    {
        var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

        if (availableAudioSource != null) FadeInSound(availableAudioSource, volume);

#if UNITY_EDITOR
        Debug.Log("재생");
#endif
    }

    protected void FadeInSound(AudioSource audioSource, float target = 0.3f, float fadeInDuration = 1.0f
        , float delay = 1f)
    {
        audioSource.Play();
        audioSource.DOFade(target, fadeInDuration).OnComplete(() => { }).OnComplete(() => { audioSource.Stop(); });
    }
}

