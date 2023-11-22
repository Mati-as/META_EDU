using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class KoreanFlag_EffectController : Base_EffectController
{
    private ParticleSystem[] particleSystems;

 
   
    private AudioSource[] _adSources;
    private AudioSource _audioSource;
    private AudioSource _subAudioSourceA;
    private AudioSource _subAudioSourceB;
    private AudioSource _subAudioSourceC;
    public AudioClip _effectClip;
    

    private void Awake()
    {
        Init();
        SetInputSystem();
        
        _adSources = new AudioSource[120];
        
        for (int i = 0; i < 20; i++)
        {
            _adSources[i] = gameObject.AddComponent<AudioSource>();
            _adSources[i].clip = _effectClip;
            _adSources[i].playOnAwake = false;
            _adSources[i].pitch = Random.Range(0.75f, 1.4f);
        }
        
        wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in _particles) particlePool.Push(ps);
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
            PlayParticle(hit.point);
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
    

    private readonly string LAYER_NAME = "UI";
    private RaycastHit _hit;
    


    public float targetVol;
    private int _count;
    public int emitAmount;
    public int burstAmount;
    public int burstCount;
    private int _currentCountForBurst;

    public override void PlayParticle(Vector3 position)
    {
        if (particlePool.Count >= emitAmount && particlePool.Count > burstCount)
        {
            if (_currentCountForBurst < burstCount)
            {
                for (var i = 0; i < emitAmount; i++)
                {
                    var ps = particlePool.Pop();
                    ps.transform.position = position;
                    ps.gameObject.SetActive(true);
                    ps.Play();
                    
                    for (var k = 0; k < _adSources.Length; k++)
                        if (!_adSources[k].isPlaying)
                        {
                            FadeInSound(_adSources[k]);
                            break;
                        }
                    
                    #if UNITY_EDITOR
                    Debug.Log("enough particles in the pool.");
                    #endif
                    _currentCountForBurst++;
                    StartCoroutine(ReturnToPoolAfterDelay(ps, 3f));
                }
            }
            
            else
            {
                _currentCountForBurst = 0;

                for (var i = 0; i < burstAmount; i++)
                {
                    var ps = particlePool.Pop();
                    ps.transform.position = position;
                    ps.gameObject.SetActive(true);
                    ps.Play();

#if UNITY_EDITOR
                    Debug.Log("to burst, enough particles in the pool.");
#endif
                    StartCoroutine(ReturnToPoolAfterDelay(ps, 3f));

                    for (var k = 0; k < _adSources.Length; k++)
                        if (!_adSources[k].isPlaying)
                        {
                            FadeInSound(_adSources[k]);
                            break;
                        }
                }
            }
        }
        else
        {
            foreach(ParticleSystem ps in _particles)
            {
                GrowPool(ps);
            }

#if UNITY_EDITOR
            Debug.Log("no particles in the pool. creating particles and push...");
#endif
            
            for (var k = 0; k < emitAmount; k++)
            {
                var ps = particlePool.Pop();
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();

                for (var j = 0; j < _adSources.Length; j++)
                    if (!_adSources[j].isPlaying)
                    {
                        FadeInSound(_adSources[j]);
                        break;
                    }
                
                StartCoroutine(ReturnToPoolAfterDelay(ps, 3f));
            }

            Debug.LogWarning("No particles available in pool!");
        }
    }

    private readonly float _returnWaitForSeconds = 3f;


    public void SetInputSystem()
    {
        _camera = Camera.main;
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;

    }

//     private void GrowPool(ParticleSystem original, int count)
//     {
//         for (var i = 0; i < count; i++)
//         {
//             var newInstance = Instantiate(original);
//             newInstance.gameObject.SetActive(false);
//             particlePool.Push(newInstance);
//         }
//     }
// //
//     public void FadeOutSound(float target, float duration, AudioSource audioSource)
//     {
//         audioSource.DOFade(0f, duration).SetDelay(fadeInDuration).OnComplete(() =>
//         {
// #if UNITY_EDITOR
//             Debug.Log("audioQuit");
// #endif
//             audioSource.Stop();
//         });
//     }
//
//     public void FadeInSound(float targetVolume, float duration, AudioSource audioSource)
//     {
// #if UNITY_EDITOR
//         Debug.Log("audioPlay");
// #endif
//         audioSource.Play();
//         audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(0.05f, 0.5f, audioSource); });
//     }
}