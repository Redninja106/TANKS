using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal class Box : IGameComponent, ICollider, IDestructable
{
    private static ITexture? texture;

    public bool IsStatic => true;

    Transform transform;
    float width, height;
    Color color = Color.White;

    public float Strength { get; }

    public Box(float x, float y, float r, float width, float height) : this(new(x, y, r), width, height)
    {
        this.width = width;
        this.height = height;
    }

    public Box(Transform transform, float width, float height)
    {
        this.transform = transform;
        this.width = width;
        this.height = height;
        texture ??= Graphics.LoadTexture("./Assets/kenneyWoodCrate.png");
    }

    public void Render(ICanvas canvas)
    {
        canvas.ApplyTransform(transform);
        canvas.Fill(Color.Parse("#c7986b"));
        canvas.DrawTexture(texture!, 0, 0, width, height, Alignment.Center);
    }

    public void Update()
    {
    }

    public Rectangle GetBounds()
    {
        return new(0, 0, width, height, Alignment.Center);
    }

    public void OnCollision(ICollider other, Vector2 mtv)
    {
        Destroy();
    }

    public void Destroy()
    {
        Program.World.Remove(this);
    }

    public ref Transform Transform => ref this.transform;

    public RenderLayer RenderLayer => RenderLayer.World;


    interface IForce
    {
        Vector2 GetStrength(float t);
    }
}
