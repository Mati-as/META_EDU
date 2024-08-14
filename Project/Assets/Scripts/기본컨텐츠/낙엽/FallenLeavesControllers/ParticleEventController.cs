using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ParticleEventController : IGameManager, IOnClicked
{
    private ParticleSystem.Particle[] particles;

    [Header("Fallen Leaves Particle")] [Space(10f)] [SerializeField]
    public ParticleSystem particleSystemA;

    [SerializeField] public ParticleSystem particleSystemB;
    [SerializeField] public ParticleSystem particleSystemC;

    [Header("Wind Power Setting")] [Space(10f)]
    public float force = 10.0f;

    private readonly float radius = 987654321;
    public float rotationPower;

    private readonly Vector3 center = new(0, 0, 0);

    public float angularSpeedWhenStop;
    public float forceWhenStop;

    [Header("Wind Frequency Setting")] [Space(10f)] [Range(0, 100)]
    public float randomTimeMin;

    [Range(0, 200)] public float randomTimeMax;

    [SerializeField] private float _randomTime;

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


    [Header("Sound Setting")] [Space(10f)] [SerializeField]
    private AudioClip clickRustlingSound;


    private AudioSource[] _audioSources;
    private Camera _camera;
    private InputAction _mouseClickAction;



    private int _count = 0;
    

    protected override void Init()
    {
        base.Init();
        

        _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        Subscribe();
        StopAllParticles();
    }

    private void Start()
    {
        PlayAllParticles();
    }


    
    private IOnClicked _iOnClicked;



    public new void OnClicked()
         {
             
         }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!PreCheck()){ return;}

        if (SceneManager.GetActiveScene().name != "BC001") return;

#if UNITY_EDITOR
     
#endif

      //  var randomChar = (char)Random.Range('A', 'C' + 1);
      //  Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/낙엽/Click" +randomChar,0.1f);
               
            


        foreach (var _hit in GameManager_Hits)
        {
            
            _hit.transform.gameObject.TryGetComponent(out _iOnClicked);
            _iOnClicked?.OnClicked();

            Debug.Log("Ray _hit: " + _hit.transform.name);
            ClickEventApplyRadialForce(_hit.point, particleSystemA);
            ClickEventApplyRadialForce(_hit.point, particleSystemB);
            ClickEventApplyRadialForce(_hit.point, particleSystemC);
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
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/낙엽/RollingLeaves");
         
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/낙엽/Wind Blowing Sound");
            randomDirection = new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));


            isWindBlowing = true;


            ApplyWindRandomForce(center, particleSystemA, randomWindAngularMin, randomWindAngularMax,
                randomWindForceMin, randomWindForceMax);
            ApplyWindRandomForce(center, particleSystemB, randomWindAngularMin,
                randomWindAngularMax, randomWindForceMin, randomWindForceMax);
            ApplyWindRandomForce(center, particleSystemC, randomWindAngularMin,
                randomWindAngularMax, randomWindForceMin, randomWindForceMax);


            _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        }

        //_angularStopElapse += Time.deltaTime;

        if (isWindBlowing)
        {
           


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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Unsubscribe();
#if UNITY_EDITOR
        Debug.Log($"Destroy GameManager :  {gameObject.name}");
#endif
        Destroy(gameObject);
        
    }


    //  메소드 목록   -----------------------

    private void Subscribe()
    {
     

        FallenLeave_NewUI_Manager.OnStart -= PlayAllParticles;
        FallenLeave_NewUI_Manager.OnStart += PlayAllParticles;
    }

    private void Unsubscribe()
    {
        FallenLeave_NewUI_Manager.OnStart -= PlayAllParticles;
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


    private void ApplyWindRandomForce(Vector3 position, ParticleSystem particleSystem, float angularRandomMin,
        float angualrRandomMax, float randomForceMin, float randomForceMax)
    {
        var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
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

                var randomForce = Random.Range(randomForceMin, randomForceMax);
                var forceMultiplier = randomForce / (1.0f + distance); // 거리에 반비례하는 힘
                particles[i].velocity += randomDirection * force * forceMultiplier;
            }
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }


    private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem)
    {
        var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
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

                var forceMultiplier = 1 / (1.0f + distance); // 거리에 반비례하는 힘
                var direction = (particles[i].position - position).normalized;
                particles[i].velocity += direction * clickForce * forceMultiplier;
            }
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}