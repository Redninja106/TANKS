using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TANKS.Particles;

namespace TANKS;
internal class TankExhaustParticleProvider : IParticleProvider
{
    Random rng = new();

    public Particle CreateParticle(ParticleEmitter emitter)
    {
        return new()
        {
            transform = new(emitter.transform.Position, emitter.transform.Rotation + rng.NextSingle() * MathF.Tau),
            velocity = emitter.Velocity + Circle.Unit.GetPoint(emitter.transform.Rotation + MathF.PI + (rng.NextSingle() - .5f)),
            angularVelocity = emitter.AngularVelocity + rng.NextSingle(),
            size = rng.NextSingle() * .1f + .1f,
            color = Color.FromHSV(0, 0, rng.NextSingle() * .2f + .1f, rng.NextSingle()),
            lifetime = rng.NextSingle() * .75f + .25f,
            drag = .9f
        };
    }
}
