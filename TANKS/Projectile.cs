using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TANKS.Particles;

namespace TANKS;
internal class Projectile : IGameComponent, ICollider
{
    const float MaxAge = 15;

    public ParticleSystem trailParticles = new();

    public bool IsStatic => false;

    const float ProjectileWidth = .2f;
    const float ProjectileHeight = .1f;

    public Vector2 velocity;
    Transform transform;

    float age;

    public Projectile(Vector2 velocity, Transform transform)
    {
        this.velocity = velocity;
        this.transform = transform;

        trailParticles.Emitter.Rate = 100f;
    }

    public void Render(ICanvas canvas)
    {
        trailParticles.Render(canvas);

        canvas.ApplyTransform(transform);

        canvas.Fill(Color.Yellow);
        canvas.DrawRect(0, 0, ProjectileWidth, ProjectileHeight, Alignment.Center);

        canvas.StrokeWidth(.05f);
        canvas.Stroke(Color.Black);
        canvas.DrawRect(0, 0, ProjectileWidth, ProjectileHeight, Alignment.Center);
    }

    public void Update()
    {
        transform.Position += velocity * Time.DeltaTime;

        trailParticles.Emitter.Velocity = this.velocity;
        trailParticles.Emitter.transform = this.transform;
        trailParticles.Emitter.ParticleProvider ??= new TrailParticleProvider();

        trailParticles.Update();

        age += Time.DeltaTime;
        if (age > MaxAge)
        {
            Destroy();
        }
    }

    public Rectangle GetBounds()
    {
        return new(0, 0, ProjectileWidth, ProjectileHeight, Alignment.Center);
    }

    public void OnCollision(ICollider other, Vector2 mtv)
    {
        var explosion = new ParticleSystem();
        explosion.Emitter.ParticleProvider ??= new TankTurretParticleProvider();
        explosion.Emitter.Transform.Position = this.transform.Position + mtv;
        explosion.Emitter.transform.Forward = mtv.Normalized();
        explosion.Emitter.Burst(100);

        Program.World.Add(explosion);
        Destroy();
    }

    private void Destroy()
    {
        Program.World.Remove(this);

        this.trailParticles.Emitter.Rate = 0;
        Program.World.Add(this.trailParticles);
    }

    public ref Transform Transform => ref this.transform;

    public RenderLayer RenderLayer => RenderLayer.Projectiles;
}


class Box2 : IGameComponent, ICollider
{
    public RenderLayer RenderLayer => RenderLayer.Entities;

    public bool IsStatic => true;
    public ref Transform Transform => ref transform;

    private Transform transform;

    public Rectangle GetBounds()
    {
        return new Rectangle(0, 0, 1, 1, Alignment.Center);
    }

    public void OnCollision(ICollider other, Vector2 mtv)
    {

    }

    public void Render(ICanvas canvas)
    {
        canvas.DrawRect(0, 0, 1, 1, Alignment.Center);
    }

    public void Update()
    {
    }
}