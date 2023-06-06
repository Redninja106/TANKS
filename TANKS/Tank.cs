using SimulationFramework.Drawing;
using SimulationFramework.SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TANKS.Particles;

namespace TANKS;

internal class Tank : IGameComponent, ICollider, IInspectable
{
    const float TankWidth = .95f;
    const float TankHeight = TankWidth * (2f/3f);

    const float TurretOffset = .075f;
    const float TurretLength = 1;
    
    const float TreadWidth = TankWidth * .8f;
    const float TreadHeight = .075f;

    const float ExhaustOffset = -.2f;
    const float ExhaustWidth = .25f;
    const float ExhaustHeight = .15f;

    public float turnSpeed = 7;
    public float turretTurnSpeed = 3;
    public float acceleration = 3;
    public float drag = .25f;
    public float rotDrag = .25f;
    public float brakes = .99f;

    public Color Color = Color.Red;

    private float speed;
    private float angularVelocity;
    private Transform transform;

    private float turretRotation;
    private float zoom;

    private ParticleSystem exhaustParticles = new();
    private ParticleSystem turretParticles = new();

    public bool IsStatic => false;

    public Rectangle GetBounds()
    {
        return new(0, 0, TankWidth, TankHeight, Alignment.Center);
    }

    public ref Transform Transform => ref transform;

    public RenderLayer RenderLayer => RenderLayer.Entities;

    public void OnCollision(ICollider other, Vector2 mtv)
    {
        transform.Position += mtv;

        Vector2 tankHitNormal = default;

        Span<Vector2> normals = stackalloc Vector2[4]
        {
            transform.Forward,
            transform.Down * (TankWidth/TankHeight),
            transform.Up * (TankWidth / TankHeight),
            transform.Backward,
        };

        float minDot = float.PositiveInfinity;

        for (int i = 0; i < normals.Length; i++)
        {
            var d = Vector2.Dot(normals[i], -mtv.Normalized());
            if (d < minDot && d > 0)
            {
                minDot = d;
                tankHitNormal = normals[i];
            }
        }

        float cross = Vector3.Cross(new(tankHitNormal, 0), new(mtv.Normalized(), 0)).Z;
        transform.Rotation += cross * Time.DeltaTime;

        float dot = Vector2.Dot(-mtv.Normalized(), MathF.Sign(speed) * transform.Forward);
        dot = 1f - Math.Clamp(dot, 0, 1);
        dot *= dot;
        if (Vector2.Dot(MathF.Sign(speed) * transform.Forward, mtv) < 0)
        {
            speed *= MathF.Pow(dot, Time.DeltaTime);
            angularVelocity *= MathF.Pow(dot, Time.DeltaTime);
        }
    }

    Vector2 target = default;

    public void Render(ICanvas canvas)
    {
        exhaustParticles.Render(canvas);
        turretParticles.Render(canvas);

        SkiaInterop.GetPaint(canvas.State).BlendMode = SkiaSharp.SKBlendMode.SrcIn;

        canvas.PushState();

        canvas.ApplyTransform(transform);

        canvas.Fill(Color.Black);
        canvas.DrawRect(0, 0, TreadWidth, TreadHeight * 2 + TankHeight, Alignment.Center);

        Rectangle exhaustRect = new(-TankHeight / 2f, ExhaustOffset, ExhaustWidth, ExhaustHeight, Alignment.CenterRight);
        
        canvas.Fill(Color);
        canvas.DrawRect(exhaustRect);

        canvas.StrokeWidth(.05f);
        canvas.Stroke(Color.Black);
        canvas.DrawRect(exhaustRect);

        canvas.Fill(Color);
        canvas.DrawRect(0, 0, TankWidth, TankHeight, Alignment.Center);

        canvas.StrokeWidth(.05f);
        canvas.Stroke(Color.Black);
        canvas.DrawRect(0, 0, TankWidth - .025f, TankHeight - .025f, Alignment.Center);

        canvas.Translate(TurretOffset, 0);
        canvas.Rotate(turretRotation);

        canvas.Fill(Color);
        canvas.DrawRect(0, 0, TurretLength, .1f, Alignment.CenterLeft);

        canvas.StrokeWidth(.05f);
        canvas.Stroke(Color.Black);
        canvas.DrawRect(0, 0, TurretLength - .025f, .1f - .025f, Alignment.CenterLeft);

        canvas.Fill(Color);
        canvas.DrawCircle(0, 0, TankHeight / 2f - .025f);

        canvas.StrokeWidth(.05f);
        canvas.Stroke(Color.Black);
        canvas.DrawCircle(0, 0, TankHeight / 2f - .025f);

        canvas.PopState();
        var turretTransform = this.transform.Translated(new(TurretOffset, 0)).Rotated(turretRotation).Translated(Vector2.UnitX);

        Vector2 turretVelocity = this.transform.Forward * this.speed;
        turretVelocity += CentrifugalForce(this.transform.Position, turretTransform.Position, this.angularVelocity);
        turretVelocity += CentrifugalForce(this.transform.Translated(new(TurretOffset, 0)).Position, turretTransform.Position, turretTurnSpeed);

        var direction = turretVelocity + turretTransform.Forward * 25;
        
        if (Program.World.Collision.RayCast(turretTransform.Position, turretTransform.Forward, 100, collider => collider is not Projectile, out RayCastHit hit))
        {
            canvas.StrokeWidth(.05f);
            canvas.Stroke(Color.Red);
            canvas.DrawCircle(hit.Point + hit.normal * .1f, .1f);
        }
    }

