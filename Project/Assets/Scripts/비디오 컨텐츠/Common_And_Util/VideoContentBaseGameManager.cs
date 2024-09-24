using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Random = UnityEngine.Random;

/// <summary>
///     *** GameManager 혹은 VideoGameManager에서 반드시 Ray를 참조하여 사용합니다.
/// </summary>
public class VideoContentBaseGameManager : Base_GameManager
{
    private string SCENE_NAME;
    private ParticleSystem[] _particles;


    public bool usePsMainTime;

    private Queue<ParticleSystem> _particlePool;
    private int _poolSize =50;
    private WaitForSeconds _waitForPs;
    public float returnWaitForSeconds;
    
    public int emitAmount;


    public static event Action OnClickInEffectManager; 
    protected VideoPlayer videoPlayer;
    protected bool _initiailized;

    private readonly string prefix = "Video_";


    public Vector3 currentHitPoint { get; private set; }
  
    
    protected Vector3 _defaultPosition { get; private set; }

    [SerializeField]
    private float playbackSpeed = 1;
    

    private HopscotchBaseGameManager _gm;

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        SetClickableWithDelay();

        foreach (var hit in GameManager_Hits)
        {
            currentHitPoint = hit.point;
            PlayParticle(_particlePool, hit.point);
            break;
        }
    }
    

    protected override void Init()
    {
        base.Init();

        DEFAULT_SENSITIVITY = 0.1f;
        SetVideo();
        //DoShake시 트위닝 오류로 비디오 객체의 재생위치가
        //원래 위치에서 벗어나는 것을 방지하기 위한 defaultPosition 설정.
        //DoShake의 OnComplete에서 원래위치로 돌아가는 로직이 동작 하도록 구성하였습니다. 02/02/24
        _defaultPosition = new Vector3();
        _defaultPosition = transform.position;
    }

    private void SetVideo()
    {
        //비디오 재생관련 세팅.
        videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.playbackSpeed = playbackSpeed;

        var mp4Path =
            Path.Combine(Application.streamingAssetsPath,
                $"{SceneManager.GetActiveScene().name}.mp4");

        if (File.Exists(mp4Path))
        {
            videoPlayer.url = mp4Path;
        }
        else
        {
            // MP4 파일이 없으면 MOV 파일 재생
            var movPath =
                Path.Combine(Application.streamingAssetsPath,
                    $"{SceneManager.GetActiveScene().name}.mov");
            videoPlayer.url = movPath;
        }

        videoPlayer.Play();

        _initiailized = true;
        SCENE_NAME = SceneManager.GetActiveScene().name;

        SetPool(ref _particlePool);
        BindEvent();
    }
    

    #region Particle System Setting ------------------------------------

    /// <summary>
    ///     초기 풀 설정 -----------------
    /// </summary>
    protected virtual void SetPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        _particles = new ParticleSystem[transform.childCount];
        Debug.Assert(_particles != null );
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
        while (psQueue.Count < _poolSize)
            foreach (var ps in _particles)
                if (ps != null)
                {
                    var newPs = Instantiate(ps, transform);
                    newPs.gameObject.SetActive(false);
                    psQueue.Enqueue(newPs);
                }
    }


    protected void GrowPool(ParticleSystem original)
    {
        var newInstance = Instantiate(original, transform);
        newInstance.gameObject.SetActive(false);
        _particlePool.Enqueue(newInstance);
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

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/VideoClickEffectSound/" + SCENE_NAME, 0.1f
            , Random.Range(1f, 1.05f));

        if (psQueue.Count >= emitAmount) TurnOnParticle(psQueue, position);
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
        if (_waitForPs == null)
        {
            if (usePsMainTime)
                _waitForPs = new WaitForSeconds(ps.main.startLifetime.constantMax);

            else
                _waitForPs = new WaitForSeconds(returnWaitForSeconds);
        }


        yield return _waitForPs;

#if UNITY_EDITOR
Debug.Log("sub sound play -");
#endif
        Managers.soundManager.Play(SoundManager.Sound.Effect, $"Audio/VideoClickEffectSound/{SceneManager.GetActiveScene().name}_OnPsEnd",0.3f);
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        _particlePool.Enqueue(ps); // Return the particle system to the pool
    }

    #endregion
}