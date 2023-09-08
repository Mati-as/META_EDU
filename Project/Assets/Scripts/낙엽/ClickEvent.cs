using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ClickEvent : MonoBehaviour
{
    public float force = 10.0f;
    public float radius = 5.0f;

    public float rotationPower;
    private ParticleSystem.Particle[] particles;

    private ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) ApplyRadialForce(hit.point);
        }
    }

    private void ApplyRadialForce(Vector3 position)
    {
        var numParticlesAlive = particleSystem.GetParticles(particles);
        for (var i = 0; i < numParticlesAlive; i++)
        {
            
            var distance = Vector3.Distance(position, particles[i].position);
            
            if (distance < radius)
            {
              
                var distanceFactor = 1f - (distance / radius); 

               
                var randomAngularVelocity = Random.Range(-10f * rotationPower * distanceFactor, 10f * rotationPower * distanceFactor);
        
                particles[i].angularVelocity = randomAngularVelocity;

                var direction = (particles[i].position - position).normalized;
                var forceMultiplier = 1.0f / (1.0f + distance); // 거리에 반비례하는 힘
                particles[i].velocity += direction * force * forceMultiplier;
            }
            
            // var distance = Vector3.Distance(position, particles[i].position);
            // if (distance < radius)
            // {
            //     var randomRotation = Random.Range(0f, 1 * rotationPower);
            //     particles[i].rotation = randomRotation;
            //
            //     var direction = (particles[i].position - position).normalized;
            //     particles[i].velocity += direction * force;
            //     
            //     var randomAngularVelocity =
            //         Random.Range(-10f * rotationPower, 10f * rotationPower); // 여기서 각속도 범위를 조절할 수 있습니다.
            //     
            //     
            //     
            //     particles[i].angularVelocity = randomAngularVelocity;
            // }
        }


        for (var i = 0; i < numParticlesAlive; i++)
        {
          
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}