using System;
using UnityEngine;
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

    
    [Header("Wind Frequency Setting")] [Space(10f)]
    [Range(0,100)]
    public float randomTimeMin;
    [Range(0,200)]
    public float randomTimeMax;
 
    [SerializeField]
    private float _randomTime;
    
    private float _elapsedTime;

    public static bool isWindBlowing;

    private void Awake()
    {
        _randomTime = Random.Range(randomTimeMin, randomTimeMax);
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
           
            
            ApplyRandomForce(center, particleSystemA);
            ApplyRandomForce(center, particleSystemB);
            ApplyRandomForce(center, particleSystemC);
            
            _elapsedTime = 0f;
            _randomTime = Random.Range(randomTimeMin, randomTimeMax);
        }
        else
        {
            isWindBlowing = false;
        }
        
        
    }
    
    //  메소드 목록   -----------------------
    public static Vector3 randomDirection; //해바라기 방향 조정에 사용.
    private void ApplyRandomForce(Vector3 position, ParticleSystem particleSystem)
    {
      
        
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        var numParticlesAlive = particleSystem.GetParticles(particles);
        
        for (var i = 0; i < numParticlesAlive; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);

            if (distance < radius)
            {
                var distanceFactor = 1f - distance / radius;

                var randomAngularVelocity = Random.Range(-10f * rotationPower * distanceFactor,
                    10f * rotationPower * distanceFactor);

                particles[i].angularVelocity = randomAngularVelocity;

                float randomForce = Random.Range(0.6f ,3.5f);
                var forceMultiplier =  randomForce / (1.0f + distance); // 거리에 반비례하는 힘
                particles[i].velocity += randomDirection * force * forceMultiplier;
            }
        }
        particleSystem.SetParticles(particles, numParticlesAlive);
      
    }
    
    
    public float clickForce = 10.0f;
    public float clickRadius = 5.0f;
    public float clickRotationPower;

    private void ClickEventApplyRadialForce(Vector3 position, ParticleSystem particleSystem)
    { 
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        var numParticlesAlive = particleSystem.GetParticles(particles);
        
        for (var i = 0; i < numParticlesAlive; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);

            if (distance < clickRadius)
            {
                Debug.Log("클립 낙엽 무빙");
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