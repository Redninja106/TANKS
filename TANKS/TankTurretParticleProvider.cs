using TANKS.Particles;

namespace TANKS;
internal class TankTurretParticleProvider : IParticleProvider
{
    Random rng = new();

    public Particle CreateParticle(ParticleEmitter emitter)
    {
        bool isSmoke = rng.NextSingle() < (1 / 3f);
        if (isSmoke)
        {
            return new()
            {
                transform = emitter.transform,
                angularVelocity = emitter.AngularVelocity + rng.NextSingle(),
                color = Color.FromHSV(0, 0, rng.NextSingle() * .25f, rng.NextSingle() * .5f),
                size = rng.NextSingle() * .05f + .1f,
                velocity = emitter.Velocity + (emitter.transform.Forward + emitter.transform.Up * ((rng.NextSingle() - .5f) * .5f)).Normalized() * (rng.NextSingle() * 2.99f + .01f),
                lifetime = rng.NextSingle() * .25f + .05f,
                drag = .9f
            };
        }
        else
        {
            return new()
            {
                transform = emitter.transform,
                angularVelocity = emitter.AngularVelocity + rng.NextSingle(),
                color = Color.FromHSV(rng.NextSingle() / 6f, 1, 1),
                size = rng.NextSingle() * .05f + .01f,
                velocity = emitter.Velocity + (emitter.transform.Forward + emitter.transform.Up * ((rng.NextSingle() - .5f) * .5f)).Normalized() * (rng.NextSingle() * 2.99f + .01f),
                lifetime = rng.NextSingle() * .25f + .05f,
                drag = .9f,
            };
        }
    }
}