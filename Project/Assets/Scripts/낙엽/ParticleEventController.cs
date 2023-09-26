using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

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
    [SerializeField] private AudioClip windBlowingSound;
    private AudioSource _audioSource;
    private event Action WindBlowTriggerEvent;
    
   
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = windBlowingSound;
        _audioSource.Stop();
        
        _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        Subscribe();
        StopAllParticles();

    }
    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Ray hit: " + hit.transform.name);
                ClickEventApplyRadialForce(hit.point,particleSystemA);
                ClickEventApplyRadialForce(hit.point,particleSystemB);
                ClickEventApplyRadialForce(hit.point,particleSystemC);
            }
        }
        
        
       
        if (_elapsedTime > _randomTime)
        {
            randomDirection = new Vector3(Random.Range(-2, 2), 0 , Random.Range(-2, 2));
            isWindBlowing = true;
           
           
            
            ApplyWindRandomForce(center, particleSystemA,randomWindAngularMin,randomWindAngularMax,
                randomWindForceMin,randomWindForceMax);
            ApplyWindRandomForce(center, particleSystemB,randomWindAngularMin,
                randomWindAngularMax,randomWindForceMin,randomWindForceMax);
            ApplyWindRandomForce(center, particleSystemC,randomWindAngularMin,
                randomWindAngularMax,randomWindForceMin,randomWindForceMax);
            
            _audioSource.Play();
            
            _angularStopElapse = 0f;
            _elapsedTime = 0f;
            _isAngularZero = false;
            _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        }
            
        else
        {
            isWindBlowing = false;
        }

        if (!isWindBlowing)
        {
            _angularStopElapse += Time.deltaTime;
            
            if (_angularStopElapse > angularStopWaitTime)
            {
                if (!_isAngularZero)
                {
                    Debug.Log("바람 멈추기");
                    _isAngularZero = true;
                    ApplyWindRandomForce(center, particleSystemA,-angularSpeedWhenStop,angularSpeedWhenStop,-forceWhenStop,forceWhenStop);
                    ApplyWindRandomForce(center, particleSystemB,-angularSpeedWhenStop,angularSpeedWhenStop,-forceWhenStop,forceWhenStop);
                    ApplyWindRandomForce(center, particleSystemC,-angularSpeedWhenStop,angularSpeedWhenStop,-forceWhenStop,forceWhenStop);
                }
               
            }
        }
        
        
        else if (isWindBlowing)
        {
            _angularStopElapse += Time.deltaTime;
        }
        
        
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