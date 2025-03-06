using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using AudioClip = UnityEngine.AudioClip;
using Random = UnityEngine.Random;

/// <summary>
///     기존 EffectMaanager의 개선 버전입니다. 070324 현재 테스트 중입니다.  
/// </summary>
public class NewEffecetManager_070324 : MonoBehaviour
{
    
    [Header("Audio Setting")]
    [SerializeField]
    public bool playOrderly; // 랜덤재생이 기본값으로 합니다. 
    
    [Range(0f, 1f)] public float[] volumes;

    
    
    [Header("Particle Play Setting")] 
    private ParticleSystem[] _particles;
    private int _currentCountForBurst;

    public bool usePsMainTimeToReturn;
    public float returnWaitForSeconds;
    
    
    [Header("ObjectPool Setting")] 
    [SerializeField]
    private int _poolSize;
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
    
    [Header("SubEmitter Setting")] public bool useSubEmitter;
    public float subEmitLifetime;

    [Header("Particle Emission Setting")] [Header("Audio Setting")]
    public int audioSize;

    [FormerlySerializedAs("_burstAudioSize")] public int burstAudioSize;

    [Space(15f)]
    //하나씩 랜덤으로 재생하고싶은경우 false, 동시에 재생하고싶은 경우, true
    public bool isPlayAltogether;
    
    //재생하게끔 하기 위한  index 설정입니다. 
    private int _currentAudioSourceIndex;
    // AudioeffectClip은 Source 없음.. Randomize 기능 구현 전용입니다
    private AudioSource[][] _audioSources;
    
    public Ray ray_EffectManager { get; private set; }
    public RaycastHit[] hits;
    public static event Action OnClickInEffectManager;
    public Vector3 currentHitPoint { get; private set; }


    //for debug.
#if UNITY_EDITOR
private bool _isRaySet;
#endif
    
    protected virtual void OnGmRaySyncedByOnGm()
    {
        ray_EffectManager = Base_GameManager.GameManager_Ray;
        
        hits = Physics.RaycastAll(ray_EffectManager);
        foreach (var hit in hits)
        {
            currentHitPoint = hit.point;
            PlayParticle(particlePool, hit.point);
            OnClickInEffectManager?.Invoke();
            break;
        }

#if UNITY_EDITOR // IgameManager와 연동확인용
if (!_isRaySet)
{
    Debug.Log("Ray Synchronized by IGameManager; effectManager is Ready.");
    _isRaySet = true;
}
#endif
    }

  

    protected virtual void Init()
    {
        var currentSceneName = SceneManager.GetActiveScene().name;
        var audioList = new List<AudioClip>();
    
        for (char audioName = 'A'; ; audioName++)
        {
            var audioResource = Resources.Load<AudioClip>($"audio/게임별분류/{currentSceneName}/{currentSceneName}_" + audioName);
            if (audioResource == null)
            {
                break;
            }
            audioList.Add(audioResource);
        }

        _audioPath = new string[audioList.Count];
    
        // populate _audioPath with resource paths if needed
        for (int i = 0; i < audioList.Count; i++)
        {
            _audioPath[i] = $"audio/게임별분류/{currentSceneName}/{currentSceneName}_" + (char)('A' + i);
        }
        
        SetPool(ref particlePool);
        BindEvent();
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
        Base_GameManager.On_GmRay_Synced -= OnGmRaySyncedByOnGm;
    }

    /// <summary>
    ///     초기 풀 설정 -----------------
    /// </summary>
    protected virtual void SetPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        
        
        var currentSceneName = SceneManager.GetActiveScene().name;
        var particleList = new List<ParticleSystem>();
    
        for (char psName = 'A'; ; psName++)
        {
            var ps = Resources.Load<GameObject>($"audio/게임별분류/{currentSceneName}/{currentSceneName}_" + psName).GetComponent<ParticleSystem>();
            if (ps == null)
            {
                break;
            }
            particleList.Add(ps);
        }

        _particles = new ParticleSystem[ particleList.Count];
        _particles = particleList.ToArray();
        
        foreach (var ps in _particles)
            if (ps != null)
            {
                psQueue.Enqueue(ps);
                ps.gameObject.SetActive(false);
            }
        
        while (psQueue.Count < _poolSize)
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
        Base_GameManager.On_GmRay_Synced -= OnGmRaySyncedByOnGm;
        Base_GameManager.On_GmRay_Synced += OnGmRaySyncedByOnGm;
    }

    protected void PlayAudio(string audioPath = null,bool isBurst = false,
        float volume = 0.8f)
    {
        if (audioPath != null)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, audioPath, volume);
            return;
        }
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

        if (psQueue.Count >= emitAmount)
        {
           
            
                TurnOnParticle(psQueue, position);
                if (!playOrderly)
                {
                    var random = Random.Range(0, _audioPath.Length);
                    PlayAudio(_audioPath[Random.Range(0,_audioPath.Length)]);
                }
              
                
#if UNITY_EDITOR

#endif
       

            
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
            if (usePsMainTimeToReturn)
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