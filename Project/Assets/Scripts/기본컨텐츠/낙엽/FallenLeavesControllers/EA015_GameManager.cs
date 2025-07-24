using System;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class EA015_Payload : IPayload
{
    public bool IsCustom
    {
        get;
    }
    
    public bool IsPopFromZero
    {
        get;
    }

    public string Narration
    {
        get;
    }


    public EA015_Payload(string narration, bool isCustomOn = false,bool isPopFromZero = true)
    {
        IsCustom = isCustomOn;
        Narration = narration;
        IsPopFromZero = isPopFromZero;
    }
}

public class EA015_GameManager : Base_GameManager, IOnClicked
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

    [SerializeField] [Range(5, 60)] private float _timeForLeavesRemovalSession;
    private float _elapsedTimeForLeavesRemovalSession;
    private bool _isLeavesRemovalSession;

    private enum LeaveColor
    {
        Red,
        Brown,
        Yellow
    }

    private LeaveColor currentLeaveColorToRemove = (LeaveColor)123;

    private bool isClickableOnSessionTerm;

    private void OnRemovalSession()
    {
        isClickableOnSessionTerm = false;
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA015/OnAllLeavesRemoved");

        DOVirtual.DelayedCall(1.5f, () =>
        {
            
            Managers.Sound.Play(SoundManager.Sound.Narration, "EA015/LetsClean");
            Messenger.Default.Publish(new EA015_Payload("색깔에 맞춰 낙엽을 터치해주세요!", true));
        });
        
        DOVirtual.DelayedCall(4f, () =>
        {
            ChangeCurrentLeaveColorToRemove(LeaveColor.Red);
            particleSystemA.Stop();
            particleSystemB.Stop();
            particleSystemC.Stop();
        });
    }

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        Messenger.Default.Publish(new EA015_Payload("낙엽을 밟으며 날려봐요!", true));
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA015/PlayWithLeaves");
    }

    private void ChangeCurrentLeaveColorToRemove(LeaveColor leaveColor)
    {
        isClickableOnSessionTerm = false;
       
        DOVirtual.DelayedCall(1.0f, () =>
        {
            switch (leaveColor)
            {
                case LeaveColor.Red:
                    Messenger.Default.Publish(new EA015_Payload("빨강색 낙엽을 터치해 없애주세요!", true));
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA015/TouchRed");
                    break;
                case LeaveColor.Brown:
                    Messenger.Default.Publish(new EA015_Payload("갈색 낙엽을 터치해 없애주세요!", true));
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA015/TouchBrown");
                    break;
                case LeaveColor.Yellow:
                    Messenger.Default.Publish(new EA015_Payload("노란색 낙엽을 터치해 없애주세요!", true));
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA015/TouchYellow");
                    break;
            }

        });
        
    
        DOVirtual.DelayedCall(5f, () =>
        {
            currentLeaveColorToRemove = leaveColor;
            isClickableOnSessionTerm = true;
        });
     
    }



    [Header("Sound Setting")] [Space(10f)] [SerializeField]
    private AudioClip clickRustlingSound;


    private AudioSource[] _audioSources;
    private Camera _camera;
    private InputAction _mouseClickAction;


    private int _count = 0;


    protected override void Init()
    {
        SHADOW_MAX_DISTANCE = 60;
        PsResourcePath = "Runtime/EA015/Fx_Click";
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


    public void OnClicked()
    {
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        
        

      


        foreach (var _hit in GameManager_Hits)
        {
            if (_isLeavesRemovalSession)
            {
                if (isClickableOnSessionTerm) OnRaySyncedOnRemovalSession(_hit.point);
                _hit.transform.gameObject.TryGetComponent(out _iOnClicked);
                _iOnClicked?.OnClicked();
                //  OnRemovalSession();
            }
            else
            {
                PlayParticleEffect(_hit.point);
                _hit.transform.gameObject.TryGetComponent(out _iOnClicked);
                _iOnClicked?.OnClicked();
                DEV_OnValidClick();

                char randomChar = (char)Random.Range('A', 'C' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA015/LeaveClick" + randomChar,0.1F);
                
                ClickEventApplyRadialForce(_hit.point, particleSystemA);
                ClickEventApplyRadialForce(_hit.point, particleSystemB);
                ClickEventApplyRadialForce(_hit.point, particleSystemC);
            }

            Debug.Log("Ray _hit: " + _hit.transform.name);
        }
    }


    private void OnAllLeavesRemoved()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA015/OnAllLeavesRemoved");
        Managers.Sound.Play(SoundManager.Sound.Narration,"EA015/OnFinishCleaningLeaves");
        DOVirtual.DelayedCall(0.5f,()=>
        {
            Messenger.Default.Publish(new EA015_Payload("잘 했어! 이제 다른 낙엽도 청소하자!", true));
        });
    }
    private void OnRaySyncedOnRemovalSession(Vector3 pos)
    {
        if (currentLeaveColorToRemove == LeaveColor.Red)
        {
            RemoveParticles(pos, particleSystemA);
            PlayParticleEffect(pos);

      
            if (currentRemovedCount >= LEAVES_COUNT_TO_REMOVE)
            {
                isClickableOnSessionTerm = false;
                currentRemovedCount = 0;
                _elapsedTimeForLeavesRemovalSession = 0;
            
                OnAllLeavesRemoved();
                DOVirtual.DelayedCall(3f, () =>
                {
                    ChangeCurrentLeaveColorToRemove(LeaveColor.Brown);
                });
                
                var particlesA = new ParticleSystem.Particle[particleSystemA.main.maxParticles];
                int numParticlesAliveA = particleSystemA.GetParticles(particlesA);

        
                for (int i = 0; i < numParticlesAliveA; i++)
                {
                    int index = i; // Capture index for closure

                    Vector3 originalSize = particlesA[index].startSize3D;
                    Vector3 targetSize = Vector3.zero;


            
                    DOVirtual.Vector3(originalSize, targetSize, 0.15f, size =>
                    {
                        particlesA[index].startSize3D = size;
                        particleSystemA.SetParticles(particlesA, numParticlesAliveA);
                    }).OnComplete(() =>
                    {
                        PlayParticleEffect( particlesA[index].position);
                  //      if(currentRemovedCount!=LEAVES_COUNT_TO_REMOVE)Messenger.Default.Publish(new EA015_Payload($"{ Mathf.Clamp(LEAVES_COUNT_TO_REMOVE - currentRemovedCount, 0, LEAVES_COUNT_TO_REMOVE)}개", true,false));
                    }).SetEase(Ease.InOutBounce);
                }
               
            }
        }

        else if (currentLeaveColorToRemove == LeaveColor.Brown)
        {
            RemoveParticles(pos, particleSystemB);
            PlayParticleEffect(pos);
          
            if (currentRemovedCount >= LEAVES_COUNT_TO_REMOVE)
            {
                isClickableOnSessionTerm = false;
                currentRemovedCount = 0;
                _elapsedTimeForLeavesRemovalSession = 0;
                OnAllLeavesRemoved();
                DOVirtual.DelayedCall(3f, () =>
                {
                    ChangeCurrentLeaveColorToRemove(LeaveColor.Yellow);
                });
                
                var particlesA = new ParticleSystem.Particle[particleSystemB.main.maxParticles];
                int numParticlesAliveA = particleSystemB.GetParticles(particlesA);

        
                for (int i = 0; i < numParticlesAliveA; i++)
                {
                    int index = i; // Capture index for closure

                    Vector3 originalSize = particlesA[index].startSize3D;
                    Vector3 targetSize = Vector3.zero;


            
                    DOVirtual.Vector3(originalSize, targetSize, 0.15f, size =>
                    {
                        particlesA[index].startSize3D = size;
                        particleSystemB.SetParticles(particlesA, numParticlesAliveA);
                    }).OnComplete(() =>
                    {
                        PlayParticleEffect( particlesA[index].position);
                  //      if(currentRemovedCount!=LEAVES_COUNT_TO_REMOVE)Messenger.Default.Publish(new EA015_Payload($"{ Mathf.Clamp(LEAVES_COUNT_TO_REMOVE - currentRemovedCount, 0, LEAVES_COUNT_TO_REMOVE)}개", true,false));
                    }).SetEase(Ease.InOutBounce);
                }
               
            }
        }
        else if (currentLeaveColorToRemove == LeaveColor.Yellow)
        {
            RemoveParticles(pos, particleSystemC);
            PlayParticleEffect(pos);
            
            if (currentRemovedCount >= LEAVES_COUNT_TO_REMOVE)
            {
                isClickableOnSessionTerm = false;
                currentRemovedCount = 0;
                _elapsedTimeForLeavesRemovalSession = 0;
                ChangeCurrentLeaveColorToRemove((LeaveColor)123);
                
                DOVirtual.DelayedCall(1.5f, () =>
                {
                 //   RemoveAllLeaves();
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA015/OnAllFinished");
                    Messenger.Default.Publish(new EA015_Payload("낙엽을 전부 치웠어!", true,true));
                });
                var particlesA = new ParticleSystem.Particle[particleSystemC.main.maxParticles];
                int numParticlesAliveA = particleSystemC.GetParticles(particlesA);

        
                for (int i = 0; i < numParticlesAliveA; i++)
                {
                    int index = i; // Capture index for closure

                    Vector3 originalSize = particlesA[index].startSize3D;
                    Vector3 targetSize = Vector3.zero;


            
                    DOVirtual.Vector3(originalSize, targetSize, 0.15f, size =>
                    {
                        particlesA[index].startSize3D = size;
                        particleSystemC.SetParticles(particlesA, numParticlesAliveA);
                    }).OnComplete(() =>
                    {
                        PlayParticleEffect( particlesA[index].position);
                      //  if(currentRemovedCount!=LEAVES_COUNT_TO_REMOVE)Messenger.Default.Publish(new EA015_Payload($"{ Mathf.Clamp(LEAVES_COUNT_TO_REMOVE - currentRemovedCount, 0, LEAVES_COUNT_TO_REMOVE)}개", true,false));
                    }).SetEase(Ease.InOutBounce);
                }
            }
            
        }
    }

    private const int LEAVES_COUNT_TO_REMOVE = 25;
    private int currentRemovedCount;
    public float duration;
    

    private void Update()
    {
        if (_isLeavesRemovalSession) return;
        if(!isStartButtonClicked) return;

        _elapsedTime += Time.deltaTime;
        _elapsedTimeForLeavesRemovalSession += Time.deltaTime;


        if (_elapsedTimeForLeavesRemovalSession > _timeForLeavesRemovalSession)
        {
            _isLeavesRemovalSession = true;
            currentLeaveColorToRemove = (LeaveColor)Random.Range(0, 3);
            _elapsedTimeForLeavesRemovalSession = 0;
            
            OnRemovalSession();
        }


        if (_elapsedTime > _randomTime)
        {

            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/낙엽/RollingLeaves");

            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/낙엽/Wind Blowing Sound");
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
        int numParticlesAlive = particleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distance = Vector3.Distance(position, particles[i].position);

            if (distance < radius)
            {
                float distanceFactor = 1f - distance / radius;

                float randomAngularVelocity = Random.Range(angularRandomMin * rotationPower * distanceFactor,
                    angualrRandomMax * rotationPower * distanceFactor);

                particles[i].angularVelocity = randomAngularVelocity;

                float randomForce = Random.Range(randomForceMin, randomForceMax);
                float forceMultiplier = randomForce / (1.0f + distance); // 거리에 반비례하는 힘
                particles[i].velocity += randomDirection * force * forceMultiplier;
            }
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }

    [SerializeField] [Range(0, 1f)] private float clickRadiusForRemoveLeaves = 0.1f;
    private bool _isRemoving;


    
    private void RemoveParticles(Vector3 position, ParticleSystem particleSystem)
    {
        var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        int numParticlesAlive = particleSystem.GetParticles(particles);

        bool isAlreadyRemoved = false;
        bool foundAnyParticle = false;

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distance = Vector3.Distance(position, particles[i].position);

            if (particles[i].startSize3D.x < 0.02f)
            {
                Logger.ContentTestLog("이미 작아짐");
                continue;
            }
            
            if (distance < clickRadiusForRemoveLeaves)
            {
                foundAnyParticle = true;

                int index = i; // Capture index for closure

                Vector3 originalSize = particles[index].startSize3D;
                Vector3 targetSize = Vector3.zero;


            
                    DOVirtual.Vector3(originalSize, targetSize, 0.12f, size =>
                    {
                        particles[index].startSize3D = size;
                        particleSystem.SetParticles(particles, numParticlesAlive);
                    }).OnComplete(() =>
                    {
                       
                        PlayParticleEffect( particles[index].position);
                      
                    }).SetEase(Ease.InOutBounce);
                    currentRemovedCount++;
                   
                    if(currentRemovedCount!=LEAVES_COUNT_TO_REMOVE)Messenger.Default.Publish(new EA015_Payload($"{ Mathf.Clamp(LEAVES_COUNT_TO_REMOVE - currentRemovedCount, 0, LEAVES_COUNT_TO_REMOVE)}개", true,false));
            }
        }

        if (!foundAnyParticle)
        {
            
            Debug.Log("❌ 클릭 위치 주변에 제거할 파티클이 없습니다.");
        }
        else
        {
            if (!_isRemoving)
            {
                char randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA015/Click" + randomChar);
                
                
                _isRemoving = true;
                DOVirtual.DelayedCall(0.1f,() =>
                {
                    _isRemoving = false;
            
                
                });
            }
            Debug.Log("✅ 파티클을 제거했습니다.");
          
        }
    }

    private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem)
    {
        var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        int numParticlesAlive = particleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distance = Vector3.Distance(position, particles[i].position);

            if (distance < clickRadius)
            {
                float distanceFactor = 1f - distance / (clickRadius * 100);

                float randomAngularVelocity = Random.Range(-10f * clickRotationPower * distanceFactor,
                    10f * clickRotationPower * distanceFactor);

                particles[i].angularVelocity = randomAngularVelocity;

                float forceMultiplier = 1 / (1.0f + distance); // 거리에 반비례하는 힘
                var direction = (particles[i].position - position).normalized;
                particles[i].velocity += direction * clickForce * forceMultiplier;
            }
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}