using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TANKS.Particles;

namespace TANKS;
internal class TrailParticleProvider : IParticleProvider
{
    Random rng = new();
    public Particle CreateParticle(ParticleEmitter emitter)
    {
        return new()
        {
            transform = emitter.transform with { Rotation = rng.NextSingle() * MathF.Tau },
            color = Color.FromHSV(0, 0, rng.NextSingle() * .1f + .7f, rng.NextSingle() * .5f + .25f),
            size = rng.NextSingle() * .05f + .05f,
            velocity = emitter.Velocity + Circle.Unit.GetPoint(rng.NextSingle() * MathF.Tau) * .0f,
            angularVelocity = emitter.AngularVelocity + rng.NextSingle() * MathF.PI / 10f,
            lifetime = rng.NextSingle(),
            drag = .00001f,
        };
    }
}
