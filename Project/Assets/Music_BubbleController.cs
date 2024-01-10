using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Music_BubbleController : MonoBehaviour
{
    private ParticleSystem[]
        particleSystems;

    [Header("Reference")] [SerializeField] private Music_GameManager _gameManager;

    public float randomTime;
    private float _currentTime;

    private Stack<ParticleSystem> _clickEffectPoolSmall;
    private Stack<ParticleSystem> _clickEffectPoolBig;


    private ParticleSystem _effect_Small;
    private ParticleSystem _effect_Big;

    private void Awake()
    {   
        BindEvent();
        
    }
    

    private void Start()
    {  Init();
        var childParticleSystems = new List<ParticleSystem>();

      
        foreach (Transform child in transform)
        {
            var ps = child.GetComponent<ParticleSystem>();
            if (ps != null) childParticleSystems.Add(ps);
        }

        randomTime = Random.Range(40, 50);
        particleSystems = childParticleSystems.ToArray();
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime > randomTime)
        {
            var layerMask = LayerMask.GetMask("Screen");

            foreach (var ps in particleSystems)
                if (Physics.Raycast(ray, out rayCastHitForBubble, Mathf.Infinity, layerMask))
                {
#if UNITY_EDITOR
                    Debug.Log("clear off all the bubbles");
#endif

                    ClickEventApplyRadialForce(rayCastHitForBubble.point, ps, 100);
                }

            _currentTime = 0;
            randomTime = Random.Range(20, 25);
        }
    }

    private const string EFFECT_PARTICE_PATH_SMALL ="게임별분류/기본컨텐츠/Music/Prefab/bubble_explode_small";
    private const string EFFECT_PARTICE_PATH_BIG ="게임별분류/기본컨텐츠/Music/Prefab/bubble_explode_big";
    
    
    private void Init()
    {
        _clickEffectPoolSmall = new Stack<ParticleSystem>();
        _clickEffectPoolBig = new Stack<ParticleSystem>();
        
        _effect_Small =Resources.Load<GameObject>(EFFECT_PARTICE_PATH_SMALL).GetComponent<ParticleSystem>();
        _effect_Big =Resources.Load<GameObject>(EFFECT_PARTICE_PATH_BIG).GetComponent<ParticleSystem>();
        
        SetPool(_clickEffectPoolSmall, EFFECT_PARTICE_PATH_SMALL);
        SetPool(_clickEffectPoolBig, EFFECT_PARTICE_PATH_BIG,3);       
    }

    private ParticleSystem GetFromPool(Stack<ParticleSystem> effectPool, string path)
    {
        if (effectPool.Count < 0) GrowPool(effectPool, path);

        var currentEffect = effectPool.Pop();
        return currentEffect;
    }
    private void GrowPool(Stack<ParticleSystem> pool,string path)
    {
        ParticleSystem ps = new ParticleSystem();
        
        if (path == EFFECT_PARTICE_PATH_SMALL)
        {
             ps = Instantiate(_effect_Small, transform);
        }
        else
        {
            ps = Instantiate(_effect_Big, transform);
        }
        
        pool.Push(ps);
    }

    private void PlayParticle(Stack<ParticleSystem> effectPool,Vector3 position,string path)
    {
        var ps = GetFromPool(effectPool,path);
        if (ps != null)
        {
            ps.Stop();
            ps.gameObject.SetActive(true);

            ps.transform.position = position;

            ps.Play();
            
            StartCoroutine(ReturnToPoolAfterDelay(ps,effectPool));
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("particle is null.");
#endif
        }


    }
    private void SetPool(Stack<ParticleSystem> effectPool, string path, int poolCount = 5)
    {
    
        var prefab = Resources.Load<GameObject>(path);

        if (prefab != null)
        {
            for (int poolSize = 0; poolSize < poolCount; poolSize++)
            {
                var effect = Instantiate(prefab, transform);
                effect.SetActive(false);
                ParticleSystem ps = new ParticleSystem();
                
                effect.TryGetComponent(out ps);
                if (ps != null)
                {
                    effectPool.Push(ps);
                    
                }
               
            }
          
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"this gameObj to pool is null.");
#endif 
        }
    }
    public WaitForSeconds wait_;
    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, Stack<ParticleSystem> particlePool)
    {
        if (wait_ == null)
        {
                wait_ = new WaitForSeconds(ps.main.startLifetime.constantMax);
        }
        
        yield return wait_;
        
#if UNITY_EDITOR

#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }


    private RaycastHit rayCastHitForBubble;
    private Ray ray;
    public Vector3 hitOffset;

    private void OnClicked()
    {
        ray = _gameManager.ray_GameManager;

        var layerMask = LayerMask.GetMask("Screen");


        if (Physics.Raycast(ray, out rayCastHitForBubble, Mathf.Infinity, layerMask))
            RemoveClosestParticle(rayCastHitForBubble.point + hitOffset);


#if UNITY_EDITOR
//            Debug.Log("ray checking");
#endif
    }



    private float closestDistance;
    private int closestIndex;

   [Header("particle control")]
    public float clickRadius;

    public float setLifeTime;


    public static event Action bigBubbleEvent;

    private void RemoveClosestParticle(Vector3 position)
    {
        var closestDistance = float.MaxValue;
        ParticleSystem closestParticleSystem = null;
        var closestParticleIndex = -1;

        foreach (var ps in particleSystems)
        {
            var (index, distance) = FindClosestParticle(position, ps);

          
            if (distance < closestDistance)
            {
                //1-9-24 Unit 불일치 의심으로 /100 수행
                closestDistance = distance ;
                closestParticleSystem = ps;
                closestParticleIndex = index;
            }
        }

        // 가장 가까운 파티클을 제거합니다.
        if (closestParticleSystem != null && closestParticleIndex != -1 && closestDistance < clickRadius)
        {
            var particles = new ParticleSystem.Particle[closestParticleSystem.particleCount];
            closestParticleSystem.GetParticles(particles);

            particles[closestParticleIndex].remainingLifetime = 0; 
       
            PlayParticle(_clickEffectPoolSmall,particles[closestParticleIndex].position,EFFECT_PARTICE_PATH_SMALL);

            if (particles[closestParticleIndex].startSize > 3)
            {
                PlayParticle(_clickEffectPoolBig,particles[closestParticleIndex].position,EFFECT_PARTICE_PATH_BIG);
                bigBubbleEvent?.Invoke();
            }
            
            // 최종적으로, 파티클 배열의 길이에서 제거된 파티클을 반영합니다.
            closestParticleSystem.SetParticles(particles, particles.Length);

#if UNITY_EDITOR
            Debug.Log($"가장 가까운 파티클 제거 인덱스: {closestParticleIndex}, 최단거리 : {closestDistance}");
#endif
        }
    }
    private (int, float) FindClosestParticle(Vector3 position, ParticleSystem particleSystem)
    {
        var particles = new ParticleSystem.Particle[particleSystem.particleCount];
        var particleCount = particleSystem.GetParticles(particles);

        closestDistance = float.MaxValue;
        closestIndex = -1;

        for (var i = 0; i < particleCount; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return (closestIndex, closestDistance);
    }
    private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem, float clickRadious)
    {
        var particles = new ParticleSystem.Particle[particleSystem.particleCount];
        var particleCount = particleSystem.GetParticles(particles);

        var closestDistance = float.MaxValue;
        var closestIndex = -1;

        // 가장 가까운 파티클을 찾습니다.
        for (var i = 0; i < particleCount; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        var numParticlesAlive = particleSystem.GetParticles(particles);

        //   파티클을 제거합니다.
        if (closestIndex != -1)
        {
            for (var i = 0; i < numParticlesAlive; i++)
            {
                var distance = Vector3.Distance(position, particles[i].position);

                if (distance < clickRadious)
                {
                    particles[i].remainingLifetime = 0.8f;
                }
            }

            particleSystem.SetParticles(particles, particleCount);
        }
    }

    protected virtual void OnDestroy()
    {
        Music_GameManager.eventAfterAGetRay -= OnClicked;
    }

    protected virtual void BindEvent()
    {
        Music_GameManager.eventAfterAGetRay -= OnClicked;
        Music_GameManager.eventAfterAGetRay += OnClicked;
    }
}


// //1-9-24 사용함수 
// private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem, float clickRadious)
// {
//     var particles = new ParticleSystem.Particle[particleSystem.particleCount];
//     var particleCount = particleSystem.GetParticles(particles);
//
//     float closestDistance = float.MaxValue;
//     int closestIndex = -1;
//        
//     // 가장 가까운 파티클을 찾습니다.
//     for (int i = 0; i < particleCount; i++)
//     {
//         float distance = Vector3.Distance(position, particles[i].position);
//         if (distance < closestDistance)
//         {
//             closestDistance = distance;
//             closestIndex = i;
//         }
//     }
//
//     var numParticlesAlive = particleSystem.GetParticles(particles);
//
//     //   파티클을 제거합니다.
//     if (closestIndex != -1)
//     {
//             
//         for (var i = 0; i < numParticlesAlive; i++)
//         {
//             var distance = Vector3.Distance(position, particles[i].position);
//
//             if (distance < clickRadious) particles[i].remainingLifetime = setLifeTime;
//         }
//
//         particleSystem.SetParticles(particles, particleCount);
//     }
// }