    public void Update()
    {
        float targetTurn = 0;

        if (Keyboard.IsKeyDown(Key.A))
            targetTurn--;

        if (Keyboard.IsKeyDown(Key.D))
            targetTurn++;

        float turnDelta = MathF.Sign(targetTurn - angularVelocity);

        angularVelocity += turnDelta * turnSpeed * Time.DeltaTime;
        transform.Rotation += angularVelocity * Time.DeltaTime;
        angularVelocity *= MathF.Pow(1f - rotDrag, Time.DeltaTime);

        if (Keyboard.IsKeyDown(Key.W))
        {
            speed += acceleration * Time.DeltaTime;
            exhaustParticles.Emitter.Rate = 15;
        }
        else
        {
            exhaustParticles.Emitter.Rate = 3;
        }

        if (Keyboard.IsKeyDown(Key.S))
        {
            if (speed > 0.1)
            {
                speed *= MathF.Pow(1f - brakes, Time.DeltaTime);
            }
            else
            {
                speed -= acceleration * 0.5f * Time.DeltaTime;
            }
        }

        speed *= MathF.Pow(1f - drag, Time.DeltaTime);

        transform.Position += transform.Forward * speed * Time.DeltaTime;

        zoom -= Mouse.ScrollWheelDelta;
        zoom = Math.Clamp(zoom, -15, 10);
        float zoomFactor = MathF.Pow(1.1f, zoom);

        Vector2 forecastedPosition = transform.Position + transform.Forward * speed * zoomFactor;

        Vector2 targetMousePosition = (forecastedPosition + transform.Position + Program.MousePosition) / 3f;

        Program.Camera.Transform.Position = Vector2.Lerp(Program.Camera.Transform.Position, targetMousePosition, LerpFactor(0.001f));
        Program.Camera.Transform.Rotation = Angle.Lerp(Program.Camera.Transform.Rotation, this.transform.Rotation+MathF.PI/2f, LerpFactor(.02f));
        Program.Camera.VerticalSize = MathHelper.Lerp(Program.Camera.VerticalSize, 15 * zoomFactor, LerpFactor(.01f));

        Vector2 diff = Program.MousePosition - transform.Translated(new(TurretOffset, 0)).Position;

        float targetTurretRotation = MathF.Atan2(diff.Y, diff.X);

        float d = Time.DeltaTime * turretTurnSpeed;

        float currentRotation = turretRotation + transform.Rotation;

        float turretDiff = currentRotation;

        currentRotation = Angle.Step(currentRotation, targetTurretRotation, d);

        turretDiff = currentRotation - turretDiff;

        turretRotation = currentRotation - transform.Rotation;

        var turretTransform = this.transform.Translated(new(TurretOffset, 0)).Rotated(turretRotation).Translated(Vector2.UnitX);

        turretParticles.Emitter.transform = turretTransform;

        Vector2 turretVelocity = this.transform.Forward * this.speed;

        turretVelocity += CentrifugalForce(this.transform.Position, turretTransform.Position, this.angularVelocity);
        turretVelocity += CentrifugalForce(this.transform.Translated(new(TurretOffset, 0)).Position, turretTransform.Position, turretDiff / Time.DeltaTime);

        if (Mouse.IsButtonPressed(MouseButton.Left))
        {
            Program.World.Add(new Projectile(turretVelocity + turretTransform.Forward * 25, turretTransform));

            turretParticles.Emitter.Velocity = turretVelocity;

            turretParticles.Emitter.ParticleProvider ??= new TankTurretParticleProvider();
            turretParticles.Emitter.Burst(100);
        }

        exhaustParticles.Emitter.Velocity = this.transform.Forward * this.speed;
        exhaustParticles.Emitter.Velocity += CentrifugalForce(this.transform.Position, turretTransform.Position, this.angularVelocity);

        exhaustParticles.Emitter.transform = this.transform.Translated(new(-TankWidth / 2f, ExhaustOffset));
        exhaustParticles.Emitter.ParticleProvider ??= new TankExhaustParticleProvider();
        exhaustParticles.Update();

        turretParticles.Update();
    }

    private float LerpFactor(float factor)
    {
        return 1f - MathF.Pow(factor, Time.DeltaTime);
    }

    private Vector2 CentrifugalForce(Vector2 point, Vector2 center, float angularVelocity)
    {
        var diff = point - center;

        Vector2 perpendicular = new(diff.Y, -diff.X);

        return perpendicular * angularVelocity;
    }

    public void Layout()
    {
        ImGui.Text("HELL WORLD");
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    private static Vector2 Cross(Vector2 a, float s)
    {
        return new(s * a.Y, -s * a.X);
    }

    private static Vector2 Cross(float s, Vector2 a)
    {
        return new(-s * a.Y, s * a.X); 
    }
}