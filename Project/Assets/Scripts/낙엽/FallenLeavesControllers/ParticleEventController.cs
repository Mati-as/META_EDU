using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ParticleEventController : MonoBehaviour
{
    private ParticleSystem.Particle[] particles;
    [Header("Fallen Leaves Particle")] [Space(10f)]
    [SerializeField] public ParticleSystem particleSystemA;
    [SerializeField] public ParticleSystem particleSystemB;
    [SerializeField] public ParticleSystem particleSystemC;

    [Header("Wind Power Setting")] [Space(10f)]
    public float force = 10.0f;
    private readonly float radius = 987654321;
    public float rotationPower;

    private Vector3 center = new Vector3(0, 0, 0);

    public float angularSpeedWhenStop;
    public float forceWhenStop;
    [Header("Wind Frequency Setting")] [Space(10f)]
    [Range(0,100)]
    public float randomTimeMin;
    [Range(0,200)]
    public float randomTimeMax;
 
    [SerializeField]
    private float _randomTime;
    
    private float _elapsedTime;
    private float _angularStopElapse;
    public float angularStopWaitTime;

    public static bool isWindBlowing;

    private bool _isAngularZero;

    [Header("physical parameter Setting")] [Space(10f)]
    public static Vector3 randomDirection; //해바라기 방향 조정에 사용.
    public float randomWindForceMax;
    public float randomWindForceMin;
    public float randomWindAngularMin;
    public float randomWindAngularMax;
    public float clickForce = 10.0f;
    public float clickRadius = 5.0f;
    public float clickRotationPower;
    
   
    [Header("Sound Setting")] [Space(10f)]
    [SerializeField] private AudioClip clickRustlingSound;
    [SerializeField] private AudioClip clickPopSound;
    [SerializeField] private AudioClip windBlowingSound;
    [SerializeField] private AudioClip rollingLeaves;
    
   
    private AudioSource[] _audioSources;
    private Camera _camera;
    private InputAction _mouseClickAction;

   
    enum FallenLeave_SoundID
    {
        RollingLeaves,
        Blowing,
        MouseClick,
        ClickPop
    }

    private int _count = 0;
    private void Awake()
    {
        _audioSources = GetComponents<AudioSource>();
                     
        _audioSources[(int)FallenLeave_SoundID.RollingLeaves].clip = rollingLeaves;
        _audioSources[(int)FallenLeave_SoundID.Blowing].clip = windBlowingSound;
        
        
        for(int i = (int)FallenLeave_SoundID.MouseClick ; i < _audioSources.Length ;i++)
        {
            
            if (i % 2 == 0)
            {
#if UNITY_EDITOR
                Debug.Log("클립할당");
#endif
                _audioSources[i].clip = clickPopSound;
            }
            else
            {
                _audioSources[i].clip = clickRustlingSound;
            }
        }
        
        _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        Subscribe();
        StopAllParticles();
    }
   
    private void Start()
    {
        _camera = Camera.main;
        
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;
        _mouseClickAction.Enable();
        
        PlayAllParticles();
        
    }
    
    private void OnDisable()
    {
        _mouseClickAction.Disable();
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        
        for(int i = 2 ;i < _audioSources.Length ;i += 2)
        {
            if (!_audioSources[i].isPlaying)
            {
                SoundManager.FadeInAndOutSound(_audioSources[i],1.0f,0.05f
                    ,duration,0.05f,rollBack:true);
                
                SoundManager.FadeInAndOutSound(_audioSources[i+1],0.05f,0.05f
                    ,0.9f,0.05f,rollBack:true);
#if UNITY_EDITOR
                Debug.Log("클릭소리 재생");
#endif
                break;

            }
        }
      
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log("Ray hit: " + hit.transform.name);
            ClickEventApplyRadialForce(hit.point,particleSystemA);
            ClickEventApplyRadialForce(hit.point,particleSystemB);
            ClickEventApplyRadialForce(hit.point,particleSystemC);
        }
    }

    public float duration;
    
    
    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        
       
        if (_elapsedTime > _randomTime)
        {
#if UNITY_EDITOR
            Debug.Log("바람소리 재생");
#endif
            SoundManager.FadeInAndOutSound(_audioSources[(int)FallenLeave_SoundID.RollingLeaves],1f,0.01f
                ,5f,0.5f);
            SoundManager.FadeInAndOutSound(_audioSources[(int)FallenLeave_SoundID.Blowing],0.18f,0.01f
                ,5f,0.5f);
            randomDirection = new Vector3(Random.Range(-2, 2), 0 , Random.Range(-2, 2));
            
            
            isWindBlowing = true;

            
            ApplyWindRandomForce(center, particleSystemA,randomWindAngularMin,randomWindAngularMax,
                randomWindForceMin,randomWindForceMax);
            ApplyWindRandomForce(center, particleSystemB,randomWindAngularMin,
                randomWindAngularMax,randomWindForceMin,randomWindForceMax);
            ApplyWindRandomForce(center, particleSystemC,randomWindAngularMin,
                randomWindAngularMax,randomWindForceMin,randomWindForceMax);
         
            
            _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        }
        
        //_angularStopElapse += Time.deltaTime;

        if (isWindBlowing)
        {
            Debug.Log($"바람 멈추기위한 isWindowBlowing Logic 진입..{angularStopWaitTime}");

           
            
            DOVirtual
                .Float(0, 1, angularStopWaitTime, val => _elapsedTime = val)
                .OnComplete(() =>
                {
                    // if (_angularStopElapse > angularStopWaitTime)
                    //     if (!_isAngularZero)
                    //     {
                    
                    _elapsedTime = 0;
                    
                    Debug.Log("바람 멈추기");
                    _isAngularZero = true;
                    ApplyWindRandomForce(center, particleSystemA, -angularSpeedWhenStop, angularSpeedWhenStop,
                        -forceWhenStop, forceWhenStop);
                    ApplyWindRandomForce(center, particleSystemB, -angularSpeedWhenStop, angularSpeedWhenStop,
                        -forceWhenStop, forceWhenStop);
                    ApplyWindRandomForce(center, particleSystemC, -angularSpeedWhenStop, angularSpeedWhenStop,
                        -forceWhenStop, forceWhenStop);
                    // }
                });
            
            isWindBlowing = false;
        }


        //else if (isWindBlowing) _angularStopElapse += Time.deltaTime;
    }
    private void OnDestroy()
    {
        Unsubscribe();
    }

  
    //  메소드 목록   -----------------------
    
    private void Subscribe()
    {
        FallenLeafInstructionButtonEventListener.FallenLeaveStartButtonEvent -= PlayAllParticles;
        FallenLeafInstructionButtonEventListener.FallenLeaveStartButtonEvent += PlayAllParticles;
    }

    private void Unsubscribe()
    {
        FallenLeafInstructionButtonEventListener.FallenLeaveStartButtonEvent -= PlayAllParticles;
    }
    
    private void PlayAllParticles()
    {
        particleSystemA.Play();
        particleSystemB.Play();
        particleSystemC.Play();
    }

    private void StopAllParticles()
    {
        particleSystemA.Stop();
        particleSystemB.Stop();
        particleSystemC.Stop();
    }
    
    

    private void ApplyWindRandomForce(Vector3 position, ParticleSystem particleSystem,float angularRandomMin,
        float angualrRandomMax,float randomForceMin, float randomForceMax)
    {
      
        
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        var numParticlesAlive = particleSystem.GetParticles(particles);
        
        for (var i = 0; i < numParticlesAlive; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);

            if (distance < radius)
            {
                var distanceFactor = 1f - distance / radius;

                var randomAngularVelocity = Random.Range(angularRandomMin * rotationPower * distanceFactor,
                    angualrRandomMax * rotationPower * distanceFactor);

                particles[i].angularVelocity = randomAngularVelocity;

                float randomForce = Random.Range(randomForceMin,randomForceMax);
                var forceMultiplier =  randomForce / (1.0f + distance); // 거리에 반비례하는 힘
                particles[i].velocity += randomDirection * force * forceMultiplier;
            }
        }
        particleSystem.SetParticles(particles, numParticlesAlive);
      
    }
    


    private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem)
    { 
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        var numParticlesAlive = particleSystem.GetParticles(particles);
        
        for (var i = 0; i < numParticlesAlive; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);

            if (distance < clickRadius)
            {
              
                var distanceFactor = 1f - distance / (clickRadius * 100);

                var randomAngularVelocity = Random.Range(-10f * clickRotationPower * distanceFactor,
                    10f * clickRotationPower * distanceFactor);

                particles[i].angularVelocity = randomAngularVelocity;
                
                var forceMultiplier =  1 / (1.0f + distance); // 거리에 반비례하는 힘
                var direction = (particles[i].position - position).normalized;
                particles[i].velocity += direction * clickForce * forceMultiplier;
            }
        }
        particleSystem.SetParticles(particles, numParticlesAlive);
    }

   

 

}