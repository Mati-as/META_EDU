using UnityEngine;
using UnityEngine.InputSystem;

public class Space_EffectController : Base_EffectController
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

        wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in _particles) particlePool.Push(ps);
    }

    // public void SetInputSystem()
    // {
    //     _camera = Camera.main;
    //     _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
    //     _mouseClickAction.performed += OnMouseClick;
    // }


    // private void OnMouseClick(InputAction.CallbackContext context)
    // {
    //     var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
    //     RaycastHit hit;
    //
    //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
    //         PlayParticle(hit.point,
    //             _adSources, _burstAdSources,
    //             ref _currentCountForBurst, false, wait: 3.4f);
    // }

    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point,
                _adSources, _burstAdSources,
                ref _currentCountForBurst, false, wait: 3.4f);
        }
    }

    // 하위 메소드 모두 BaseController로 이전 및 상속. 정상작동 (11/23/23)       


    // protected override void PlayParticle(Vector3 position, AudioSource[] audioSources, AudioSource[]
    //         burstAudioSources, ref int _currentCountForBurst, bool isBurst = false,
    //     int emitAmount = 2, int burstCount = 10, int burstAmount = 5, float wait = 3f)
    // {
    //     base.PlayParticle(position, audioSources, burstAudioSources, ref _currentCountForBurst, isBurst, emitAmount, burstCount, burstAmount, wait);
    //     
    // }

//     public override void PlayParticle(Vector3 position, bool isBurst = false)
//     {
//         if ((particlePool.Count < emitAmount || (_currentCountForBurst < burstCount && particlePool.Count < burstCount)))
//         {
//             // 에디터상에서 배치한 순서대로 파티클을 Push하기 위해 for문 사용합니다. 
//             for (var i = 0; i < 2; i++)
//             {
//                 foreach (var ps in _particles)
//                 {
//                     GrowPool(ps);
//                 }
//             }
//
// #if UNITY_EDITOR
//             Debug.Log("no particles in the pool. creating particles and push...");
// #endif
//         }
//         
//         if (particlePool.Count >= emitAmount)
//         {
//             if (_currentCountForBurst > burstCount || isBurst)
//             {
//                 if (particlePool.Count <= burstAmount)
//                 {
//                     foreach (var ps in _particles) GrowPool(ps);
//                 }
//              
//                 TurnOnParticle(position, burstCount);
//                 FindAndPlayAudio(isBurst:true);
//                 _currentCountForBurst = 0;
//             }
//             else
//             {
//                 TurnOnParticle(position, emitAmount);
//                 FindAndPlayAudio();
//                 _currentCountForBurst++;
//             }
//             
//         }
//     }
//

//     private void TurnOnParticle(Vector3 position, int loopCount = 2, float delayToReturn = 3f)
//     {
//         for (var i = 0; i < loopCount; i++)
//         {
//             var ps = particlePool.Pop();
//             ps.transform.position = position;
//             ps.gameObject.SetActive(true);
//             ps.Play();
// #if UNITY_EDITOR
//             Debug.Log("enough particles in the pool.");
// #endif
//             StartCoroutine(ReturnToPoolAfterDelay(ps, delayToReturn));
//         }
//     }
//
//
//     private void FindAndPlayAudio(bool isBurst = false, bool recursive = false)
//     {
//         if (!isBurst)
//         {
//             var availableAudioSource = Array.Find(_adSources, audioSource => !audioSource.isPlaying);
//
//             if (availableAudioSource != null)
//             {
//                 FadeInSound(availableAudioSource);
//             }
//             else
//             {
// #if UNITY_EDITOR
//                 Debug.LogWarning("No available AudioSource!");
// #endif
//             }
//         }
//         else if(isBurst)
//         {
//             var availableAudioSourceBurst = Array.Find(_burstAdSources, audioSource => !audioSource.isPlaying);
//             FadeInSound(availableAudioSourceBurst);
//
//             if (availableAudioSourceBurst != null)
//             {
//                 FadeInSound(availableAudioSourceBurst);
//             }
//             else
//             {
// #if UNITY_EDITOR
//                 Debug.LogWarning("No available AudioSource!");
// #endif
//             }
//         }
//         
// #if UNITY_EDITOR
//         Debug.LogWarning("Audio Played");
// #endif
//     }
//
}