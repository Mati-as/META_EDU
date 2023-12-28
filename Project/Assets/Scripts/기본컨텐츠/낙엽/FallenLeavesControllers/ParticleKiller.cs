using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public float maxDistanceFromSource = 10f;

    private ParticleSystem.Particle[] particles;

    private void Update()
    {
        if (particleSystem != null)
        {
            // Ensure our particle array is big enough
            int maxParticles = particleSystem.main.maxParticles;
            if (particles == null || particles.Length < maxParticles)
                particles = new ParticleSystem.Particle[maxParticles];

            // Get current particles
            int particleCount = particleSystem.GetParticles(particles);

            // Iterate through all particles
            for (int i = 0; i < particleCount; i++)
            {
                if (Vector3.Distance(particles[i].position, transform.position) > maxDistanceFromSource)
                {
                    // Set the lifetime of the particle to 0 to kill it
                    particles[i].remainingLifetime = 0;
                }
            }

            // Apply the changes back to the particle system
            particleSystem.SetParticles(particles, particleCount);
        }
    }
}
