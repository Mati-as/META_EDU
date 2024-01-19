using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;



/// <summary>
/// *** GameManager 혹은 VideoGameManager에서 반드시 Ray를 참조하여 사용합니다.
/// </summary>
public  class  Base_EffectManager : MonoBehaviour
{
    [Header("Particle Play Setting")] public ParticleSystem[] _particles;
    private int _currentCountForBurst;

    public bool usePsMainTime;
    public float returnWaitForSeconds;

    [Header("ObjectPool Setting")] public int poolSize;

    private readonly string AUDIO_PATH_EFFECT_A;
    [HideInInspector] public InputAction _mouseClickAction;
    public Queue<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
    public WaitForSeconds subWait_;

    [Header("Particle Emission Setting")] private int _count;
    public int emitAmount;

    //여러 오디오클립을 랜덤하게 사용하고싶은 경우 체크하여 사용
    //_effectClip A,B,C..에 할당하면됨
    public bool isMultipleRandomClip;
    
    [Header("Burst Setting")] public bool useBurstMode;
    [FormerlySerializedAs("burstAmount")] public int burstEmitAmount;
    public int burstCount;

    [Header("SubEmitter Setting")] public bool useSubEmitter;
    public float subEmitLifetime;

    [Header("Particle Emission Setting")] [Header("Audio Setting")]
    public int audioSize;

    public int _burstAudioSize;

    [FormerlySerializedAs("playAltogether")] [Space(15f)]
    //하나씩 랜덤으로 재생하고싶은경우 false, 동시에 재생하고싶은 경우, true
    public bool isPlayAltogether;

    //각 이펙트를 한번에 플레이 하는 경우가 아닌 (isPlayTogther == false) 경우에 차례대로 index를 돌면서
    //재생하게끔 하기 위한  index 설정입니다. 
    private int currentAudioSourceIndex;

    //아래 오디오 클립을 모두 저장하는데 사용합니다, 여러 클립을 할당받고 랜덤하게 재생하기 위해 사용중입니다 1-17-24
    private List<AudioClip> _audioClips;
    private int CLIP_COUNTS = 10; //max로 10으로 설정
    public AudioClip _effectClipA;
    [Range(0f, 1f)] public float volumeA;
    public AudioClip _effectClipB;
    [Range(0f, 1f)] public float volumeB;

    public AudioClip _effectClipC;
    [Range(0f, 1f)] public float volumeC;
    public AudioClip _effectClipD;
    [Range(0f, 1f)] public float volumeD;
    public AudioClip _effectClipE;

    public AudioClip _subAudioClip;
    [Range(0f, 1f)] public float volumeSub;
    public AudioClip _burstClip;
    [Range(0f, 1f)] public float volumeBurst;


    // AudioeffectClip은 Source 없음.. Randomize 기능 구현 전용입니다
    private AudioSource[][] _audioSources;

    // 차례대로 Audiosource를 재생하기위한 AudioSource Count
    private int totalActiveAudioSouceSortCount;

    private AudioSource[] _audioSourcesA;
    private AudioSource[] _audioSourcesB;
    private AudioSource[] _audioSourcesC;
    private AudioSource[] _audioSourcesD;
    private AudioSource[] _subAudioSources;
    private AudioSource[] _burstAudioSources;

