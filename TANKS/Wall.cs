using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal class Wall : IGameComponent, ICollider
{
    public bool IsStatic => true;

    Transform transform;
    public float width, height;

    public Wall(float x, float y, float r, float width, float height) : this(new(x, y, r), width, height)
    {
        this.width = width;
        this.height = height;
    }

    public Wall(Transform transform, float width, float height)
    {
        this.transform = transform;
        this.width = width;
        this.height = height;
    }

    public Rectangle GetBounds()
    {
        return new(0, 0, width, height, Alignment.Center);
    }

    public void OnCollision(ICollider other, Vector2 mtv)
    {
    }

    public void Render(ICanvas canvas)
    {
        canvas.ApplyTransform(transform);
        canvas.Fill(Color.FromHSV(0, 0, .9f));
        canvas.DrawRect(0, 0, width, height, Alignment.Center);
    }

    public void Update()
    {
    }

    public ref Transform Transform => ref transform;

    public RenderLayer RenderLayer => RenderLayer.World;
}
