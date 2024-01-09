using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Music_BubbleController : MonoBehaviour
{
    private ParticleSystem[]
        particleSystems;

    [Header("Reference")] [SerializeField] private Music_GameManager _gameManager;

    public float randomTime;
    private float _currentTime;

    private void Awake()
    {
        BindEvent();
    }

    private void Start()
    {
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
                    Debug.Log("전체 버블 없애기");
#endif

                    ClickEventApplyRadialForce(rayCastHitForBubble.point, ps, 100);
                }

            _currentTime = 0;
            randomTime = Random.Range(20, 25);
        }
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

            particles[closestParticleIndex].remainingLifetime = 0; // 파티클을 즉시 제거하기 위해 수명을 0으로 설정

            // 최종적으로, 파티클 배열의 길이에서 제거된 파티클을 반영합니다.
            closestParticleSystem.SetParticles(particles, particles.Length); // 파티클 개수 감소

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

                if (distance < clickRadious) particles[i].remainingLifetime = setLifeTime;
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