using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Turtle_EffectController : Base_EffectController
{
  
    private ParticleSystem[] particleSystem;

    private AudioSource[] _adSources;
    private AudioSource[] _burstAdSources;

    public AudioClip _effectClip;
    public AudioClip _burstClip;

    public int audioSize;
    private int _currentCountForBurst;
    private float _returnWaitForSeconds = 25f;
    
    private readonly string LAYER_NAME = "UI";
    private RaycastHit _hit;

    [Space(20f)]
    
    [Range(0.5f,2f)]
    public float pitch;
    [Range(0.5f,2f)]
    public float pitch_brust;
    [Range(0f,1f)]
    public float interval;

    
    // private ParticleSystem[] particleSystems;
    // private Camera _camera;
    // private InputAction _mouseClickAction;
    // private ParticleSystem[] _particles;
    // private WaitForSeconds wait_;
    // private AudioSource[] _adSources;
    // private AudioSource _audioSource;
    // private AudioSource _subAudioSourceA;
    // private AudioSource _subAudioSourceB;
    // private AudioSource _subAudioSourceC;
    // public AudioClip _effectClip;
    // private readonly Stack<ParticleSystem> particlePool = new();

    private void Awake()
    {
        Init();
        //SetInputSystem();
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
      
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point, _adSources, _burstAdSources
                , ref _currentCountForBurst
                , false,usePsMainDuration:true);
        }
    }

    protected override void Init()
    {
        burstAmount = 100;
        base.Init();

        _adSources = new AudioSource[audioSize];

        for (var i = 0; i < audioSize; i++)
        {
            _adSources[i] = gameObject.AddComponent<AudioSource>();
            _adSources[i].clip = _effectClip;
            _adSources[i].playOnAwake = false;
            _adSources[i].pitch = Random.Range(pitch - interval, pitch + interval);
        }

        _burstAdSources = new AudioSource[_burstAudioSize];

        for (var i = 0; i < _burstAudioSize; i++)
        {
            _burstAdSources[i] = gameObject.AddComponent<AudioSource>();
            _burstAdSources[i].clip = _burstClip;
            _burstAdSources[i].playOnAwake = false;
            _burstAdSources[i].pitch = Random.Range(pitch_brust - interval, pitch_brust + interval);
        }

        //  wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in _particles) particlePool.Enqueue(ps);
    }
    
    
    
//     private void OnMouseClick(InputAction.CallbackContext context)
//     {
//         var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
//         RaycastHit hit;
//
//         if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
//         {
//             
//             for(int i = 0;i < _adSources.Length;i++)
//             {
//                 if (!_adSources[i].isPlaying)
//                 {
//                     FadeInSound(targetVol,fadeInDuration,_adSources[i]);
//                     break;
//                 }
//             }
//         
//          
//             PlayParticle(hit.point);
//         }
//     }
//
//     public float targetVol;
//     private int _count;
//
//     private void PlayParticle(Vector3 position)
//     {
//         if (particlePool.Count >= 6)
//         {
//             for (var i = 0; i < _particles.Length; i++)
//             {
//                 var ps = particlePool.Pop();
//                 ps.transform.position = position;
//                 ps.gameObject.SetActive(true);
//                 ps.Play();
//
//                 StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
//             }
//         }
//         else
//         {
//             _count = 0;
//             foreach (var ps in _particles)
//             {
//                 GrowPool(_particles[_count], 1);
//                 _count++;
//             }
//
//             for (var i = 0; i < _particles.Length; i++)
//             {
//                 var ps = particlePool.Pop();
//                 ps.transform.position = position;
//                 ps.gameObject.SetActive(true);
//                 ps.Play();
//
//                 StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
//             }
//
//             Debug.LogWarning("No particles available in pool!");
//         }
//     }
//
//     private readonly float _returnWaitForSeconds = 5f;
//
//     private IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float delay)
//     {
//         yield return wait_;
//         ps.Stop();
//         ps.Clear();
//         ps.gameObject.SetActive(false);
//         particlePool.Push(ps); // Return the particle system to the pool
//     }
//
//
//     private void GrowPool(ParticleSystem original, int count)
//     {
//         for (var i = 0; i < count; i++)
//         {
//             var newInstance = Instantiate(original);
//             newInstance.gameObject.SetActive(false);
//             particlePool.Push(newInstance);
//         }
//     }
//     
//     public void FadeOutSound(float target,float duration,AudioSource audioSource)
//     {
//         audioSource.DOFade(0f, duration).SetDelay(fadeInDuration).OnComplete(() =>
//         {
//             
// #if UNITY_EDITOR
//             Debug.Log("audioQuit");
// #endif 
//             audioSource.Pause();
//         });
//     }
//
//     public void FadeInSound(float targetVolume, float duration,AudioSource audioSource)
//     {
//
//         #if UNITY_EDITOR
//         Debug.Log("audioPlay");
//         #endif 
//         audioSource.Play();
//         audioSource.DOFade(targetVolume, duration).OnComplete(() =>
//         {
//             FadeOutSound(0.05f, 0.5f,audioSource);
//         });
//         
//         
//     }
}