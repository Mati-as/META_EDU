
using System;
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
    public int audioSize;

    private void Awake()
    {
        Init();
        SetInputSystem();
        
        _adSources = new AudioSource[audioSize];
        
        for (int i = 0; i < audioSize; i++)
        {
            _adSources[i] = gameObject.AddComponent<AudioSource>();
            _adSources[i].clip = _effectClip;
            _adSources[i].playOnAwake = false;
            _adSources[i].pitch = UnityEngine.Random.Range(0.75f, 1.4f);
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
        
        
        if (particlePool.Count >= emitAmount)
        {
            
           
                for (var i = 0; i < emitAmount; i++)
                {
                    var ps = particlePool.Pop();
                    ps.transform.position = position;
                    ps.gameObject.SetActive(true);
                    ps.Play();
                    
#if UNITY_EDITOR
    Debug.Log("enough particles in the pool.");
#endif
                    StartCoroutine(ReturnToPoolAfterDelay(ps, 3f));
                }
                
                _currentCountForBurst++;
                
                var availableAudioSource = Array.Find(_adSources, audioSource => !audioSource.isPlaying);
                if (availableAudioSource != null)
                {
                    FadeInSound(availableAudioSource);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("No available AudioSource!");
#endif
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
                
                StartCoroutine(ReturnToPoolAfterDelay(ps, 3f));
            }

            var availableAudioSource = Array.Find(_adSources, audioSource => !audioSource.isPlaying);
            if (availableAudioSource != null)
            {
                FadeInSound(availableAudioSource);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("No available AudioSource!");
#endif
            }
            
           
        }
    }

    private readonly float _returnWaitForSeconds = 3f;


    public void SetInputSystem()
    {
        _camera = Camera.main;
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;

    }


}