using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS.Particles;
internal class ParticleEmitter : IPositionable
{
    public ref Transform Transform => ref transform;

    public Transform transform;
    public Vector2 Velocity;
    public float AngularVelocity;


    ParticleSystem system;

    public IParticleProvider ParticleProvider { get; set; }
    public float Rate { get; set; } // particles/second

    private float lastParticle;

    public ParticleEmitter(ParticleSystem system)
    {
        lastParticle = Time.TotalTime;
        this.system = system;
    }

    public void Update()
    {
        float timeSinceParticle = Time.TotalTime - lastParticle;

        float freq = (1f / Rate);

        while (timeSinceParticle > freq)
        {
            Particle p = ParticleProvider.CreateParticle(this);
            system.Add(p);

            timeSinceParticle -= freq;
            lastParticle = Time.TotalTime;
        }
    }

    public void Burst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            system.Add(ParticleProvider.CreateParticle(this));
        }
    }
}
