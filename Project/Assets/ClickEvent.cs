using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ClickEvent : MonoBehaviour
{
    public float force = 10.0f;
    public float radius = 5.0f;

    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ApplyRadialForce(hit.point);
            }
        }
    }

    void ApplyRadialForce(Vector3 position)
    {
        int numParticlesAlive = particleSystem.GetParticles(particles);
        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distance = Vector3.Distance(position, particles[i].position);
            if (distance < radius)
            {
                Vector3 direction = (particles[i].position - position).normalized;
                particles[i].velocity += direction * force;
            }
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}
