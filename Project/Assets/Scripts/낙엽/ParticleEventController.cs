using UnityEngine;

public class ParticleEventController : MonoBehaviour
{
    private ParticleSystem.Particle[] particles;
    [SerializeField] public ParticleSystem particleSystemA;

    [SerializeField] public ParticleSystem particleSystemB;

    public float force = 10.0f;
    private readonly float radius = 987654321;
    public float rotationPower;

    public Vector3 center;

    public float randomTime;
    private float _elapsedTime;

    public static bool isWindBlowing; 

    private void Update()
    {
       
        _elapsedTime += Time.deltaTime;
       
        if (_elapsedTime > randomTime)
        {
            randomDirection = new Vector3(Random.Range(-2, 2), 0 , Random.Range(-2, 2));
            isWindBlowing = true;
            _elapsedTime = 0f;
            
            ApplyRandomForce(center, particleSystemA);
            ApplyRandomForce(center, particleSystemB);
        }
        else
        {
            isWindBlowing = false;
        }
    }
    
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

                
                var forceMultiplier = 1.0f / (1.0f + distance); // 거리에 반비례하는 힘
                particles[i].velocity += randomDirection * force * forceMultiplier;
            }
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}