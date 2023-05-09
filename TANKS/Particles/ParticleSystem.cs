using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANKS.Particles;
internal class ParticleSystem : IGameComponent
{
    private List<Particle> particles;
    public ParticleEmitter Emitter { get; private set; }

    public bool IsAlive => particles.Count > 0;

    public RenderLayer RenderLayer => RenderLayer.Particles;

    public ParticleSystem()
    {
        Emitter = new(this);
        particles = new();
    }

    public void Render(ICanvas canvas)
    {
        for (int i = 0; i < particles.Count; i++)
        {
            particles[i].Render(canvas);
        }
    }

    public void Update()
    {
        Emitter.Update();

        for (int i = 0; i < particles.Count; i++)
        {
            var particle = particles[i];
            if (particle.Update())
            {
                particles[i] = particle;
            }
            else
            {
                particles.RemoveAt(i);
            }
        }

        if (!IsAlive)
        {
            Program.World.Remove(this);
        }
    }

    public void Add(Particle particle)
    {
        this.particles.Add(particle);
    }
}