    private AudioSource[][] _audioSourceGroups;


    
    public Ray ray_EffectManager { get; private set; }
    public RaycastHit[] hits;
    public static event Action OnRaySyncFromGameManager;
    public static event Action OnRaySyncFromGmSingle;
    public Vector3 hitPoint { get; private set; }

#if UNITY_EDITOR
    private bool _isRaySet;
#endif
    protected virtual void OnGmRaySyncedByOnGm()
    {

        ray_EffectManager = IGameManager.GameManager_Ray;
#if UNITY_EDITOR
        if (!_isRaySet)
        {
            Debug.Log($"Ray Synchronized by IGameManager; effectManager is Ready.");
            _isRaySet = true;
        }
        
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            
            hitPoint = hit.point;
            
            PlayParticle(particlePool,hit.point);
            

            if (!Base_Interactable_VideoGameManager._isShaked)
            {
                OnRaySyncFromGameManager?.Invoke();
            }
            
            OnRaySyncFromGameManager?.Invoke();
            break;
            
        }
     
#endif
    }

    protected virtual void Init()
    {
       
        
        SetPool(ref particlePool);
        SetAudio();
        BindEvent();
        
        if(isMultipleRandomClip)SetRandomClip();
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
        IGameManager.On_GmRay_Synced -= OnGmRaySyncedByOnGm;
    }
    /// <summary>
    /// 초기 풀 설정 -----------------
    /// </summary>

    protected virtual void SetPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        _particles = new ParticleSystem[transform.childCount];
        var index = 0;
        foreach (Transform child in transform)
        {
            var ps = child.GetComponent<ParticleSystem>();
            if (ps != null) _particles[index++] = ps;
        }

        // Only enqueue each ParticleSystem instance once
        foreach (var ps in _particles)
            if (ps != null)
            {
                psQueue.Enqueue(ps);
                ps.gameObject.SetActive(false);
            }

        // Optionally, if you need more instances than available, clone them
        while (psQueue.Count < poolSize)
            foreach (var ps in _particles)
                if (ps != null)
                {
                    var newPs = Instantiate(ps, transform);
                    newPs.gameObject.SetActive(false);
                    psQueue.Enqueue(newPs);
                }
    }

    protected virtual void BindEvent()
    {
        IGameManager.On_GmRay_Synced -= OnGmRaySyncedByOnGm;
        IGameManager.On_GmRay_Synced += OnGmRaySyncedByOnGm;
    }

    /// <summary>
    /// 오디오 초기화 및 재생을 위한 메소드 목록 -----------------
    /// </summary>
    protected virtual void SetAudio()
    {
        if (AUDIO_PATH_EFFECT_A!= null && _effectClipA ==null)
        {
            _effectClipA = Resources.Load<AudioClip>(AUDIO_PATH_EFFECT_A);
        }
        
        _audioSourcesA = SetAudioSettings(_audioSourcesA, _effectClipA, audioSize, volumeA);
        if (_effectClipB != null) _audioSourcesB = SetAudioSettings(_audioSourcesB, _effectClipB, audioSize, volumeB);
        if (_effectClipC != null) _audioSourcesC = SetAudioSettings(_audioSourcesC, _effectClipC, audioSize, volumeC);
        if (_effectClipD != null) _audioSourcesD = SetAudioSettings(_audioSourcesD, _effectClipD, audioSize, volumeD);
        if (_subAudioClip != null && useSubEmitter )
            _subAudioSources = SetAudioSettings(_subAudioSources, _subAudioClip, audioSize, volumeSub);
        if (_burstClip != null)
            _burstAudioSources = SetAudioSettings(_burstAudioSources, _burstClip, _burstAudioSize, volumeBurst);
    }


    private AudioSource[] SetAudioSettings(AudioSource[] audioSources, AudioClip audioClip, int size, float volume = 1f,
        float interval = 0.25f)
    {
        //오디오 갯수아닌 오디오 종류의 갯수.
        totalActiveAudioSouceSortCount++;

        audioSources = new AudioSource[size];
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

        return audioSources;
    }
    
    
    protected void FindAndPlayAudio(AudioSource[] audioSources, bool isBurst = false, bool recursive = false,
        float volume = 0.8f)
    {
        if (!isBurst)
        {
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

            if (availableAudioSource != null)
            {
                if(isMultipleRandomClip) availableAudioSource.clip = RandomizeClip();
                
#if UNITY_EDITOR
                Debug.Log($"availableAudioSource.Clip:   {availableAudioSource.clip}," +
                          $" _currentRandomClipIndex :{_currentRandomClipIndex}");
#endif
                FadeInSound(availableAudioSource, volume);
            }

#if UNITY_EDITOR
#endif
        }
    }

    private int _totalCilpCountWhenUseMultipleClips;
    protected void SetRandomClip()
    {
        _audioClips = new List<AudioClip>();

        if (_effectClipA != null)
        {
            _audioClips.Add(_effectClipA);
            _totalCilpCountWhenUseMultipleClips++;
        }
        if (_effectClipB != null)
        {
            _audioClips.Add(_effectClipB);
            _totalCilpCountWhenUseMultipleClips++;
        }
        if (_effectClipC != null)
        {
            _audioClips.Add(_effectClipC);
            _totalCilpCountWhenUseMultipleClips++;
        }
        if (_effectClipD != null)
        {
            _audioClips.Add(_effectClipD);
            _totalCilpCountWhenUseMultipleClips++;
        }
        if (_effectClipD != null)
        {
            _audioClips.Add(_effectClipE);
            _totalCilpCountWhenUseMultipleClips++;
        }
 

    }
    protected AudioClip RandomizeClip()
    {
        _currentRandomClipIndex = Random.Range(0, _totalCilpCountWhenUseMultipleClips);
        AudioClip randomClip = _audioClips[_currentRandomClipIndex];
        return randomClip;
    }

    private int _currentRandomClipIndex;

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

    protected void GrowPool(ParticleSystem original)
    {
        var newInstance = Instantiate(original, transform);
        newInstance.gameObject.SetActive(false);
        particlePool.Enqueue(newInstance);
    }


    protected virtual void PlayParticle(Queue<ParticleSystem> psQueue,Vector3 position,  bool isBurstMode = false,
        int burstCount = 10, int burstAmount = 5)
    {
        //UnderFlow를 방지하기 위해서 선제적으로 GrowPool 실행 
        if (psQueue.Count < emitAmount || psQueue.Count < burstCount)
        {
            // 에디터상에서 배치한 순서대로 파티클을 Push하기 위해 for문 사용합니다. 
            for (var i = 0; i < burstAmount; i++)
                foreach (var ps in _particles)
                    GrowPool(ps);
#if UNITY_EDITOR

#endif
        }

        if (psQueue.Count >= emitAmount)
        {
            if (_currentCountForBurst > burstCount && isBurstMode)
            {
                TurnOnParticle(psQueue,position);
                FindAndPlayAudio(_burstAudioSources, volume: volumeBurst);
                _currentCountForBurst = 0;
            }
            else
            {
                TurnOnParticle(psQueue,position);
                if (isPlayAltogether)
                {
                    FindAndPlayAudio(_audioSourcesA, volume: volumeA);
                    if (_effectClipB != null) FindAndPlayAudio(_audioSourcesB, volume: volumeB);
                    if (_effectClipC != null) FindAndPlayAudio(_audioSourcesC, volume: volumeB);
                    if (_effectClipD != null) FindAndPlayAudio(_audioSourcesC, volume: volumeB);
                }

                if (!isPlayAltogether)
                {
                    FindAndPlayAudio(_audioSourcesA, volume: volumeA);

                    AudioSource[] currentAudioSourceArray = null;
                    var currentVolume = 0f;
#if UNITY_EDITOR
              
#endif

                    switch (currentAudioSourceIndex)
                    {
                        case 0:
                            if (_audioSourcesB != null) FindAndPlayAudio(_audioSourcesB, volume: volumeB);

                            break;
                        case 1:
                            if (_audioSourcesC != null) FindAndPlayAudio(_audioSourcesC, volume: volumeC);
                            break;
                        case 2:
                            if (_audioSourcesD != null) FindAndPlayAudio(_audioSourcesD, volume: volumeD);
                            break;
                    }

                    currentAudioSourceIndex = ++currentAudioSourceIndex % (totalActiveAudioSouceSortCount);
                }

                _currentCountForBurst++;
            }
        }
    }

    /// <summary>
    /// 파티클 초기화 및 재생을 위한 메소드 목록 -----------------
    /// </summary>
    /// <param name="position"></param>

    protected void TurnOnParticle(Queue<ParticleSystem> psQueue, Vector3 position)
    {
        for (var i = 0; i < emitAmount; i++)
        {
            var ps = psQueue.Dequeue();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            
            ps.Play();

            StartCoroutine(ReturnToPoolAfterDelay(ps));
        }
    }

    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps)
    {
        if (wait_ == null)
        {
            if (usePsMainTime)
                wait_ = new WaitForSeconds(ps.main.startLifetime.constantMax);

            else
                wait_ = new WaitForSeconds(returnWaitForSeconds);
        }

        if (useSubEmitter && subWait_ == null)
        {
            subWait_ = new WaitForSeconds(subEmitLifetime);
        }


        yield return wait_;

        if (useSubEmitter) FindAndPlayAudio(_subAudioSources, volume: volumeSub);

        if (subWait_ != null) yield return subWait_;

#if UNITY_EDITOR

#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Enqueue(ps); // Return the particle system to the pool
    }


}