using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityInitializer : MonoBehaviour

{
    private ParticleSystem.Particle[] particles;
    [SerializeField] public ParticleSystem particleSystemA;
    [SerializeField] public ParticleSystem particleSystemB;
    [SerializeField] public ParticleSystem particleSystemC;
    
    [Range(0,10)]
    public float randomInitialSpeedMin;
    [Range(0,10)]
    public float randomInitialSpeedMax;
    [Range(0,10)]
    public float initializableRadius;
    
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, initializableRadius);
    }

    // Update is called once per frame
    void Update()
    {
        ApplyForce(transform.position,particleSystemA);
        ApplyForce(transform.position,particleSystemB);
        ApplyForce(transform.position,particleSystemC);
    }
   
  
    private void ApplyForce(Vector3 position, ParticleSystem particleSystem)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        var numParticlesAlive = particleSystem.GetParticles(particles);
        
        for (var i = 0; i < numParticlesAlive; i++)
        {
            var distance = Vector3.Distance(position, particles[i].position);

            if (distance < initializableRadius)
            {
              

                float randomAngularVelocity = Random.Range(randomInitialSpeedMin, randomInitialSpeedMax);
                
                randomAngularVelocity -= Time.deltaTime;
                particles[i].angularVelocity = randomAngularVelocity;
                
            }
        }
        particleSystem.SetParticles(particles, numParticlesAlive);
      
    }

}
