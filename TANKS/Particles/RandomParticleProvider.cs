using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANKS.Particles;
internal class RandomParticleProvider : IParticleProvider
{
    Random rng = new();

    public Particle CreateParticle(ParticleEmitter emitter)
    {
        return new()
        {
            transform = new(
                emitter.Transform.Position.X + rng.NextSingle(),
                emitter.Transform.Position.Y + rng.NextSingle(),
                emitter.Transform.Rotation + rng.NextSingle() * MathF.Tau
                ),
            angularVelocity = rng.NextSingle(),
            velocity = new(rng.NextSingle() * 2 - 1, rng.NextSingle() * 2 - 1),
            color = Color.FromHSV(rng.NextSingle(), 1, 1),
            size = rng.NextSingle() * .1f + .1f,
            lifetime = rng.NextSingle() * 2 + 2,
        };
    }
}
