using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
///     *** GameManager 혹은 VideoGameManager에서 반드시 Ray를 참조하여 사용합니다.
/// </summary>
public class EffectManager : IGameManager
{

    private enum Sound
    {
        Basic,
        Burst
    }

    private string SCENE_NAME;
    [Header("Particle Play Setting")] public ParticleSystem[] _particles;
    private int _currentCountForBurst;

    public bool usePsMainTime;
    public float returnWaitForSeconds;

    [Header("ObjectPool Setting")] 
    public int poolSize;
    private readonly string AUDIO_PATH_EFFECT_A;
    [HideInInspector] public InputAction _mouseClickAction;
    public Queue<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
    public WaitForSeconds subWait_;

    [Header("Audio Setting")] 
    private string [] _audioPath;
    
    [Header("Particle Emission Setting")]
    private int _count;
    public int emitAmount;

    //여러 오디오클립을 랜덤하게 사용하고싶은 경우 체크하여 사용
    //_effectClip A,B,C..에 할당하면됨
    public bool isMultipleRandomClip;

    [Header("Burst Setting")] 
    public bool useBurstMode;
     public int burstEmitAmount;
    public int burstCount;

    [Header("SubEmitter Setting")] public bool useSubEmitter;
    public float subEmitLifetime;

    [Header("Particle Emission Setting")] [Header("Audio Setting")]
    public int audioSize;

    public int _burstAudioSize;

    [Space(15f)]
    //하나씩 랜덤으로 재생하고싶은경우 false, 동시에 재생하고싶은 경우, true
    public bool isPlayAltogether;

    //각 이펙트를 한번에 플레이 하는 경우가 아닌 (isPlayTogther == false) 경우에 차례대로 index를 돌면서
    //재생하게끔 하기 위한  index 설정입니다. 
    private int currentAudioSourceIndex;

    //아래 오디오 클립을 모두 저장하는데 사용합니다, 여러 클립을 할당받고 랜덤하게 재생하기 위해 사용중입니다 1-17-24
    
    
   


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
    public static event Action OnClickInEffectManager;
    public Vector3 currentHitPoint { get; private set; }

    protected  readonly float CLICKABLE_DELAY = 0.08f;
    protected WaitForSeconds _waitForClickable;
    public bool isClickable { get; private set; } = true;

    /// <summary>
    /// 08/13/2024
    /// 1.Click빈도수를 유니티상에서 제어할때 사용합니다.
    /// 2.물체가 많은경우 정확도 이슈로 센서자체를 필터링 하는것은 권장되지 않다고 판단하고 있습니다.
    /// </summary>
    protected void SetClickableWithDelay() => StartCoroutine(SetClickableWithDelayCo());

    IEnumerator SetClickableWithDelayCo()
    {
        if(_waitForClickable ==null) _waitForClickable = new WaitForSeconds(CLICKABLE_DELAY);
        
        isClickable = false;
        yield return _waitForClickable;
        isClickable = true;
    }


    //for debug.
#if UNITY_EDITOR
private bool _isRaySet;
#endif

    private Hopscotch_GameManager _gm;
    private bool _isStartBtnClicked;
    protected virtual void OnGmRaySyncedByOnGm()
    {
        if (!_isStartBtnClicked) return;
        if (!isClickable) return;
     
        
        SetClickableWithDelay();
            
        
        
        if (_gm!=null && !_gm.isStartButtonClicked) return;
        ray_EffectManager = IGameManager.GameManager_Ray;

        
        
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            currentHitPoint = hit.point;

            PlayParticle(particlePool, hit.point);
            
            if (!_isClickable)
            {
#if UNITY_EDITOR
                //      Debug.Log("it's not clickable temporary ----------------------");
#endif
                return;
            }
            SetClickable();
            OnClickInEffectManager?.Invoke();
            break;
        }

#if UNITY_EDITOR
if (!_isRaySet)
{
    Debug.Log("Ray Synchronized by IGameManager; effectManager is Ready.");
    _isRaySet = true;
}

#endif
    }
    
    private float _clickInterval = 1f;
    private WaitForSeconds _clickWait;
    private bool _isClickable =true;
    private void SetClickable()
    {
        StartCoroutine(SetClickableCo());
    }

    private IEnumerator SetClickableCo()
    {
        _isClickable = false;
        
        if (_clickWait == null)
        {
            _clickWait = new WaitForSeconds(_clickInterval);
        }

        yield return _clickWait;

        _isClickable = true;

    }

    protected virtual void Init()
    {

        SCENE_NAME = SceneManager.GetActiveScene().name;
        
        if (SceneManager.GetActiveScene().name == "BB002")
        {
            _gm = GameObject.Find("Hopscotch_GameManager").GetComponent<Hopscotch_GameManager>();
        }
        
        
        SetPool(ref particlePool);
        BindEvent();
        if (SceneManager.GetActiveScene().name == "BB002")
        {
            _gm = GameObject.Find("Hopscotch_GameManager").GetComponent<Hopscotch_GameManager>();
        }
       
        
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
        UI_Scene_Button.onBtnShut -= OnStartBtnClicked;
        IGameManager.On_GmRay_Synced -= OnGmRaySyncedByOnGm;
    }

    /// <summary>
    ///     초기 풀 설정 -----------------
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

    private void OnStartBtnClicked()
    {
        _isStartBtnClicked = true;
    }

    protected virtual void BindEvent()
    {
        UI_Scene_Button.onBtnShut -= OnStartBtnClicked;
        UI_Scene_Button.onBtnShut += OnStartBtnClicked;
        IGameManager.On_GmRay_Synced -= OnGmRaySyncedByOnGm;
        IGameManager.On_GmRay_Synced += OnGmRaySyncedByOnGm;
    }



    
    protected void GrowPool(ParticleSystem original)
    {
        var newInstance = Instantiate(original, transform);
        newInstance.gameObject.SetActive(false);
        particlePool.Enqueue(newInstance);
    }
    
    protected virtual void PlayParticle(Queue<ParticleSystem> psQueue, Vector3 position, bool isBurstMode = false,
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

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/VideoClickEffectSound/" + SCENE_NAME,0.1f
            ,pitch:Random.Range(1f,1.05f));
        
        if (psQueue.Count >= emitAmount)
        {
            if (_currentCountForBurst > burstCount && isBurstMode)
            {
                TurnOnParticle(psQueue, position);
            }
            else
            {
                TurnOnParticle(psQueue, position);
    

            }
        }
    }

    /// <summary>
    ///     파티클 초기화 및 재생을 위한 메소드 목록 -----------------
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

        if (useSubEmitter && subWait_ == null) subWait_ = new WaitForSeconds(subEmitLifetime);


        yield return wait_;

        if (subWait_ != null) yield return subWait_;

#if UNITY_EDITOR

#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Enqueue(ps); // Return the particle system to the pool
    }
}